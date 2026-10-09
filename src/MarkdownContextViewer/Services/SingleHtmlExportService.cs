using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace MarkdownContextViewer;

/// <summary>
/// 将目录（或单篇）Markdown 合并导出为一个自包含的 docs.html，本地图片转为 base64。
/// </summary>
public sealed class SingleHtmlExportService
{
    private static readonly Regex ImgSrcRegex = new(
        @"<img\b([^>]*?)\bsrc\s*=\s*(['""])(?<src>[^'""]+)\2([^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly PipelineHolder Pipeline = new();

    public (string outputPath, int documentCount, int embeddedImages) ExportDirectory(
        string sourceRoot,
        string? outputFilePath = null)
    {
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"目录不存在：{sourceRoot}");
        }

        var files = Directory.GetFiles(sourceRoot, "*.md", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
        {
            throw new InvalidOperationException("所选目录下未找到 .md 文件。");
        }

        var outputPath = string.IsNullOrWhiteSpace(outputFilePath)
            ? Path.Combine(sourceRoot, "docs.html")
            : outputFilePath;

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var sections = new List<(string id, string title, string relative, string body)>();
        var embeddedImages = 0;

        foreach (var file in files)
        {
            var relative = Path.GetRelativePath(sourceRoot, file).Replace('\\', '/');
            var markdown = File.ReadAllText(file, Encoding.UTF8);
            var title = ExtractTitle(markdown, Path.GetFileNameWithoutExtension(file));
            var id = MakeAnchorId(relative);
            var body = Markdown.ToHtml(markdown, Pipeline.Instance);
            var mdDir = Path.GetDirectoryName(file) ?? sourceRoot;
            (body, var imgCount) = EmbedLocalImages(body, mdDir);
            embeddedImages += imgCount;
            sections.Add((id, title, relative, body));
        }

        var html = BuildDocument(Path.GetFileName(sourceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)), sections);
        File.WriteAllText(outputPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return (outputPath, sections.Count, embeddedImages);
    }

    public (string outputPath, int documentCount, int embeddedImages) ExportSingleDocument(
        string sourceFilePath,
        string markdownText,
        string? outputFilePath = null)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new ArgumentException("缺少源文件路径。", nameof(sourceFilePath));
        }

        var dir = Path.GetDirectoryName(sourceFilePath) ?? AppContext.BaseDirectory;
        var outputPath = string.IsNullOrWhiteSpace(outputFilePath)
            ? Path.Combine(dir, "docs.html")
            : outputFilePath;

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var fileName = Path.GetFileName(sourceFilePath);
        var title = ExtractTitle(markdownText, Path.GetFileNameWithoutExtension(sourceFilePath));
        var id = MakeAnchorId(fileName);
        var body = Markdown.ToHtml(markdownText ?? string.Empty, Pipeline.Instance);
        (body, var embeddedImages) = EmbedLocalImages(body, dir);

        var html = BuildDocument(title, [(id, title, fileName, body)]);
        File.WriteAllText(outputPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return (outputPath, 1, embeddedImages);
    }

    private static (string html, int embeddedCount) EmbedLocalImages(string html, string baseDirectory)
    {
        var embeddedCount = 0;
        var result = ImgSrcRegex.Replace(html, match =>
        {
            var src = WebUtility.HtmlDecode(match.Groups["src"].Value.Trim());
            if (string.IsNullOrWhiteSpace(src)
                || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                || src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || src.StartsWith("//", StringComparison.Ordinal))
            {
                return match.Value;
            }

            var localPath = src.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                ? new Uri(src).LocalPath
                : Path.GetFullPath(Path.Combine(baseDirectory, src.Replace('/', Path.DirectorySeparatorChar)));

            if (!File.Exists(localPath))
            {
                return match.Value;
            }

            try
            {
                var bytes = File.ReadAllBytes(localPath);
                var mime = GuessMime(localPath);
                var dataUri = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
                embeddedCount++;
                var before = match.Groups[1].Value;
                var quote = match.Groups[2].Value;
                var after = match.Groups[3].Value;
                return $"<img{before}src={quote}{dataUri}{quote}{after}>";
            }
            catch
            {
                return match.Value;
            }
        });

        return (result, embeddedCount);
    }

    private static string GuessMime(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }

    private static string ExtractTitle(string markdown, string fallback)
    {
        using var reader = new StringReader(markdown ?? string.Empty);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("# ", StringComparison.Ordinal))
            {
                return trimmed[2..].Trim();
            }
        }

        return fallback;
    }

    private static string MakeAnchorId(string relativePath)
    {
        var sb = new StringBuilder("doc-");
        foreach (var ch in relativePath)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else
            {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }

    private static string BuildDocument(string siteTitle, List<(string id, string title, string relative, string body)> sections)
    {
        var toc = new StringBuilder();
        var body = new StringBuilder();
        foreach (var section in sections)
        {
            toc.AppendLine($"    <li><a href=\"#{WebUtility.HtmlEncode(section.id)}\">{WebUtility.HtmlEncode(section.title)}</a> <span class=\"path\">{WebUtility.HtmlEncode(section.relative)}</span></li>");
            body.AppendLine($@"  <section id=""{WebUtility.HtmlEncode(section.id)}"" class=""doc"">");
            body.AppendLine($"    <h1>{WebUtility.HtmlEncode(section.title)}</h1>");
            body.AppendLine($"    <p class=\"path\">{WebUtility.HtmlEncode(section.relative)}</p>");
            body.AppendLine(section.body);
            body.AppendLine("  </section>");
        }

        return $@"<!DOCTYPE html>
<html lang=""zh-CN"">
<head>
  <meta charset=""utf-8"" />
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <title>{WebUtility.HtmlEncode(siteTitle)} - docs</title>
  <style>
    :root {{ color-scheme: light; }}
    body {{ font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif; margin: 0; line-height: 1.6; color: #1f2328; }}
    .layout {{ display: grid; grid-template-columns: 280px 1fr; min-height: 100vh; }}
    nav {{ position: sticky; top: 0; height: 100vh; overflow: auto; padding: 20px 16px; background: #f6f8fa; border-right: 1px solid #d0d7de; box-sizing: border-box; }}
    nav h2 {{ margin: 0 0 12px; font-size: 16px; }}
    nav ul {{ list-style: none; padding: 0; margin: 0; }}
    nav li {{ margin: 0 0 10px; line-height: 1.35; }}
    nav a {{ color: #0969da; text-decoration: none; }}
    nav a:hover {{ text-decoration: underline; }}
    .path {{ color: #656d76; font-size: 12px; }}
    main {{ padding: 24px 32px 64px; max-width: 920px; }}
    section.doc {{ padding-bottom: 32px; margin-bottom: 32px; border-bottom: 1px solid #d0d7de; }}
    section.doc:last-child {{ border-bottom: none; }}
    img {{ max-width: 100%; height: auto; }}
    pre {{ background: #f6f8fa; padding: 12px; border-radius: 6px; overflow: auto; }}
    code {{ background: #f0f0f0; padding: 2px 4px; border-radius: 4px; }}
    table {{ border-collapse: collapse; }}
    th, td {{ border: 1px solid #ddd; padding: 6px 10px; }}
    @media (max-width: 900px) {{
      .layout {{ grid-template-columns: 1fr; }}
      nav {{ position: static; height: auto; border-right: none; border-bottom: 1px solid #d0d7de; }}
    }}
  </style>
</head>
<body>
<div class=""layout"">
  <nav>
    <h2>目录</h2>
    <ul>
{toc.ToString().TrimEnd()}
    </ul>
  </nav>
  <main>
{body.ToString().TrimEnd()}
  </main>
</div>
</body>
</html>
";
    }

    private sealed class PipelineHolder
    {
        public MarkdownPipeline Instance { get; } = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }
}
