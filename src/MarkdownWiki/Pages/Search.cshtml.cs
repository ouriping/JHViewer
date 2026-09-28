using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages;

public class SearchModel : PageModel
{
    private readonly WikiDocumentService _docs;

    public SearchModel(WikiDocumentService docs)
    {
        _docs = docs;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public IReadOnlyList<WikiSearchHit> Hits { get; private set; } = [];

    public void OnGet()
    {
        ViewData["Title"] = "搜索";
        ViewData["Query"] = Q;
        Hits = string.IsNullOrWhiteSpace(Q) ? [] : _docs.Search(Q);
    }
}
