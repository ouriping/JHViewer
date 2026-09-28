namespace MarkdownWiki.Core.Models;

public sealed class WikiMediaResult
{
    /// <summary>写入磁盘后的相对文档根路径（POSIX）。</summary>
    public required string RelativePath { get; init; }

    /// <summary>建议插入 Markdown 的相对当前文档路径。</summary>
    public required string MarkdownPath { get; init; }

    /// <summary>Web 预览 URL（/files/...）。</summary>
    public required string Url { get; init; }

    public required string ContentType { get; init; }

    public long SizeBytes { get; init; }
}
