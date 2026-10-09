namespace MarkdownWiki.Core.Models;

public sealed class WikiDocument
{
    /// <summary>
    /// 文档相对路径，不包含文件名。例如：/pages/admin/。
    /// </summary>
    public required string RelativePath { get; init; }

    public required string Title { get; init; }
    /// <summary>
    /// Markdown 的文本内容
    /// </summary>

    public required string Markdown { get; init; }

    public DateTimeOffset LastWriteTimeUtc { get; init; }
}
