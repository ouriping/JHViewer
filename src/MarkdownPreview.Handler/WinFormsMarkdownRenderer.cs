using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

internal static class PreviewEngineChoice
{
    public static bool UseWinForms()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarkdownContextViewer",
            "preview-engine.txt");
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var text = File.ReadAllText(path).Trim();
            return string.Equals(text, "WinForms", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取预览引擎设置失败。", ex);
            return false;
        }
    }
}

internal static class WinFormsMarkdownRenderer
{
    public static void Render(RichTextBox box, string markdown)
    {
        box.Clear();
        var text = markdown ?? string.Empty;
        var lines = text.Replace("\r\n", "\n").Split('\n');
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
                Append(box, raw + Environment.NewLine, new Font("Consolas", 10F), Color.FromArgb(36, 41, 47));
                continue;
            }

            var line = raw;
            var size = 10.5f;
            var heading = false;
            if (line.StartsWith("### "))
            {
                line = line.Substring(4);
                size = 13f;
                heading = true;
            }
            else if (line.StartsWith("## "))
            {
                line = line.Substring(3);
                size = 16f;
                heading = true;
            }
            else if (line.StartsWith("# "))
            {
                line = line.Substring(2);
                size = 20f;
                heading = true;
            }
            else if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                line = "• " + line.Substring(2);
            }

            var parts = (line + Environment.NewLine).Split(new[] { "**" }, StringSplitOptions.None);
            for (var i = 0; i < parts.Length; i++)
            {
                var bold = heading || i % 2 == 1;
                var style = bold ? FontStyle.Bold : FontStyle.Regular;
                Append(box, parts[i], new Font("Microsoft YaHei UI", size, style), Color.FromArgb(36, 41, 47));
            }
        }

        box.SelectionStart = 0;
        box.SelectionLength = 0;
    }

    private static void Append(RichTextBox box, string text, Font font, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;
        box.SelectionFont = font;
        box.SelectionColor = color;
        box.AppendText(text);
    }
}
