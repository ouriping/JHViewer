using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;

namespace MarkdownContextViewer;

/// <summary>桌面端调用 MarkdownWiki.Core 导出 CHM。</summary>
public static class ChmDesktopExporter
{
    public static ChmExportResult Export(string contentRoot, string? outputDirectory = null, bool compile = true, bool includeIndex = true)
    {
        if (!Directory.Exists(contentRoot))
        {
            throw new DirectoryNotFoundException("文档根目录不存在：" + contentRoot);
        }

        var output = outputDirectory;
        if (string.IsNullOrWhiteSpace(output))
        {
            output = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(contentRoot)) ?? contentRoot, "chm-output");
        }

        var wikiOptions = new WikiOptions
        {
            ContentRoot = Path.GetFullPath(contentRoot),
            SiteTitle = "帮助文档",
            HomeDocument = File.Exists(Path.Combine(contentRoot, "index.md")) ? "index.md" : FindFirstMarkdown(contentRoot),
            Chm = new WikiChmOptions
            {
                OutputDirectory = Path.GetFullPath(output),
                FileName = "Help.chm",
                IncludeIndex = includeIndex,
                FullTextSearch = true,
                PreferredCompiler = compile ? "Auto" : "None"
            }
        };

        var guard = new WikiPathGuard(wikiOptions.ContentRoot);
        var tree = new WikiTreeService(guard, wikiOptions);
        var docs = new WikiDocumentService(guard, tree);
        var render = new WikiRenderService();
        var html = new ChmHtmlBuilder(render);
        var writer = new ChmProjectWriter();
        var compiler = new ChmCompiler();
        var export = new ChmExportService(guard, wikiOptions, tree, docs, html, writer, compiler);

        var opts = export.CreateDefaultOptions();
        opts.Compile = compile;
        opts.IncludeIndex = includeIndex;
        if (!compile)
        {
            opts.PreferredCompiler = "None";
        }

        return export.Export(opts);
    }

    private static string FindFirstMarkdown(string root)
    {
        var file = Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories).FirstOrDefault();
        if (file is null)
        {
            throw new InvalidOperationException("目录中没有 .md 文件。");
        }

        return Path.GetRelativePath(root, file).Replace('\\', '/');
    }
}
