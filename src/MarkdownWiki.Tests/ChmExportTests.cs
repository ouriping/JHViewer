using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;

namespace MarkdownWiki.Tests;

public class ChmExportTests
{
    [Fact]
    public void MakeRelative_computes_parent_paths()
    {
        Assert.Equal("../../css/chm.css", ChmHtmlBuilder.MakeRelative("html/guide", "css/chm.css"));
        Assert.Equal("../css/chm.css", ChmHtmlBuilder.MakeRelative("html", "css/chm.css"));
        Assert.Equal("faq.htm", ChmHtmlBuilder.MakeRelative("html/guide", "html/guide/faq.htm"));
        Assert.Equal("../index.htm", ChmHtmlBuilder.MakeRelative("html/guide", "html/index.htm"));
    }

    [Fact]
    public void Export_generates_hhp_hhc_and_html_without_compiler()
    {
        var root = Path.Combine(Path.GetTempPath(), "markdown-wiki-chm", Guid.NewGuid().ToString("N"));
        var outDir = Path.Combine(root, "out");
        Directory.CreateDirectory(Path.Combine(root, "guide"));
        File.WriteAllText(Path.Combine(root, "index.md"), "# Home\n\nSee [FAQ](guide/faq.md).");
        File.WriteAllText(Path.Combine(root, "guide", "faq.md"), "# FAQ\n\n## Detail\n\nAnswer.");

        try
        {
            var wikiOptions = new WikiOptions { ContentRoot = root, SiteTitle = "Test Help", HomeDocument = "index.md" };
            var guard = new WikiPathGuard(root);
            var tree = new WikiTreeService(guard, wikiOptions);
            var docs = new WikiDocumentService(guard, tree);
            var render = new WikiRenderService();
            var html = new ChmHtmlBuilder(render);
            var writer = new ChmProjectWriter();
            var compiler = new ChmCompiler();
            var export = new ChmExportService(guard, wikiOptions, tree, docs, html, writer, compiler);

            var result = export.Export(new ChmExportOptions
            {
                Title = "Test Help",
                HomeDocument = "index.md",
                OutputDirectory = outDir,
                FileName = "Test.chm",
                PreferredCompiler = "None",
                Compile = false,
                IncludeIndex = true
            });

            Assert.True(result.Ok, result.Message);
            Assert.False(result.Compiled);
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "Project.hhp")));
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "TableOfContents.hhc")));
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "Index.hhk")));
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "html", "index.htm")));
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "html", "guide", "faq.htm")));
            Assert.True(File.Exists(Path.Combine(result.BuildDirectory, "css", "chm.css")));

            var faqHtml = File.ReadAllText(Path.Combine(result.BuildDirectory, "html", "index.htm"));
            Assert.Contains("guide/faq.htm", faqHtml.Replace('\\', '/'));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
