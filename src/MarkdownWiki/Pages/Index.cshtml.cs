using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages;

public class IndexModel : PageModel
{
    private readonly WikiDocumentService _docs;
    private readonly WikiRenderService _render;
    private readonly WikiOptions _options;

    public IndexModel(WikiDocumentService docs, WikiRenderService render, WikiOptions options)
    {
        _docs = docs;
        _render = render;
        _options = options;
    }

    public string CurrentPath { get; private set; } = string.Empty;
    public string HtmlContent { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public bool EnableEdit => _options.EnableEdit;

    public IActionResult OnGet([FromQuery] string? path)
    {
        var target = string.IsNullOrWhiteSpace(path) ? _options.HomeDocument : path;
        try
        {
            var doc = _docs.Get(target);
            CurrentPath = doc.RelativePath;
            HtmlContent = _render.RenderFragment(doc.Markdown, doc.RelativePath);
            ViewData["Title"] = doc.Title;
            return Page();
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
            CurrentPath = target;
            ViewData["Title"] = "未找到";
            return Page();
        }
    }
}
