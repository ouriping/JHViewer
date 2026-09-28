namespace MarkdownWiki.Core.Models;

public sealed class WikiDocument
{
    public required string RelativePath { get; init; }

    public required string Title { get; init; }

    public required string Markdown { get; init; }

    public DateTimeOffset LastWriteTimeUtc { get; init; }
}
