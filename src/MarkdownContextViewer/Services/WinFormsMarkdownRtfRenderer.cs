using System.Text;
using Markdig;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;

namespace MarkdownContextViewer;

/// <summary>
/// 将 Markdown（Markdig AST）渲染到 RichTextBox，覆盖标题/列表/表格/代码/引用等常用结构。
/// </summary>
public static class WinFormsMarkdownRtfRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static readonly Color TextColor = Color.FromArgb(36, 41, 47);
    private static readonly Color HeadingColor = Color.FromArgb(31, 35, 40);
    private static readonly Color MutedColor = Color.FromArgb(87, 96, 106);
    private static readonly Color CodeColor = Color.FromArgb(36, 41, 47);
    private static readonly Color QuoteColor = Color.FromArgb(87, 96, 106);
    private static readonly Color LinkColor = Color.FromArgb(9, 105, 218);

    public static void Render(RichTextBox box, string? markdown)
    {
        box.SuspendLayout();
        try
        {
            box.Clear();
            var document = Markdown.Parse(markdown ?? string.Empty, Pipeline);
            foreach (var block in document)
            {
                WriteBlock(box, block, indent: 0);
            }

            box.SelectionStart = 0;
            box.SelectionLength = 0;
        }
        finally
        {
            box.ResumeLayout();
        }
    }

    private static void WriteBlock(RichTextBox box, Block block, int indent)
    {
        switch (block)
        {
            case HeadingBlock heading:
                WriteHeading(box, heading);
                break;
            case ParagraphBlock paragraph:
                WriteIndent(box, indent);
                WriteInlineContainer(box, paragraph.Inline, 10.5f, FontStyle.Regular, TextColor);
                Append(box, Environment.NewLine + Environment.NewLine, NormalFont(10.5f), TextColor);
                break;
            case QuoteBlock quote:
                WriteQuote(box, quote, indent);
                break;
            case ListBlock list:
                WriteList(box, list, indent);
                Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
                break;
            case FencedCodeBlock fenced:
                WriteCodeBlock(box, fenced.Lines.ToString(), indent);
                break;
            case CodeBlock code:
                WriteCodeBlock(box, code.Lines.ToString(), indent);
                break;
            case ThematicBreakBlock:
                Append(box, new string('─', 40) + Environment.NewLine + Environment.NewLine, NormalFont(10.5f), MutedColor);
                break;
            case Table table:
                WriteTable(box, table, indent);
                break;
            case HtmlBlock html:
                Append(box, html.Lines.ToString().TrimEnd() + Environment.NewLine + Environment.NewLine, CodeFont(9.5f), MutedColor);
                break;
            case ContainerBlock container:
                foreach (var child in container)
                {
                    WriteBlock(box, child, indent);
                }

                break;
            default:
                // 未知块：尽量输出叶子文本，避免整块“消失”
                if (block is LeafBlock leaf && leaf.Inline is not null)
                {
                    WriteIndent(box, indent);
                    WriteInlineContainer(box, leaf.Inline, 10.5f, FontStyle.Regular, TextColor);
                    Append(box, Environment.NewLine + Environment.NewLine, NormalFont(10.5f), TextColor);
                }

                break;
        }
    }

    private static void WriteHeading(RichTextBox box, HeadingBlock heading)
    {
        var size = heading.Level switch
        {
            1 => 20f,
            2 => 16f,
            3 => 13.5f,
            4 => 12f,
            _ => 11f
        };
        WriteInlineContainer(box, heading.Inline, size, FontStyle.Bold, HeadingColor);
        Append(box, Environment.NewLine + Environment.NewLine, NormalFont(10.5f), TextColor);
    }

    private static void WriteQuote(RichTextBox box, QuoteBlock quote, int indent)
    {
        foreach (var child in quote)
        {
            if (child is ParagraphBlock paragraph)
            {
                WriteIndent(box, indent);
                Append(box, "│ ", NormalFont(10.5f), QuoteColor);
                WriteInlineContainer(box, paragraph.Inline, 10.5f, FontStyle.Italic, QuoteColor);
                Append(box, Environment.NewLine, NormalFont(10.5f), QuoteColor);
            }
            else
            {
                WriteBlock(box, child, indent + 1);
            }
        }

        Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
    }

    private static void WriteList(RichTextBox box, ListBlock list, int indent)
    {
        var index = 1;
        if (list.IsOrdered && !int.TryParse(list.OrderedStart, out index))
        {
            index = 1;
        }

        foreach (var item in list.OfType<ListItemBlock>())
        {
            WriteIndent(box, indent);
            var marker = list.IsOrdered ? $"{index}. " : "• ";
            Append(box, marker, NormalFont(10.5f), TextColor);

            var first = true;
            foreach (var child in item)
            {
                if (child is ParagraphBlock paragraph)
                {
                    if (!first)
                    {
                        WriteIndent(box, indent + 1);
                    }

                    WriteInlineContainer(box, paragraph.Inline, 10.5f, FontStyle.Regular, TextColor);
                    Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
                    first = false;
                }
                else if (child is ListBlock nested)
                {
                    WriteList(box, nested, indent + 1);
                }
                else
                {
                    WriteBlock(box, child, indent + 1);
                    first = false;
                }
            }

            index++;
        }
    }

    private static void WriteCodeBlock(RichTextBox box, string code, int indent)
    {
        var text = (code ?? string.Empty).Replace("\r\n", "\n").TrimEnd('\n');
        foreach (var line in text.Split('\n'))
        {
            WriteIndent(box, indent);
            Append(box, "  " + line + Environment.NewLine, CodeFont(10f), CodeColor);
        }

        Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
    }

    private static void WriteTable(RichTextBox box, Table table, int indent)
    {
        var rows = table.OfType<TableRow>().ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var grid = rows.Select(row =>
            row.OfType<TableCell>()
                .Select(cell => FlattenInlines(cell.OfType<ParagraphBlock>().FirstOrDefault()?.Inline).Trim())
                .ToList())
            .ToList();

        var colCount = grid.Max(r => r.Count);
        var widths = new int[colCount];
        foreach (var row in grid)
        {
            for (var c = 0; c < colCount; c++)
            {
                var text = c < row.Count ? row[c] : string.Empty;
                widths[c] = Math.Max(widths[c], Math.Max(DisplayWidth(text), 3));
            }
        }

        for (var r = 0; r < grid.Count; r++)
        {
            WriteIndent(box, indent);
            var sb = new StringBuilder();
            sb.Append('│');
            for (var c = 0; c < colCount; c++)
            {
                var text = c < grid[r].Count ? grid[r][c] : string.Empty;
                sb.Append(' ');
                sb.Append(PadRightDisplay(text, widths[c]));
                sb.Append(' ');
                sb.Append('│');
            }

            var isHeader = r == 0;
            Append(box, sb + Environment.NewLine, CodeFont(9.5f), isHeader ? HeadingColor : TextColor);

            if (r == 0)
            {
                WriteIndent(box, indent);
                var sep = new StringBuilder();
                sep.Append('├');
                for (var c = 0; c < colCount; c++)
                {
                    sep.Append(new string('─', widths[c] + 2));
                    sep.Append(c == colCount - 1 ? '┤' : '┼');
                }

                Append(box, sep + Environment.NewLine, CodeFont(9.5f), MutedColor);
            }
        }

        Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
    }

    private static void WriteInlineContainer(
        RichTextBox box,
        ContainerInline? container,
        float size,
        FontStyle style,
        Color color)
    {
        if (container is null)
        {
            return;
        }

        foreach (var inline in container)
        {
            WriteInline(box, inline, size, style, color);
        }
    }

    private static void WriteInline(
        RichTextBox box,
        Inline inline,
        float size,
        FontStyle style,
        Color color)
    {
        switch (inline)
        {
            case LiteralInline literal:
                Append(box, literal.Content.ToString(), new Font("Microsoft YaHei UI", size, style), color);
                break;
            case CodeInline code:
                Append(box, code.Content, CodeFont(Math.Max(9f, size - 0.5f)), CodeColor);
                break;
            case EmphasisInline emphasis:
                var nextStyle = style;
                if (emphasis.DelimiterCount >= 2)
                {
                    nextStyle |= FontStyle.Bold;
                }
                else
                {
                    nextStyle |= FontStyle.Italic;
                }

                WriteInlineContainer(box, emphasis, size, nextStyle, color);
                break;
            case LinkInline link:
                var label = FlattenInlines(link);
                if (string.IsNullOrWhiteSpace(label))
                {
                    label = link.IsImage ? "[图片]" : (link.Url ?? "链接");
                }

                if (link.IsImage)
                {
                    Append(box, $"[图片] {label}", new Font("Microsoft YaHei UI", size, style), MutedColor);
                }
                else
                {
                    Append(box, label, new Font("Microsoft YaHei UI", size, style | FontStyle.Underline), LinkColor);
                    if (!string.IsNullOrWhiteSpace(link.Url))
                    {
                        Append(box, $" ({link.Url})", new Font("Microsoft YaHei UI", Math.Max(9f, size - 1f), FontStyle.Regular), MutedColor);
                    }
                }

                break;
            case AutolinkInline auto:
                Append(box, auto.Url, new Font("Microsoft YaHei UI", size, style | FontStyle.Underline), LinkColor);
                break;
            case LineBreakInline:
                Append(box, Environment.NewLine, NormalFont(size), color);
                break;
            case HtmlInline html:
                Append(box, html.Tag, CodeFont(Math.Max(9f, size - 1f)), MutedColor);
                break;
            case TaskList task:
                Append(box, task.Checked ? "☑ " : "☐ ", NormalFont(size), color);
                break;
            case ContainerInline container:
                WriteInlineContainer(box, container, size, style, color);
                break;
            default:
                // 兜底：尽量保留可读文本
                var fallback = inline.ToString();
                if (!string.IsNullOrEmpty(fallback) && fallback != inline.GetType().Name)
                {
                    Append(box, fallback, new Font("Microsoft YaHei UI", size, style), color);
                }

                break;
        }
    }

    private static string FlattenInlines(ContainerInline? container)
    {
        if (container is null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    sb.Append(literal.Content);
                    break;
                case CodeInline code:
                    sb.Append(code.Content);
                    break;
                case LinkInline link:
                    sb.Append(FlattenInlines(link));
                    break;
                case EmphasisInline emphasis:
                    sb.Append(FlattenInlines(emphasis));
                    break;
                case LineBreakInline:
                    sb.Append(' ');
                    break;
                case ContainerInline nested:
                    sb.Append(FlattenInlines(nested));
                    break;
            }
        }

        return sb.ToString();
    }

    private static void WriteIndent(RichTextBox box, int indent)
    {
        if (indent <= 0)
        {
            return;
        }

        Append(box, new string(' ', indent * 2), NormalFont(10.5f), TextColor);
    }

    private static void Append(RichTextBox box, string text, Font font, Color color)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionFont = font;
        box.SelectionColor = color;
        box.AppendText(text);
    }

    private static Font NormalFont(float size) => new("Microsoft YaHei UI", size, FontStyle.Regular);

    private static Font CodeFont(float size) => new("Consolas", size, FontStyle.Regular);

    private static int DisplayWidth(string text)
    {
        var width = 0;
        foreach (var ch in text)
        {
            width += ch > 0x7F ? 2 : 1;
        }

        return width;
    }

    private static string PadRightDisplay(string text, int width)
    {
        var pad = width - DisplayWidth(text);
        return pad <= 0 ? text : text + new string(' ', pad);
    }
}
