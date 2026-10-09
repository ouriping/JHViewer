using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownPreview.Handler;

internal static class PreviewEngineChoice
{
    private static string SettingPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MarkdownContextViewer",
        "preview-engine.txt");

    public static bool UseWinForms() => IsWinForms(Load());

    public static string Load()
    {
        try
        {
            if (!File.Exists(SettingPath))
            {
                return "WebView";
            }

            var text = File.ReadAllText(SettingPath).Trim();
            return IsWinForms(text) ? "WinForms" : "WebView";
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取预览引擎设置失败。", ex);
            return "WebView";
        }
    }

    public static void Save(bool useWinForms)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingPath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(SettingPath, useWinForms ? "WinForms" : "WebView");
            HandlerLog.Info("预览引擎已保存: " + (useWinForms ? "WinForms" : "WebView"));
        }
        catch (Exception ex)
        {
            HandlerLog.Error("保存预览引擎设置失败。", ex);
        }
    }

    private static bool IsWinForms(string text) =>
        string.Equals(text, "WinForms", StringComparison.OrdinalIgnoreCase);
}

internal static class WinFormsMarkdownRenderer
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

    public static void Render(RichTextBox box, string markdown)
    {
        box.SuspendLayout();
        try
        {
            box.Clear();
            var document = Markdown.Parse(markdown ?? string.Empty, Pipeline);
            foreach (var block in document)
            {
                WriteBlock(box, block, 0);
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
        if (block is HeadingBlock heading)
        {
            WriteHeading(box, heading);
            return;
        }

        if (block is ParagraphBlock paragraph)
        {
            WriteIndent(box, indent);
            WriteInlineContainer(box, paragraph.Inline, 10.5f, FontStyle.Regular, TextColor);
            Append(box, Environment.NewLine + Environment.NewLine, NormalFont(10.5f), TextColor);
            return;
        }

        if (block is QuoteBlock quote)
        {
            WriteQuote(box, quote, indent);
            return;
        }

        if (block is ListBlock list)
        {
            WriteList(box, list, indent);
            Append(box, Environment.NewLine, NormalFont(10.5f), TextColor);
            return;
        }

        if (block is FencedCodeBlock fenced)
        {
            WriteCodeBlock(box, fenced.Lines.ToString(), indent);
            return;
        }

        if (block is CodeBlock code)
        {
            WriteCodeBlock(box, code.Lines.ToString(), indent);
            return;
        }

        if (block is ThematicBreakBlock)
        {
            Append(box, new string('─', 40) + Environment.NewLine + Environment.NewLine, NormalFont(10.5f), MutedColor);
            return;
        }

        if (block is Table table)
        {
            WriteTable(box, table, indent);
            return;
        }

        if (block is HtmlBlock html)
        {
            Append(box, html.Lines.ToString().TrimEnd() + Environment.NewLine + Environment.NewLine, CodeFont(9.5f), MutedColor);
            return;
        }

        if (block is ContainerBlock container)
        {
            foreach (var child in container)
            {
                WriteBlock(box, child, indent);
            }

            return;
        }

        if (block is LeafBlock leaf && leaf.Inline != null)
        {
            WriteIndent(box, indent);
            WriteInlineContainer(box, leaf.Inline, 10.5f, FontStyle.Regular, TextColor);
            Append(box, Environment.NewLine + Environment.NewLine, NormalFont(10.5f), TextColor);
        }
    }

    private static void WriteHeading(RichTextBox box, HeadingBlock heading)
    {
        float size;
        switch (heading.Level)
        {
            case 1: size = 20f; break;
            case 2: size = 16f; break;
            case 3: size = 13.5f; break;
            case 4: size = 12f; break;
            default: size = 11f; break;
        }

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
            var marker = list.IsOrdered ? index.ToString() + ". " : "• ";
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
                .Select(cell =>
                {
                    var p = cell.OfType<ParagraphBlock>().FirstOrDefault();
                    return FlattenInlines(p != null ? p.Inline : null).Trim();
                })
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

            Append(box, sb + Environment.NewLine, CodeFont(9.5f), r == 0 ? HeadingColor : TextColor);

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
        ContainerInline container,
        float size,
        FontStyle style,
        Color color)
    {
        if (container == null)
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
        if (inline is LiteralInline literal)
        {
            Append(box, literal.Content.ToString(), new Font("Microsoft YaHei UI", size, style), color);
            return;
        }

        if (inline is CodeInline code)
        {
            Append(box, code.Content, CodeFont(Math.Max(9f, size - 0.5f)), CodeColor);
            return;
        }

        if (inline is EmphasisInline emphasis)
        {
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
            return;
        }

        if (inline is LinkInline link)
        {
            var label = FlattenInlines(link);
            if (string.IsNullOrWhiteSpace(label))
            {
                label = link.IsImage ? "[图片]" : (link.Url ?? "链接");
            }

            if (link.IsImage)
            {
                Append(box, "[图片] " + label, new Font("Microsoft YaHei UI", size, style), MutedColor);
            }
            else
            {
                Append(box, label, new Font("Microsoft YaHei UI", size, style | FontStyle.Underline), LinkColor);
                if (!string.IsNullOrWhiteSpace(link.Url))
                {
                    Append(box, " (" + link.Url + ")", new Font("Microsoft YaHei UI", Math.Max(9f, size - 1f), FontStyle.Regular), MutedColor);
                }
            }

            return;
        }

        if (inline is AutolinkInline auto)
        {
            Append(box, auto.Url, new Font("Microsoft YaHei UI", size, style | FontStyle.Underline), LinkColor);
            return;
        }

        if (inline is LineBreakInline)
        {
            Append(box, Environment.NewLine, NormalFont(size), color);
            return;
        }

        if (inline is HtmlInline html)
        {
            Append(box, html.Tag, CodeFont(Math.Max(9f, size - 1f)), MutedColor);
            return;
        }

        if (inline is TaskList task)
        {
            Append(box, task.Checked ? "☑ " : "☐ ", NormalFont(size), color);
            return;
        }

        if (inline is ContainerInline container)
        {
            WriteInlineContainer(box, container, size, style, color);
        }
    }

    private static string FlattenInlines(ContainerInline container)
    {
        if (container == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var inline in container)
        {
            if (inline is LiteralInline literal)
            {
                sb.Append(literal.Content);
            }
            else if (inline is CodeInline code)
            {
                sb.Append(code.Content);
            }
            else if (inline is LinkInline link)
            {
                sb.Append(FlattenInlines(link));
            }
            else if (inline is EmphasisInline emphasis)
            {
                sb.Append(FlattenInlines(emphasis));
            }
            else if (inline is LineBreakInline)
            {
                sb.Append(' ');
            }
            else if (inline is ContainerInline nested)
            {
                sb.Append(FlattenInlines(nested));
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

    private static Font NormalFont(float size) => new Font("Microsoft YaHei UI", size, FontStyle.Regular);

    private static Font CodeFont(float size) => new Font("Consolas", size, FontStyle.Regular);

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
