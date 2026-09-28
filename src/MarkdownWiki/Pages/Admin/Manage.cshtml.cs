using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

public class ManageModel : PageModel
{
    private readonly WikiDocumentService _docs;
    private readonly WikiOptions _options;

    public ManageModel(WikiDocumentService docs, WikiOptions options)
    {
        _docs = docs;
        _options = options;
    }

    [BindProperty]
    public string? Parent { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? TargetPath { get; set; }

    [BindProperty]
    public string? NewName { get; set; }

    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        ViewData["Title"] = "文档管理";
        return Page();
    }

    public IActionResult OnPostCreateDoc() => Run(() =>
    {
        var path = _docs.CreateDocument(Parent, Name ?? "new");
        Message = $"已创建文档：{path}";
    });

    public IActionResult OnPostCreateFolder() => Run(() =>
    {
        var path = _docs.CreateFolder(Parent, Name ?? "folder");
        Message = $"已创建文件夹：{path}";
    });

    public IActionResult OnPostRename() => Run(() =>
    {
        var path = _docs.Rename(TargetPath ?? string.Empty, NewName ?? string.Empty);
        Message = $"已重命名为：{path}";
    });

    public IActionResult OnPostDelete() => Run(() =>
    {
        _docs.Delete(TargetPath ?? string.Empty);
        Message = $"已删除：{TargetPath}";
    });

    private IActionResult Run(Action action)
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        try
        {
            action();
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException or DirectoryNotFoundException)
        {
            ErrorMessage = ex.Message;
        }

        ViewData["Title"] = "文档管理";
        return Page();
    }
}
