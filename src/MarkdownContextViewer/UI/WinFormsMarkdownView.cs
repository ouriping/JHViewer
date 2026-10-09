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
        WinFormsMarkdownRtfRenderer.Render(richPreview, markdown);
    }
}
