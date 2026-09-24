namespace MarkdownContextViewer;

public sealed class SiteExportService
{
    private readonly MarkdownRenderService _markdownRenderService;

    public SiteExportService(MarkdownRenderService markdownRenderService)
    {
        _markdownRenderService = markdownRenderService;
    }

    public string ExportSingleDocument(string sourceFilePath, string markdownText, string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);

        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        var safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(fileName) ? "document" : fileName);

        var htmlFileName = $"{safeName}.html";
        var htmlPath = Path.Combine(outputRoot, htmlFileName);
        var indexPath = Path.Combine(outputRoot, "index.html");

        var html = _markdownRenderService.RenderHtml(markdownText);
        File.WriteAllText(htmlPath, html);

        var indexHtml = BuildIndexHtml(htmlFileName, Path.GetFileName(sourceFilePath));
        File.WriteAllText(indexPath, indexHtml);

        return indexPath;
    }

    private static string BuildIndexHtml(string htmlFileName, string displayName)
    {
        return $@"
<html>
<head>
  <meta charset=""utf-8"" />
  <title>Markdown 文档站点</title>
  <style>
    body {{ font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif; margin: 24px; }}
    h1 {{ margin-bottom: 16px; }}
    ul {{ line-height: 2; }}
    a {{ color: #0b57d0; text-decoration: none; }}
    a:hover {{ text-decoration: underline; }}
  </style>
</head>
<body>
  <h1>Markdown 文档站点</h1>
  <p>已导出到程序运行目录下的 <code>site-output</code> 文件夹。</p>
  <ul>
    <li><a href=""{htmlFileName}"">{displayName}</a></li>
  </ul>
</body>
</html>";
    }

    private static string MakeSafeFileName(string input)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = input.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }
}

