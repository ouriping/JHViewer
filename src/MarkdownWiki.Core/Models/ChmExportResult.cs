namespace MarkdownWiki.Core.Models;

public sealed class ChmExportResult
{
    public bool Ok { get; init; }

    public string? ChmPath { get; init; }

    public string BuildDirectory { get; init; } = string.Empty;

    public string? ProjectPath { get; init; }

    public string? CompilerUsed { get; init; }

    public bool Compiled { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<string> Logs { get; init; } = [];
}
