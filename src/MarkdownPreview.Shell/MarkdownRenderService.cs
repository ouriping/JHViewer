using Markdig;

namespace MarkdownPreview.Shell;
/// <summary>
///  Markdown 渲染服务，用于将 Markdown 文本转换为 HTML。通过 Markdig 库实现。
///  也可以使用其它库进行渲染。比喻：Markdig、HtmlAgilityPack、NHtmlParser 等。当前只实现使用 Markdig 进行渲染。
/// </summary>
public sealed class MarkdownRenderService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
    /// <summary>
    /// 将 Markdown 文本渲染为 HTML。渲染成功，则返回 HTML 字符串
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public string RenderHtml(string markdown)
    {
        var htmlBody = Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
        return WrapHtml(htmlBody);
    }

    private static string WrapHtml(string body)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <meta charset=""utf-8"" />
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
