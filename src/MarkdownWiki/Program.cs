using System.Text.Json;
using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WikiOptions>(builder.Configuration.GetSection(WikiOptions.SectionName));
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<WikiOptions>>().Value;
    ApplyCommandLineOverrides(args, options);
    var contentRoot = Path.GetFullPath(options.ContentRoot, builder.Environment.ContentRootPath);
    options.ContentRoot = contentRoot;
    return options;
});
builder.Services.AddSingleton(sp => new WikiPathGuard(sp.GetRequiredService<WikiOptions>().ContentRoot));
builder.Services.AddSingleton<WikiTreeService>();
builder.Services.AddSingleton<WikiDocumentService>();
builder.Services.AddSingleton<WikiRenderService>();
builder.Services.AddSingleton<WikiMediaService>();
builder.Services.AddSingleton<WikiContentWatcher>();
builder.Services.AddSingleton<ChmHtmlBuilder>();
builder.Services.AddSingleton<ChmProjectWriter>();
builder.Services.AddSingleton<ChmCompiler>();
builder.Services.AddSingleton<ChmExportState>();
builder.Services.AddSingleton<ChmExportService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
        options.Cookie.Name = "MarkdownWiki.Admin";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
    options.Conventions.AllowAnonymousToPage("/Admin/Logout");
});
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var wikiOptionsForHost = builder.Configuration.GetSection(WikiOptions.SectionName).Get<WikiOptions>() ?? new WikiOptions();
ApplyCommandLineOverrides(args, wikiOptionsForHost);
var host = wikiOptionsForHost.AllowLan ? "*" : "127.0.0.1";
builder.WebHost.UseUrls($"http://{host}:{wikiOptionsForHost.Port}");

var app = builder.Build();

_ = app.Services.GetRequiredService<WikiContentWatcher>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// 旧路径兼容跳转
app.MapGet("/Edit", (string? path, string? help) =>
{
    var target = "/Admin/Edit";
    var q = new List<string>();
    if (!string.IsNullOrWhiteSpace(path)) q.Add("path=" + Uri.EscapeDataString(path));
    if (!string.IsNullOrWhiteSpace(help)) q.Add("help=" + Uri.EscapeDataString(help));
    if (q.Count > 0) target += "?" + string.Join("&", q);
    return Results.Redirect(target);
});
app.MapGet("/Manage", () => Results.Redirect("/Admin/Manage"));
app.MapGet("/Help", () => Results.Redirect("/Admin/Help"));

MapWikiApi(app);

app.MapGet("/files/{**path}", (string path, WikiPathGuard guard) =>
{
    try
    {
        var full = guard.ResolveSafePath(path);
        if (!File.Exists(full))
        {
            return Results.NotFound();
        }

        var contentType = GuessContentType(full);
        return Results.File(full, contentType);
    }
    catch (InvalidOperationException)
    {
        return Results.BadRequest("非法路径");
    }
});

var options = app.Services.GetRequiredService<WikiOptions>();
app.Logger.LogInformation("MarkdownWiki 已启动：http://{Host}:{Port}/  文档根：{Root}  编辑：{Edit}",
    options.AllowLan ? "0.0.0.0" : "127.0.0.1",
    options.Port,
    options.ContentRoot,
    options.EnableEdit ? "开" : "关");

app.Run();

static void MapWikiApi(WebApplication app)
{
    var api = app.MapGroup("/api");

    api.MapGet("/tree", (WikiTreeService tree) =>
        Results.Json(new { ok = true, data = tree.GetTree() }, JsonOptions()));

    api.MapGet("/docs/{**path}", (string path, WikiDocumentService docs) =>
    {
        try
        {
            return Results.Json(new { ok = true, data = docs.Get(path) }, JsonOptions());
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 404);
        }
    });

    api.MapGet("/search", (string? q, WikiDocumentService docs) =>
        Results.Json(new { ok = true, data = docs.Search(q ?? string.Empty) }, JsonOptions()));

    var adminApi = api.MapGroup("").RequireAuthorization();

    adminApi.MapPost("/export/chm", (ChmExportService export, ChmExportState state, WikiOptions wikiOptions, bool? compile, bool? includeIndex) =>
    {
        if (!wikiOptions.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        var opts = export.CreateDefaultOptions();
        opts.Compile = compile ?? true;
        opts.IncludeIndex = includeIndex ?? opts.IncludeIndex;
        if (opts.Compile == false)
        {
            opts.PreferredCompiler = "None";
        }

        var result = export.Export(opts);
        state.Set(result.ChmPath, result.BuildDirectory);
        return Results.Json(new { ok = result.Ok, data = result }, JsonOptions(), statusCode: result.Ok ? 200 : 500);
    });

    adminApi.MapGet("/export/chm/download", (ChmExportState state) =>
    {
        var (chmPath, _) = state.Get();
        if (string.IsNullOrWhiteSpace(chmPath) || !File.Exists(chmPath))
        {
            return Results.NotFound(new { ok = false, error = "尚无可用的 CHM，请先导出。" });
        }

        var name = Path.GetFileName(chmPath);
        return Results.File(chmPath, "application/vnd.ms-htmlhelp", name);
    });

    adminApi.MapGet("/export/chm/status", (ChmExportService export, ChmExportState state) =>
    {
        var detected = export.DetectCompiler();
        var last = state.Get();
        return Results.Json(new
        {
            ok = true,
            data = new
            {
                compiler = detected is null ? null : new { detected.Kind, detected.Path },
                lastChmPath = last.chmPath,
                lastBuildDirectory = last.buildDirectory
            }
        }, JsonOptions());
    });

    adminApi.MapPost("/preview", async (HttpRequest request, WikiRenderService render) =>
    {
        var body = await JsonSerializer.DeserializeAsync<PreviewBody>(request.Body, JsonOptions());
        var html = render.RenderFragment(body?.Markdown ?? string.Empty, body?.Path);
        return Results.Json(new { ok = true, data = html }, JsonOptions());
    });

    adminApi.MapPost("/media", async (HttpRequest request, WikiMediaService media, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        if (!request.HasFormContentType)
        {
            return Results.Json(new { ok = false, error = "需要 multipart/form-data。" }, JsonOptions(), statusCode: 400);
        }

        var form = await request.ReadFormAsync();
        var documentPath = form["documentPath"].ToString();
        var file = form.Files.GetFile("file");
        if (string.IsNullOrWhiteSpace(documentPath) || file is null || file.Length == 0)
        {
            return Results.Json(new { ok = false, error = "缺少 documentPath 或 file。" }, JsonOptions(), statusCode: 400);
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await media.SaveImageAsync(documentPath, stream, file.FileName, file.ContentType);
            return Results.Json(new { ok = true, data = result }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });

    adminApi.MapPut("/docs/{**path}", async (string path, HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync();
        try
        {
            docs.Save(path, content);
            return Results.Json(new { ok = true }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });

    adminApi.MapPost("/docs", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<CreateBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
        {
            return Results.Json(new { ok = false, error = "缺少 name。" }, JsonOptions(), statusCode: 400);
        }

        try
        {
            var path = docs.CreateDocument(body.Parent, body.Name);
            return Results.Json(new { ok = true, data = path }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });

    adminApi.MapPost("/folders", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<CreateBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
        {
            return Results.Json(new { ok = false, error = "缺少 name。" }, JsonOptions(), statusCode: 400);
        }

        try
        {
            var path = docs.CreateFolder(body.Parent, body.Name);
            return Results.Json(new { ok = true, data = path }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });

    adminApi.MapPatch("/nodes", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<PatchBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Path))
        {
            return Results.Json(new { ok = false, error = "缺少 path。" }, JsonOptions(), statusCode: 400);
        }

        try
        {
            string result;
            if (!string.IsNullOrWhiteSpace(body.NewName))
            {
                result = docs.Rename(body.Path, body.NewName);
            }
            else if (body.ToParent is not null)
            {
                result = docs.Move(body.Path, body.ToParent);
            }
            else
            {
                return Results.Json(new { ok = false, error = "需要 newName 或 toParent。" }, JsonOptions(), statusCode: 400);
            }

            return Results.Json(new { ok = true, data = result }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });

    adminApi.MapDelete("/nodes/{**path}", (string path, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, JsonOptions(), statusCode: 403);
        }

        try
        {
            docs.Delete(path);
            return Results.Json(new { ok = true }, JsonOptions());
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, JsonOptions(), statusCode: 400);
        }
    });
}

static JsonSerializerOptions JsonOptions() => new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

static void ApplyCommandLineOverrides(string[] args, WikiOptions options)
{
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if ((arg is "--content" or "-c") && i + 1 < args.Length)
        {
            options.ContentRoot = args[++i];
        }
        else if ((arg is "--port" or "-p") && i + 1 < args.Length && int.TryParse(args[++i], out var port))
        {
            options.Port = port;
        }
        else if (arg is "--lan")
        {
            options.AllowLan = true;
        }
        else if (arg is "--readonly")
        {
            options.EnableEdit = false;
        }
    }
}

static string GuessContentType(string path)
{
    return Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        ".css" => "text/css",
        ".js" => "application/javascript",
        ".json" => "application/json",
        ".txt" => "text/plain; charset=utf-8",
        ".md" => "text/markdown; charset=utf-8",
        _ => "application/octet-stream"
    };
}

file sealed class CreateBody
{
    public string? Parent { get; set; }
    public string? Name { get; set; }
}

file sealed class PatchBody
{
    public string? Path { get; set; }
    public string? NewName { get; set; }
    public string? ToParent { get; set; }
}

file sealed class PreviewBody
{
    public string? Path { get; set; }
    public string? Markdown { get; set; }
}
