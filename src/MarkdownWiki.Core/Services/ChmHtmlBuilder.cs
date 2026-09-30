using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownWiki.Core.Services;

/// <summary>将 Markdown 渲染为适合 CHM 的完整 HTML 页面（相对 .htm 链接）。</summary>
public sealed class ChmHtmlBuilder
{
    private static readonly Regex HeadingRegex = new(
        @"^\s{0,3}(?<level>#{1,6})\s+(?<text>.+?)\s*#*\s*$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly WikiRenderService _render;

    public ChmHtmlBuilder(WikiRenderService render)
    {
        _render = render;
    }

    public string BuildPage(string markdown, string documentRelativePath, string pageTitle, string siteTitle)
    {
        var fragment = RenderChmFragment(markdown, documentRelativePath);
        var cssHref = MakeRelative(GetHtmlDir(documentRelativePath), "css/chm.css").Replace('\\', '/');
        var safeTitle = WebUtility.HtmlEncode(pageTitle);
        var safeSite = WebUtility.HtmlEncode(siteTitle);

        return $@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>
<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
  <title>{safeTitle} - {safeSite}</title>
  <link rel=""stylesheet"" type=""text/css"" href=""{cssHref}"" />
</head>
<body>
  <div class=""markdown-body"">
{fragment}
  </div>
</body>
</html>
";
    }

    public string RenderChmFragment(string markdown, string documentRelativePath)
    {
        var html = _render.RenderFragment(markdown, documentRelativePath);
        html = RewriteWikiLinksToRelativeHtm(html, documentRelativePath);
        html = RewriteFilesUrlsToRelative(html, documentRelativePath);
        return html;
    }

    public static IReadOnlyList<(string text, int level)> ExtractHeadings(string markdown)
    {
        var list = new List<(string, int)>();
        foreach (Match match in HeadingRegex.Matches(markdown ?? string.Empty))
        {
            var level = match.Groups["level"].Value.Length;
            var text = match.Groups["text"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                list.Add((text, level));
            }
        }

        return list;
    }

    public static string ToHtmRelativeFromRoot(string mdRelativePath)
    {
        var posix = mdRelativePath.Replace('\\', '/');
        if (!posix.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            posix += ".md";
        }

        return ("html/" + Path.ChangeExtension(posix, ".htm")).Replace('\\', '/');
    }

    private static string RewriteWikiLinksToRelativeHtm(string html, string documentRelativePath)
    {
        var fromDir = GetHtmlDir(documentRelativePath);
        return Regex.Replace(html,
            @"href=""/\?path=(?<path>[^""#]+)(?<hash>#[^""]*)?""",
            match =>
            {
                var posix = Uri.UnescapeDataString(match.Groups["path"].Value).Replace('\\', '/');
                var target = ToHtmRelativeFromRoot(posix);
                var relative = MakeRelative(fromDir, target).Replace('\\', '/');
                var hash = match.Groups["hash"].Success ? match.Groups["hash"].Value : string.Empty;
                return $"href=\"{relative}{hash}\"";
            },
            RegexOptions.IgnoreCase);
    }

    private static string RewriteFilesUrlsToRelative(string html, string documentRelativePath)
    {
        var fromDir = GetHtmlDir(documentRelativePath);
        return Regex.Replace(html,
            @"src=""/files/(?<path>[^""]+)""",
            match =>
            {
                var posix = Uri.UnescapeDataString(match.Groups["path"].Value).Replace('\\', '/');
                var target = "html/" + posix;
                var relative = MakeRelative(fromDir, target).Replace('\\', '/');
                return $"src=\"{relative}\"";
            },
            RegexOptions.IgnoreCase);
    }

    private static string GetHtmlDir(string documentRelativePath)
    {
        var posix = documentRelativePath.Replace('\\', '/');
        var dir = Path.GetDirectoryName(posix)?.Replace('\\', '/') ?? string.Empty;
        return string.IsNullOrEmpty(dir) ? "html" : "html/" + dir;
    }

    public static string MakeRelative(string fromDir, string toPath)
    {
        var fromParts = fromDir.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        var toParts = toPath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

        var i = 0;
        while (i < fromParts.Count && i < toParts.Count - 1 &&
               string.Equals(fromParts[i], toParts[i], StringComparison.OrdinalIgnoreCase))
        {
            i++;
        }

        var sb = new StringBuilder();
        for (var u = 0; u < fromParts.Count - i; u++)
        {
            sb.Append("../");
        }

        sb.Append(string.Join('/', toParts.Skip(i)));
        var result = sb.ToString();
        return string.IsNullOrEmpty(result) ? toParts[^1] : result;
    }
}
