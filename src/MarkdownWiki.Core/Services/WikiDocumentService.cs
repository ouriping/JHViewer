using System.Text;
using System.Text.RegularExpressions;
using MarkdownWiki.Core.Models;

namespace MarkdownWiki.Core.Services;

public sealed class WikiDocumentService
{
    private static readonly Regex HeadingRegex = new(@"^\s*#\s+(.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly WikiPathGuard _pathGuard;
    private readonly WikiTreeService _treeService;

    public WikiDocumentService(WikiPathGuard pathGuard, WikiTreeService treeService)
    {
        _pathGuard = pathGuard;
        _treeService = treeService;
    }

    public WikiDocument Get(string relativePath)
    {
        var full = _pathGuard.ResolveSafePath(relativePath, expectDocument: true);
        if (!File.Exists(full))
        {
            throw new FileNotFoundException("文档不存在。", relativePath);
        }

        var markdown = File.ReadAllText(full, Encoding.UTF8);
        return new WikiDocument
        {
            RelativePath = _pathGuard.ToRelativePosix(full),
            Title = ExtractTitle(markdown, Path.GetFileNameWithoutExtension(full)),
            Markdown = markdown,
            LastWriteTimeUtc = File.GetLastWriteTimeUtc(full)
        };
    }

    public void Save(string relativePath, string content)
    {
        var full = _pathGuard.ResolveSafePath(relativePath, expectDocument: true);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var temp = full + ".tmp";
        File.WriteAllText(temp, content ?? string.Empty, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.Move(temp, full, overwrite: true);
        _treeService.Invalidate();
    }

    public string CreateDocument(string? parentRelative, string name)
    {
        WikiPathGuard.ValidateSegmentName(name);
        if (!name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            name += ".md";
        }

        var parent = string.IsNullOrWhiteSpace(parentRelative)
            ? _pathGuard.ContentRoot
            : _pathGuard.ResolveSafePath(parentRelative);

        if (!Directory.Exists(parent))
        {
            throw new DirectoryNotFoundException("父目录不存在。");
        }

        var full = Path.Combine(parent, name);
        var relative = _pathGuard.ToRelativePosix(full);
        _ = _pathGuard.ResolveSafePath(relative, expectDocument: true);

        if (File.Exists(full))
        {
            throw new InvalidOperationException("文档已存在。");
        }

        var title = Path.GetFileNameWithoutExtension(name);
        File.WriteAllText(full, $"# {title}{Environment.NewLine}{Environment.NewLine}", new UTF8Encoding(false));
        _treeService.Invalidate();
        return relative;
    }
    /// <summary>
    /// 创建文档时，验证文件名是否合法。非法则抛出。
    /// </summary>
    /// <param name="parentRelative"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public string CreateFolder(string? parentRelative, string name)
    {
        WikiPathGuard.ValidateSegmentName(name);

        var parent = string.IsNullOrWhiteSpace(parentRelative)
            ? _pathGuard.ContentRoot
            : _pathGuard.ResolveSafePath(parentRelative);

        if (!Directory.Exists(parent))
        {
            throw new DirectoryNotFoundException("父目录不存在。");
        }

        var full = Path.Combine(parent, name);
        if (Directory.Exists(full) || File.Exists(full))
        {
            throw new InvalidOperationException("文件夹已存在。");
        }

        Directory.CreateDirectory(full);
        _treeService.Invalidate();
        return _pathGuard.ToRelativePosix(full);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="relativePath"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public string Rename(string relativePath, string newName)
    {
        WikiPathGuard.ValidateSegmentName(newName);
        var full = _pathGuard.ResolveSafePath(relativePath);
        var parent = Path.GetDirectoryName(full) ?? _pathGuard.ContentRoot;

        string target;
        if (File.Exists(full))
        {
            if (!newName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                newName += ".md";
            }

            target = Path.Combine(parent, newName);
            if (File.Exists(target))
            {
                throw new InvalidOperationException("目标文件已存在。");
            }

            File.Move(full, target);
        }
        else if (Directory.Exists(full))
        {
            target = Path.Combine(parent, newName);
            if (Directory.Exists(target))
            {
                throw new InvalidOperationException("目标文件夹已存在。");
            }

            Directory.Move(full, target);
        }
        else
        {
            throw new FileNotFoundException("路径不存在。", relativePath);
        }

        _treeService.Invalidate();
        return _pathGuard.ToRelativePosix(target);
    }

    public string Move(string relativePath, string? toParentRelative)
    {
        var full = _pathGuard.ResolveSafePath(relativePath);
        var toParent = string.IsNullOrWhiteSpace(toParentRelative)
            ? _pathGuard.ContentRoot
            : _pathGuard.ResolveSafePath(toParentRelative);

        if (!Directory.Exists(toParent))
        {
            throw new DirectoryNotFoundException("目标父目录不存在。");
        }

        var name = Path.GetFileName(full);
        var target = Path.Combine(toParent, name);

        if (File.Exists(full))
        {
            if (File.Exists(target))
            {
                throw new InvalidOperationException("目标已存在同名文件。");
            }

            File.Move(full, target);
        }
        else if (Directory.Exists(full))
        {
            if (Directory.Exists(target) || File.Exists(target))
            {
                throw new InvalidOperationException("目标已存在同名项。");
            }

            // 禁止移入自身子目录
            var targetFull = Path.GetFullPath(target);
            var sourceFull = Path.GetFullPath(full);
            if (targetFull.StartsWith(sourceFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("不能将文件夹移动到其子目录中。");
            }

            Directory.Move(full, target);
        }
        else
        {
            throw new FileNotFoundException("路径不存在。", relativePath);
        }

        _treeService.Invalidate();
        return _pathGuard.ToRelativePosix(target);
    }

    public void Delete(string relativePath)
    {
        var full = _pathGuard.ResolveSafePath(relativePath);
        if (File.Exists(full))
        {
            File.Delete(full);
        }
        else if (Directory.Exists(full))
        {
            if (Directory.EnumerateFileSystemEntries(full).Any())
            {
                throw new InvalidOperationException("只能删除空文件夹，请先清空内容。");
            }

            Directory.Delete(full);
        }
        else
        {
            throw new FileNotFoundException("路径不存在。", relativePath);
        }

        _treeService.Invalidate();
    }

    public IReadOnlyList<WikiSearchHit> Search(string query, int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var q = query.Trim();
        var hits = new List<WikiSearchHit>();
        foreach (var file in Directory.EnumerateFiles(_pathGuard.ContentRoot, "*.md", SearchOption.AllDirectories))
        {
            var relative = _pathGuard.ToRelativePosix(file);
            if (relative.StartsWith("_meta/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string text;
            try
            {
                text = File.ReadAllText(file, Encoding.UTF8);
            }
            catch
            {
                continue;
            }

            var title = ExtractTitle(text, Path.GetFileNameWithoutExtension(file));
            var nameMatch = relative.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                            title.Contains(q, StringComparison.OrdinalIgnoreCase);
            var idx = text.IndexOf(q, StringComparison.OrdinalIgnoreCase);
            if (!nameMatch && idx < 0)
            {
                continue;
            }

            var snippet = BuildSnippet(text, idx >= 0 ? idx : 0, q.Length);
            hits.Add(new WikiSearchHit
            {
                RelativePath = relative,
                Title = title,
                Snippet = snippet
            });

            if (hits.Count >= maxResults)
            {
                break;
            }
        }

        return hits;
    }

    public static string ExtractTitle(string markdown, string fallback)
    {
        var match = HeadingRegex.Match(markdown ?? string.Empty);
        return match.Success ? match.Groups[1].Value.Trim() : fallback;
    }

    private static string BuildSnippet(string text, int index, int length)
    {
        var start = Math.Max(0, index - 40);
        var end = Math.Min(text.Length, index + Math.Max(length, 1) + 80);
        var snippet = text[start..end].Replace('\r', ' ').Replace('\n', ' ').Trim();
        if (start > 0)
        {
            snippet = "…" + snippet;
        }

        if (end < text.Length)
        {
            snippet += "…";
        }

        return snippet;
    }
}
