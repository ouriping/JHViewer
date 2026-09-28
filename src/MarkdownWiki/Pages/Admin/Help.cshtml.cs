using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

public class HelpModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public HelpModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public IReadOnlyList<HelpItem> Items { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Tag { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Markdown 编辑帮助（GFM）";
        var path = System.IO.Path.Combine(_env.WebRootPath, "help", "gfm-cheatsheet.json");
        if (!System.IO.File.Exists(path))
        {
            return;
        }

        await using var stream = System.IO.File.OpenRead(path);
        var items = await JsonSerializer.DeserializeAsync<List<HelpItem>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Items = items ?? [];
    }

    public sealed class HelpItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string[] Tags { get; set; } = [];
        public string Syntax { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
