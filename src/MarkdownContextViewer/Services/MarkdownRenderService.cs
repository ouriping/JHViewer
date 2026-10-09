using System.Net;
using Markdig;

namespace MarkdownContextViewer;

public sealed class MarkdownRenderService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public string RenderHtml(string markdown) => RenderHtml(markdown, baseDirectory: null);

    /// <summary>
    /// 将 Markdown 渲染为完整 HTML。
    /// baseDirectory 非空时写入 &lt;base href&gt;，便于相对图片/链接正确加载。
    /// </summary>
    public string RenderHtml(string markdown, string? baseDirectory)
    {
        var htmlBody = Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
        return WrapHtml(htmlBody, baseDirectory);
    }

    private static string WrapHtml(string body, string? baseDirectory)
    {
        var baseTag = string.Empty;
        if (!string.IsNullOrWhiteSpace(baseDirectory) && Directory.Exists(baseDirectory))
        {
            var href = new Uri(AppendDirectorySeparator(Path.GetFullPath(baseDirectory))).AbsoluteUri;
            baseTag = $"  <base href=\"{WebUtility.HtmlEncode(href)}\" />{Environment.NewLine}";
        }

        return $@"<!DOCTYPE html>
<html>
<head>
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <meta charset=""utf-8"" />
{baseTag}  <style>
    html, body {{ margin: 0; padding: 0; background: #ffffff; color: #24292f; }}
    body {{ font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif; margin: 14px; line-height: 1.65; font-size: 14px; }}
    h1, h2, h3, h4 {{ border-bottom: 1px solid #d0d7de; padding-bottom: 0.2em; }}
    pre {{ background: #f6f8fa; padding: 12px; border-radius: 6px; overflow: auto; }}
    code {{ background: #f0f0f0; padding: 2px 4px; border-radius: 4px; font-family: Consolas, ""Courier New"", monospace; }}
    pre code {{ background: transparent; padding: 0; }}
    blockquote {{ border-left: 4px solid #d0d7de; margin: 0; padding: 0.2em 1em; color: #656d76; }}
    table {{ border-collapse: collapse; }}
    th, td {{ border: 1px solid #d0d7de; padding: 6px 10px; }}
    a {{ color: #0969da; }}
    img {{ max-width: 100%; }}
  </style>
</head>
<body>{body}</body>
</html>";
    }

    private static string AppendDirectorySeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}
