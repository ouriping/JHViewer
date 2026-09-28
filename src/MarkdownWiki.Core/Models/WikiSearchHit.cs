namespace MarkdownWiki.Core.Models;

public sealed class WikiSearchHit
{
    public required string RelativePath { get; init; }

    public required string Title { get; init; }

    public required string Snippet { get; init; }
}
