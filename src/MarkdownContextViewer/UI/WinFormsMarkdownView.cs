namespace MarkdownContextViewer;

/// <summary>
/// 用 RichTextBox 渲染 Markdown，不经过 WebView / WebBrowser。
/// </summary>
public partial class WinFormsMarkdownView : UserControl
{
    public WinFormsMarkdownView()
    {
        InitializeComponent();
    }

    public void ShowMarkdown(string? markdown)
    {
        richPreview.Clear();
        var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var inCode = false;
        foreach (var raw in lines)
        {
            if (raw.TrimStart().StartsWith("```"))
            {
                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                Append(raw + Environment.NewLine, new Font("Consolas", 10F), Color.FromArgb(36, 41, 47));
                continue;
            }

            var line = raw;
            var size = 10.5f;
            var heading = false;
            if (line.StartsWith("### "))
            {
                line = line[4..];
                size = 13f;
                heading = true;
            }
            else if (line.StartsWith("## "))
            {
                line = line[3..];
                size = 16f;
                heading = true;
            }
            else if (line.StartsWith("# "))
            {
                line = line[2..];
                size = 20f;
                heading = true;
            }
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                line = "• " + line[2..];
            }

            AppendFormatted(line + Environment.NewLine, size, heading);
        }

        richPreview.SelectionStart = 0;
        richPreview.SelectionLength = 0;
    }

    private void AppendFormatted(string text, float size, bool heading)
    {
        var parts = text.Split("**");
        for (var i = 0; i < parts.Length; i++)
        {
            var bold = heading || i % 2 == 1;
            var style = bold ? FontStyle.Bold : FontStyle.Regular;
            Append(parts[i], new Font("Microsoft YaHei UI", size, style), heading ? Color.FromArgb(31, 35, 40) : Color.FromArgb(36, 41, 47));
        }
    }

    private void Append(string text, Font font, Color color)
    {
        richPreview.SelectionStart = richPreview.TextLength;
        richPreview.SelectionLength = 0;
        richPreview.SelectionFont = font;
        richPreview.SelectionColor = color;
        richPreview.AppendText(text);
    }
}
