using Markdig;

namespace MarkdownContextViewer;

public sealed class MarkdownRenderService
{
    public string RenderHtml(string markdown)
    {
        var htmlBody = Markdown.ToHtml(markdown ?? string.Empty);
        return WrapHtml(htmlBody);
    }

    private static string WrapHtml(string body)
    {
        return $@"
<html>
<head>
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
  <style>
    body {{ font-family: ""Segoe UI"", ""Microsoft YaHei"", sans-serif; margin: 18px; line-height: 1.6; }}
    pre {{ background: #f6f8fa; padding: 12px; border-radius: 6px; overflow: auto; }}
    code {{ background: #f0f0f0; padding: 2px 4px; border-radius: 4px; }}
    table {{ border-collapse: collapse; }}
    th, td {{ border: 1px solid #ddd; padding: 6px 10px; }}
  </style>
</head>
<body>{body}</body>
</html>";
    }
}

