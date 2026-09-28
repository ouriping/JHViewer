using MarkdownWiki.Core.Models;

namespace MarkdownWiki.Core.Services;

public sealed class WikiTreeService
{
    private readonly WikiPathGuard _pathGuard;
    private readonly WikiOptions _options;
    private readonly object _lock = new();
    private WikiNode? _cache;

    public WikiTreeService(WikiPathGuard pathGuard, WikiOptions options)
    {
        _pathGuard = pathGuard;
        _options = options;
    }

    public WikiNode GetTree()
    {
        lock (_lock)
        {
            return _cache ??= BuildTree();
        }
    }

    public void Invalidate()
    {
        lock (_lock)
        {
            _cache = null;
        }
    }

    private WikiNode BuildTree()
    {
        var root = new WikiNode
        {
            Name = _options.SiteTitle,
            RelativePath = string.Empty,
            Kind = WikiNodeKind.Folder
        };

        FillChildren(root, _pathGuard.ContentRoot);
        return root;
    }

    private void FillChildren(WikiNode parent, string directory)
    {
        var ignored = new HashSet<string>(_options.IgnoredDirectories ?? [], StringComparer.OrdinalIgnoreCase);

        IEnumerable<string> dirs;
        IEnumerable<string> files;
        try
        {
            dirs = Directory.EnumerateDirectories(directory);
            files = Directory.EnumerateFiles(directory, "*.md");
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        foreach (var dir in dirs.OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(dir);
            if (ignored.Contains(name) || name.StartsWith('.'))
            {
                continue;
            }

            var node = new WikiNode
            {
                Name = name,
                RelativePath = _pathGuard.ToRelativePosix(dir),
                Kind = WikiNodeKind.Folder
            };
            FillChildren(node, dir);
            parent.Children.Add(node);
        }

        foreach (var file in files.OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith('.'))
            {
                continue;
            }

            parent.Children.Add(new WikiNode
            {
                Name = Path.GetFileNameWithoutExtension(name),
                RelativePath = _pathGuard.ToRelativePosix(file),
                Kind = WikiNodeKind.Document
            });
        }
    }
}
