using Markdig;

namespace MarkdownPreview.Handler;

/// <summary>
/// Markdown 转 HTML 的渲染器。
/// Markdig 本身会忽略 HTML头部，这里把 HTML 内联到样式中。
/// </summary>
internal static class MarkdownHtmlRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml() // 降低 XSS：忽略源文件中的原始 HTML
        .Build();

    public static string Render(string markdown)
    {
        var body = Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
        return Wrap(body);
    }

    private static string Wrap(string body)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <style>
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
}
