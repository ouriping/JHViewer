namespace MarkdownWiki.Pages.Shared;

public sealed class MarkdownEditorViewModel
{
    public required string DocumentPath { get; init; }

    public required string Markdown { get; init; }

    public string TextAreaName { get; init; } = "Markdown";

    public string TextAreaId { get; init; } = "Markdown";

    /// <summary>可选：打开时定位到某条帮助（gfm-cheatsheet.json 的 id）。</summary>
    public string? HelpFocusId { get; init; }
}
