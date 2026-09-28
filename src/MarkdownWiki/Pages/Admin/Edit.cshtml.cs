using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

public class EditModel : PageModel
{
    private readonly WikiDocumentService _docs;
    private readonly WikiOptions _options;

    public EditModel(WikiDocumentService docs, WikiOptions options)
    {
        _docs = docs;
        _options = options;
    }

    [BindProperty(SupportsGet = true)]
    public string? Path { get; set; }

    [BindProperty(SupportsGet = true, Name = "help")]
    public string? HelpFocus { get; set; }

    [BindProperty]
    public string Markdown { get; set; } = string.Empty;

    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        Path ??= _options.HomeDocument;
        try
        {
            var doc = _docs.Get(Path);
            Path = doc.RelativePath;
            Markdown = doc.Markdown;
            ViewData["Title"] = "编辑 - " + doc.Title;
            return Page();
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
        {
            ErrorMessage = ex.Message;
            ViewData["Title"] = "编辑文档";
            return Page();
        }
    }

    public IActionResult OnPostSave()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        try
        {
            _docs.Save(Path ?? string.Empty, Markdown);
            Message = "已保存。";
            ViewData["Title"] = "编辑文档";
            return Page();
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or DirectoryNotFoundException)
        {
            ErrorMessage = ex.Message;
            ViewData["Title"] = "编辑文档";
            return Page();
        }
    }
}
