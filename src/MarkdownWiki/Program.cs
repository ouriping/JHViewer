using System.Text.Json;
using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
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
builder.Services.AddSingleton<WikiContentWatcher>();
builder.Services.AddRazorPages();
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
app.MapRazorPages();

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

    api.MapPut("/docs/{**path}", async (string path, HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, statusCode: 403);
        }

        using var reader = new StreamReader(request.Body);
        var content = await reader.ReadToEndAsync();
        try
        {
            docs.Save(path, content);
            return Results.Json(new { ok = true });
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, statusCode: 400);
        }
    });

    api.MapPost("/docs", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<CreateBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
        {
            return Results.Json(new { ok = false, error = "缺少 name。" }, statusCode: 400);
        }

        try
        {
            var path = docs.CreateDocument(body.Parent, body.Name);
            return Results.Json(new { ok = true, data = path });
        }
        catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, statusCode: 400);
        }
    });

    api.MapPost("/folders", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<CreateBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Name))
        {
            return Results.Json(new { ok = false, error = "缺少 name。" }, statusCode: 400);
        }

        try
        {
            var path = docs.CreateFolder(body.Parent, body.Name);
            return Results.Json(new { ok = true, data = path });
        }
        catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, statusCode: 400);
        }
    });

    api.MapPatch("/nodes", async (HttpRequest request, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, statusCode: 403);
        }

        var body = await JsonSerializer.DeserializeAsync<PatchBody>(request.Body, JsonOptions());
        if (body is null || string.IsNullOrWhiteSpace(body.Path))
        {
            return Results.Json(new { ok = false, error = "缺少 path。" }, statusCode: 400);
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
                return Results.Json(new { ok = false, error = "需要 newName 或 toParent。" }, statusCode: 400);
            }

            return Results.Json(new { ok = true, data = result });
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, statusCode: 400);
        }
    });

    api.MapDelete("/nodes/{**path}", (string path, WikiDocumentService docs, WikiOptions options) =>
    {
        if (!options.EnableEdit)
        {
            return Results.Json(new { ok = false, error = "当前为只读模式。" }, statusCode: 403);
        }

        try
        {
            docs.Delete(path);
            return Results.Json(new { ok = true });
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException)
        {
            return Results.Json(new { ok = false, error = ex.Message }, statusCode: 400);
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
