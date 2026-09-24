using System.Text;

namespace MarkdownPreview.Shell;

/// <summary>
/// 可在 VS2022 设计器中打开的 Markdown 渲染预览控件（非纯文本）。
/// </summary>
public partial class MarkdownPreviewControl : UserControl
{
    private readonly MarkdownRenderService _renderService = new();
    private string? _pendingHtml;
    private string? _tempHtmlPath;

    public MarkdownPreviewControl()
    {
        InitializeComponent();
        webBrowserPreview.DocumentCompleted += WebBrowserPreview_DocumentCompleted;
        Disposed += (_, _) => CleanupTempFile();
    }

    public string? CurrentFilePath { get; private set; }

    public void LoadMarkdownFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ShowStatus($"无法打开文件：{filePath}");
            return;
        }

        CurrentFilePath = filePath;
        var markdown = File.ReadAllText(filePath);
        LoadMarkdown(markdown);
    }

    public void LoadMarkdown(string markdown)
    {
        try
        {
            var html = _renderService.RenderHtml(markdown);
            NavigateHtml(html);
        }
        catch (Exception ex)
        {
            ShowStatus($"渲染失败：{ex.Message}");
        }
    }

    public void ClearPreview()
    {
        CurrentFilePath = null;
        NavigateHtml(_renderService.RenderHtml("_暂无预览内容_"));
    }

    public void ShowStatus(string message)
    {
        NavigateHtml(_renderService.RenderHtml($"**{message}**"));
    }

    private void NavigateHtml(string html)
    {
        CleanupTempFile();

        // 低完整性 prevhost 只能可靠写入 LocalLow\Temp；优先用 GetTempPath()。
        var tempDir = Path.GetTempPath();
        try
        {
            Directory.CreateDirectory(tempDir);
        }
        catch
        {
            tempDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "Temp");
            Directory.CreateDirectory(tempDir);
        }

        _tempHtmlPath = Path.Combine(tempDir, $"MarkdownContextViewer-{Guid.NewGuid():N}.html");
        File.WriteAllText(_tempHtmlPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        _pendingHtml = html;
        webBrowserPreview.Navigate(_tempHtmlPath);
    }

    private void WebBrowserPreview_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
    {
        if (_pendingHtml is null || webBrowserPreview.Document is null)
        {
            return;
        }

        try
        {
            // 部分宿主下再写一次 DocumentText，确保样式生效。
            if (string.Equals(e.Url?.AbsoluteUri, "about:blank", StringComparison.OrdinalIgnoreCase))
            {
                webBrowserPreview.Document.OpenNew(true);
                webBrowserPreview.Document.Write(_pendingHtml);
                webBrowserPreview.Refresh();
            }
        }
        catch
        {
            // 已通过 Navigate 展示，忽略二次写入失败。
        }
        finally
        {
            _pendingHtml = null;
        }
    }

    private void CleanupTempFile()
    {
        if (string.IsNullOrWhiteSpace(_tempHtmlPath))
        {
            return;
        }

        try
        {
            if (File.Exists(_tempHtmlPath))
            {
                File.Delete(_tempHtmlPath);
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            _tempHtmlPath = null;
        }
    }
}
