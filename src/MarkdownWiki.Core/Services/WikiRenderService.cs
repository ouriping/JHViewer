using System.Text.RegularExpressions;
using Markdig;

namespace MarkdownWiki.Core.Services;

public sealed class WikiRenderService
{
    private static readonly Regex MdHref = new(
        @"href=""(?<href>[^""]+\.md)(?<hash>#[^""]*)?""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MdImg = new(
        @"src=""(?<src>(?!https?:|/|data:)[^""]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>将 Markdown 转为 HTML 片段（不含完整页面外壳）。</summary>
    public string RenderFragment(string markdown, string? documentRelativePath = null)
    {
        var html = Markdown.ToHtml(markdown ?? string.Empty, _pipeline);
        html = RewriteMarkdownLinks(html, documentRelativePath);
        html = RewriteRelativeImages(html, documentRelativePath);
        return html;
    }

    private static string RewriteMarkdownLinks(string html, string? documentRelativePath)
    {
        var baseDir = string.IsNullOrEmpty(documentRelativePath)
            ? string.Empty
            : (Path.GetDirectoryName(documentRelativePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty)
                .Replace(Path.DirectorySeparatorChar, '/');

        return MdHref.Replace(html, match =>
        {
            var href = match.Groups["href"].Value.Replace('\\', '/');
            var hash = match.Groups["hash"].Success ? match.Groups["hash"].Value : string.Empty;
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase) || href.StartsWith("/"))
            {
                return match.Value;
            }

            var resolved = ResolveRelative(baseDir, href);
            return $"href=\"/?path={Uri.EscapeDataString(resolved)}{hash}\"";
        });
    }

    private static string RewriteRelativeImages(string html, string? documentRelativePath)
    {
        var baseDir = string.IsNullOrEmpty(documentRelativePath)
            ? string.Empty
            : (Path.GetDirectoryName(documentRelativePath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty)
                .Replace(Path.DirectorySeparatorChar, '/');

        return MdImg.Replace(html, match =>
        {
            var src = match.Groups["src"].Value.Replace('\\', '/');
            var resolved = ResolveRelative(baseDir, src);
            return $"src=\"/files/{resolved}\"";
        });
    }

    private static string ResolveRelative(string baseDir, string relative)
    {
        var combined = string.IsNullOrEmpty(baseDir) ? relative : $"{baseDir}/{relative}";
        var parts = new List<string>();
        foreach (var part in combined.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                continue;
            }

            parts.Add(part);
        }

        return string.Join('/', parts);
    }
}