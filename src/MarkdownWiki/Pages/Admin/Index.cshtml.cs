using MarkdownWiki.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly WikiOptions _options;

    public IndexModel(WikiOptions options)
    {
        _options = options;
    }

    public IActionResult OnGet()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        ViewData["Title"] = "管理后台";
        return Page();
    }
}
