namespace MarkdownWiki.Core.Models;

public enum WikiNodeKind
{
    Folder,
    Document
}

public sealed class WikiNode
{
    public required string Name { get; init; }

    /// <summary>相对文档根的 POSIX 路径；文件夹不以 / 结尾。</summary>
    public required string RelativePath { get; init; }

    public required WikiNodeKind Kind { get; init; }

    public List<WikiNode> Children { get; init; } = [];
}
