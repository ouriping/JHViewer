namespace MarkdownContextViewer;

/// <summary>
/// Markdown 编辑命令层：UI 只负责触发，格式化逻辑集中在此（常见于 Typora / VS Code / GitLab Content Editor）。
/// 通过 SelectedText 修改，尽量保留 RichTextBox 撤销栈。
/// </summary>
internal sealed class MarkdownTextCommands
{
    private readonly RichTextBox _editor;

    public MarkdownTextCommands(RichTextBox editor)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public void Undo()
    {
        if (_editor.CanUndo)
        {
            _editor.Undo();
        }

        FocusEditor();
    }

    public void Redo()
    {
        if (_editor.CanRedo)
        {
            _editor.Redo();
        }

        FocusEditor();
    }

    public void ToggleBold() => ToggleWrap("**", "粗体文本");

    public void ToggleItalic() => ToggleWrap("*", "斜体文本");

    public void ToggleStrikethrough() => ToggleWrap("~~", "删除线文本");

    public void ToggleInlineCode() => ToggleWrap("`", "code");

    public void SetHeading(int level)
    {
        level = Math.Clamp(level, 1, 6);
        var prefix = new string('#', level) + " ";
        TransformSelectedLines(line =>
        {
            var trimmed = line.TrimStart();
            var hashes = 0;
            while (hashes < trimmed.Length && trimmed[hashes] == '#')
            {
                hashes++;
            }

            if (hashes > 0 && hashes <= 6 && hashes < trimmed.Length && trimmed[hashes] == ' ')
            {
                trimmed = trimmed[(hashes + 1)..];
            }

            return prefix + trimmed;
        });
    }

    public void ToggleQuote() => ToggleLinePrefix("> ");

    public void ToggleBulletList() => ToggleLinePrefix("- ");

    public void ToggleOrderedList()
    {
        if (SelectedLinesMatch(IsOrderedListLine))
        {
            TransformSelectedLines(StripListPrefix);
            return;
        }

        var index = 1;
        TransformSelectedLines(line => $"{index++}. {StripListPrefix(line)}");
    }

    public void ToggleTaskList() => ToggleLinePrefix("- [ ] ");

    public void InsertLink()
    {
        var selected = _editor.SelectedText;
        if (string.IsNullOrEmpty(selected))
        {
            InsertAndSelectInner("[文本](https://example.com)", startOffset: 1, length: 2);
            return;
        }

        var replacement = $"[{selected}](https://example.com)";
        var start = _editor.SelectionStart;
        _editor.SelectedText = replacement;
        _editor.SelectionStart = start + selected.Length + 3;
        _editor.SelectionLength = "https://example.com".Length;
        FocusEditor();
    }

    public void InsertImage()
    {
        InsertAndSelectInner("![说明](https://example.com/image.png)", startOffset: 2, length: 2);
    }

    public void InsertCodeBlock()
    {
        var selected = _editor.SelectedText;
        if (string.IsNullOrWhiteSpace(selected))
        {
            InsertAndSelectInner("```\ncode\n```\n", startOffset: 4, length: 4);
            return;
        }

        var start = _editor.SelectionStart;
        var block = "```\n" + selected.TrimEnd('\r', '\n') + "\n```\n";
        _editor.SelectedText = block;
        _editor.SelectionStart = start + 4;
        _editor.SelectionLength = selected.TrimEnd('\r', '\n').Length;
        FocusEditor();
    }

    public void InsertTable()
    {
        const string table =
            "| 列1 | 列2 | 列3 |\n" +
            "| --- | --- | --- |\n" +
            "|  |  |  |\n";
        InsertSnippet(table);
    }

    public void InsertHorizontalRule() => InsertSnippet("\n---\n");

    private void ToggleWrap(string marker, string placeholder)
    {
        var start = _editor.SelectionStart;
        var selected = _editor.SelectedText;

        if (!string.IsNullOrEmpty(selected)
            && selected.Length >= marker.Length * 2
            && selected.StartsWith(marker, StringComparison.Ordinal)
            && selected.EndsWith(marker, StringComparison.Ordinal))
        {
            var inner = selected[marker.Length..^marker.Length];
            _editor.SelectedText = inner;
            _editor.SelectionStart = start;
            _editor.SelectionLength = inner.Length;
            FocusEditor();
            return;
        }

        if (string.IsNullOrEmpty(selected))
        {
            var snippet = marker + placeholder + marker;
            _editor.SelectedText = snippet;
            _editor.SelectionStart = start + marker.Length;
            _editor.SelectionLength = placeholder.Length;
            FocusEditor();
            return;
        }

        _editor.SelectedText = marker + selected + marker;
        _editor.SelectionStart = start + marker.Length;
        _editor.SelectionLength = selected.Length;
        FocusEditor();
    }

    private void ToggleLinePrefix(string prefix)
    {
        TransformSelectedLines(line =>
        {
            var trimmedStart = line.TrimStart();
            var leading = line[..^trimmedStart.Length];
            if (trimmedStart.StartsWith(prefix, StringComparison.Ordinal))
            {
                return leading + trimmedStart[prefix.Length..];
            }

            return leading + prefix + StripListPrefix(trimmedStart);
        });
    }

    private bool SelectedLinesMatch(Func<string, bool> predicate)
    {
        var text = _editor.Text;
        GetSelectionLineRange(out var lineStart, out var lineEndExclusive);
        var block = text[lineStart..lineEndExclusive];
        var core = block.EndsWith('\n') ? block[..^1] : block;
        var lines = core.Length == 0 ? new[] { string.Empty } : core.Split('\n');
        return lines.All(predicate);
    }

    private static bool IsOrderedListLine(string line)
    {
        var trimmed = line.TrimStart();
        var i = 0;
        while (i < trimmed.Length && char.IsDigit(trimmed[i]))
        {
            i++;
        }

        return i > 0 && i + 1 < trimmed.Length && trimmed[i] == '.' && trimmed[i + 1] == ' ';
    }

    private void TransformSelectedLines(Func<string, string> transform)
    {
        var text = _editor.Text;
        GetSelectionLineRange(out var lineStart, out var lineEndExclusive);

        var block = text[lineStart..lineEndExclusive];
        var endsWithNewline = block.EndsWith('\n');
        var core = endsWithNewline ? block[..^1] : block;
        var lines = core.Length == 0 && !endsWithNewline
            ? [string.Empty]
            : core.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = transform(lines[i]);
        }

        var replacement = string.Join("\n", lines) + (endsWithNewline ? "\n" : string.Empty);
        _editor.SelectionStart = lineStart;
        _editor.SelectionLength = lineEndExclusive - lineStart;
        _editor.SelectedText = replacement;
        _editor.SelectionStart = lineStart;
        _editor.SelectionLength = replacement.Length - (endsWithNewline ? 1 : 0);
        FocusEditor();
    }

    private void GetSelectionLineRange(out int lineStart, out int lineEndExclusive)
    {
        var text = _editor.Text;
        var selStart = _editor.SelectionStart;
        var selEnd = selStart + _editor.SelectionLength;

        lineStart = text.LastIndexOf('\n', Math.Max(0, selStart - 1));
        lineStart = lineStart < 0 ? 0 : lineStart + 1;

        if (selEnd <= 0 || text.Length == 0)
        {
            lineEndExclusive = text.Length;
            return;
        }

        var endProbe = selEnd;
        if (endProbe > lineStart && endProbe <= text.Length && text[endProbe - 1] == '\n')
        {
            lineEndExclusive = endProbe;
            return;
        }

        var nextBreak = text.IndexOf('\n', Math.Min(endProbe, text.Length));
        lineEndExclusive = nextBreak < 0 ? text.Length : nextBreak + 1;
    }

    private static string StripListPrefix(string line)
    {
        var trimmed = line.TrimStart();
        var leading = line[..^trimmed.Length];

        if (trimmed.StartsWith("- [ ] ", StringComparison.Ordinal)
            || trimmed.StartsWith("- [x] ", StringComparison.OrdinalIgnoreCase))
        {
            return leading + trimmed[6..];
        }

        if (trimmed.StartsWith("- ", StringComparison.Ordinal)
            || trimmed.StartsWith("* ", StringComparison.Ordinal)
            || trimmed.StartsWith("> ", StringComparison.Ordinal))
        {
            return leading + trimmed[2..];
        }

        var i = 0;
        while (i < trimmed.Length && char.IsDigit(trimmed[i]))
        {
            i++;
        }

        if (i > 0 && i < trimmed.Length && trimmed[i] == '.' && i + 1 < trimmed.Length && trimmed[i + 1] == ' ')
        {
            return leading + trimmed[(i + 2)..];
        }

        return line;
    }

    private void InsertSnippet(string value)
    {
        var start = _editor.SelectionStart;
        _editor.SelectionLength = 0;
        _editor.SelectedText = value;
        _editor.SelectionStart = start + value.Length;
        _editor.SelectionLength = 0;
        FocusEditor();
    }

    private void InsertAndSelectInner(string snippet, int startOffset, int length)
    {
        var start = _editor.SelectionStart;
        _editor.SelectionLength = 0;
        _editor.SelectedText = snippet;
        _editor.SelectionStart = start + startOffset;
        _editor.SelectionLength = length;
        FocusEditor();
    }

    private void FocusEditor() => _editor.Focus();
}
