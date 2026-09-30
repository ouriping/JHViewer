using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

public class ExportChmModel : PageModel
{
    private readonly ChmExportService _export;
    private readonly ChmExportState _state;
    private readonly WikiOptions _options;

    public ExportChmModel(ChmExportService export, ChmExportState state, WikiOptions options)
    {
        _export = export;
        _state = state;
        _options = options;
    }

    [BindProperty]
    public bool Compile { get; set; } = true;

    [BindProperty]
    public bool IncludeIndex { get; set; } = true;

    public string? DetectedCompiler { get; private set; }
    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? LastChmPath { get; private set; }
    public string? LastBuildDirectory { get; private set; }
    public List<string> Logs { get; private set; } = [];

    public void OnGet()
    {
        ViewData["Title"] = "导出 CHM";
        var detected = _export.DetectCompiler();
        DetectedCompiler = detected is null ? null : $"{detected.Kind} — {detected.Path}";
        IncludeIndex = _options.Chm?.IncludeIndex ?? true;
        var last = _state.Get();
        LastChmPath = last.chmPath;
        LastBuildDirectory = last.buildDirectory;
    }

    public IActionResult OnPostExport()
    {
        ViewData["Title"] = "导出 CHM";
        var opts = _export.CreateDefaultOptions();
        opts.Compile = Compile;
        opts.IncludeIndex = IncludeIndex;
        if (!Compile)
        {
            opts.PreferredCompiler = "None";
        }

        var detected = _export.DetectCompiler(opts);
        DetectedCompiler = detected is null ? null : $"{detected.Kind} — {detected.Path}";

        var result = _export.Export(opts);
        Logs = result.Logs.ToList();
        LastBuildDirectory = result.BuildDirectory;
        LastChmPath = result.ChmPath;
        _state.Set(result.ChmPath, result.BuildDirectory);

        if (result.Ok)
        {
            Message = result.Message;
        }
        else
        {
            ErrorMessage = result.Message;
        }

        return Page();
    }
}
