namespace MarkdownContextViewer;

public sealed class DirectorySiteExportService
{
    private readonly MarkdownRenderService _markdownRenderService;

    public DirectorySiteExportService(MarkdownRenderService markdownRenderService)
    {
        _markdownRenderService = markdownRenderService;
    }

    public (string indexPath, int count) ExportDirectory(string sourceRoot, string outputRoot)
    {
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"目录不存在：{sourceRoot}");
        }

        Directory.CreateDirectory(outputRoot);

        var markdownFiles = Directory.GetFiles(sourceRoot, "*.md", SearchOption.AllDirectories);
        var links = new List<(string title, string href)>();

        foreach (var file in markdownFiles)
        {
            var markdown = File.ReadAllText(file);
            var html = _markdownRenderService.RenderHtml(markdown);

            var relative = Path.GetRelativePath(sourceRoot, file);
            var htmlRelative = Path.ChangeExtension(relative, ".html");
            var htmlPath = Path.Combine(outputRoot, htmlRelative);

            var htmlDir = Path.GetDirectoryName(htmlPath);
            if (!string.IsNullOrWhiteSpace(htmlDir))
            {
                Directory.CreateDirectory(htmlDir);
            }

            File.WriteAllText(htmlPath, html);

            var href = htmlRelative.Replace(Path.DirectorySeparatorChar, '/');
            links.Add((relative, href));
        }

        var indexPath = Path.Combine(outputRoot, "index.html");
        File.WriteAllText(indexPath, BuildIndexHtml(links));

        return (indexPath, links.Count);
    }

    private static string BuildIndexHtml(List<(string title, string href)> links)
    {
        var listItems = links.Count == 0
            ? "<li>未发现 .md 文件</li>"
            : string.Join(Environment.NewLine, links.Select(link => $"<li><a href=\"{link.href}\">{link.title}</a></li>"));

        return $@"
<html>
<head>
  <meta charset=""utf-8"" />
  <title>Markdown 目录站点</title>
  <style>
    body {{ font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif; margin: 24px; }}
    h1 {{ margin-bottom: 16px; }}
    ul {{ line-height: 2; }}
    a {{ color: #0b57d0; text-decoration: none; }}
    a:hover {{ text-decoration: underline; }}
  </style>
</head>
<body>
  <h1>Markdown 目录站点</h1>
  <ul>
    {listItems}
  </ul>
</body>
</html>";
    }
}

