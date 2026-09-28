using System.Text.RegularExpressions;

namespace MarkdownWiki.Core.Services;

/// <summary>将相对路径限制在文档根内，防止路径穿越。</summary>
public sealed class WikiPathGuard
{
    private static readonly Regex InvalidName = new(@"[\\/:*?""<>|\x00-\x1F]", RegexOptions.Compiled);

    private readonly string _contentRoot;

    public WikiPathGuard(string contentRoot)
    {
        _contentRoot = Path.GetFullPath(contentRoot);
        Directory.CreateDirectory(_contentRoot);
    }

    public string ContentRoot => _contentRoot;

    /// <summary>将相对路径解析为绝对路径；非法则抛出。</summary>
    public string ResolveSafePath(string? relativePath, bool expectDocument = false)
    {
        var normalized = NormalizeRelative(relativePath);
        if (expectDocument && !normalized.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("仅允许操作 .md 文档。");
        }

        var combined = Path.GetFullPath(Path.Combine(_contentRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!combined.StartsWith(_contentRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("路径越界，禁止访问文档根以外的文件。");
        }

        return combined;
    }
    /// <summary>
    /// 将绝对路径转换为相对路径，相对于文档根。非法则抛出。
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string ToRelativePosix(string fullPath)
    {
        var full = Path.GetFullPath(fullPath);
        if (!full.StartsWith(_contentRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("路径越界。");
        }

        var relative = Path.GetRelativePath(_contentRoot, full);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }
    /// <summary>
    /// 将相对路径规范化，去除不必要的 ./ 和 ..，防止路径穿越。非法则抛出。
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string NormalizeRelative(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var path = relativePath.Replace('\\', '/').Trim();
        while (path.StartsWith("./", StringComparison.Ordinal))
        {
            path = path[2..];
        }

        path = path.TrimStart('/');

        if (path.Contains(':', StringComparison.Ordinal) ||
            path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(s => s is "." or ".."))
        {
            throw new InvalidOperationException("非法相对路径。");
        }

        return path;
    }
    /// <summary>
    /// 验证路径段名称，防止路径穿越。非法则抛出。
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateSegmentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || InvalidName.IsMatch(name))
        {
            throw new InvalidOperationException("名称非法。");
        }
    }
}
