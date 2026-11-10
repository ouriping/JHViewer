using System.Text;

namespace MarkdownPreview.Shell;

/// <summary>
/// 可在 VS2022 设计器中打开的 Markdown 渲染预览控件（非纯文本）。
/// </summary>
public partial class MarkdownPreviewControl : UserControl
{
    private readonly MarkdownRenderService _renderService = new();
    private readonly TextBox _sourceView;
    private readonly Font _sourceFont;
    private string? _pendingHtml;
    private string? _tempPreviewDirectory;
    private string _markdownText = string.Empty;

    public MarkdownPreviewControl()
    {
        InitializeComponent();
        _sourceFont = new Font("Microsoft YaHei UI", 10.5f);
        _sourceView = new TextBox
        {
            Name = "sourceView",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            AcceptsTab = true,
            HideSelection = false,
            BackColor = SystemColors.Window,
            Font = _sourceFont,
            Visible = false
        };
        Controls.Add(_sourceView);
        webBrowserPreview.DocumentCompleted += WebBrowserPreview_DocumentCompleted;
        Disposed += (_, _) =>
        {
            CleanupTempFile();
            _sourceFont.Dispose();
        };
    }

    public string? CurrentFilePath { get; private set; }

    /// <summary>解析相对图片时使用的目录，通常是 Markdown 文件所在目录。</summary>
    public string? AssetDirectory { get; private set; }

    public string MarkdownText => _markdownText;

    public bool ShowSource
    {
        get => _sourceView.Visible;
        set
        {
            _sourceView.Visible = value;
            webBrowserPreview.Visible = !value;
            if (value)
            {
                _sourceView.Text = _markdownText;
                _sourceView.BringToFront();
            }
            else
            {
                webBrowserPreview.BringToFront();
            }
        }
    }

    public void LoadMarkdownFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ShowStatus($"无法打开文件：{filePath}");
            return;
        }

        CurrentFilePath = Path.GetFullPath(filePath);
        var markdown = File.ReadAllText(CurrentFilePath, Encoding.UTF8);
        LoadMarkdown(markdown, Path.GetDirectoryName(CurrentFilePath));
    }

    public void LoadMarkdown(string markdown) => LoadMarkdown(markdown, AssetDirectory);

    public void LoadMarkdown(string markdown, string? baseDirectory)
    {
        try
        {
            _markdownText = markdown ?? string.Empty;
            AssetDirectory = baseDirectory;
            if (_sourceView.Visible)
            {
                _sourceView.Text = _markdownText;
            }

            var html = _renderService.RenderHtml(_markdownText);
            NavigateHtml(html, baseDirectory);
        }
        catch (Exception ex)
        {
            ShowStatus($"渲染失败：{ex.Message}");
        }
    }

    public void ClearPreview()
    {
        CurrentFilePath = null;
        AssetDirectory = null;
        _markdownText = string.Empty;
        _sourceView.Clear();
        NavigateHtml(_renderService.RenderHtml("_暂无预览内容_"), null);
    }

    public void ShowStatus(string message)
    {
        NavigateHtml(_renderService.RenderHtml($"**{message}**"), null);
    }

    private void NavigateHtml(string html, string? baseDirectory)
    {
        CleanupTempFile();

        var root = Path.GetTempPath();
        try
        {
            Directory.CreateDirectory(root);
        }
        catch
        {
            root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "Temp");
            Directory.CreateDirectory(root);
        }

        _tempPreviewDirectory = Path.Combine(root, "JHViewerPreview-" + Guid.NewGuid().ToString("N"));
        var htmlPath = MarkdownHtmlPackager.Write(html, baseDirectory, _tempPreviewDirectory);
        _pendingHtml = html;
        webBrowserPreview.Navigate(htmlPath);
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
        if (string.IsNullOrWhiteSpace(_tempPreviewDirectory))
        {
            return;
        }

        try
        {
            if (Directory.Exists(_tempPreviewDirectory))
            {
                Directory.Delete(_tempPreviewDirectory, recursive: true);
            }
        }
        catch
        {
            // 浏览器可能仍占用刚才打开的页面。
        }
        finally
        {
            _tempPreviewDirectory = null;
        }
    }
}
