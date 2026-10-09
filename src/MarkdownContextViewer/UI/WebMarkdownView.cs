using System.ComponentModel;
using System.Text;

namespace MarkdownContextViewer;

/// <summary>
/// 使用 Web（WebBrowser）引擎显示 Markdown 的独立用户控件。
/// 可在设计器中拖放到窗体；支持加载文件或直接传入 Markdown 文本。
/// </summary>
public partial class WebMarkdownView : UserControl
{
    private readonly MarkdownRenderService _renderService;
    private string? _tempHtmlPath;
    private string? _pendingHtml;
    private string? _markdownText;

    public WebMarkdownView()
        : this(new MarkdownRenderService())
    {
    }

    public WebMarkdownView(MarkdownRenderService renderService)
    {
        _renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));
        InitializeComponent();
        webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
    }

    /// <summary>当前关联的 .md 文件路径（若通过 <see cref="LoadMarkdownFile"/> 打开）。</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? CurrentFilePath { get; private set; }

    /// <summary>当前显示的 Markdown 原文。</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string MarkdownText
    {
        get => _markdownText ?? string.Empty;
        set => LoadMarkdown(value);
    }

    /// <summary>从磁盘加载并渲染 .md 文件（相对图片按文件所在目录解析）。</summary>
    public void LoadMarkdownFile(string filePath)
    {
        if (IsDesignTime())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ShowStatus($"无法打开文件：{filePath}");
            return;
        }

        CurrentFilePath = Path.GetFullPath(filePath);
        var markdown = File.ReadAllText(CurrentFilePath, Encoding.UTF8);
        LoadMarkdown(markdown, Path.GetDirectoryName(CurrentFilePath));
    }

    /// <summary>渲染 Markdown 文本。</summary>
    public void LoadMarkdown(string? markdown) =>
        LoadMarkdown(markdown, baseDirectory: Path.GetDirectoryName(CurrentFilePath));

    /// <summary>渲染 Markdown 文本，并用 baseDirectory 解析相对资源（图片等）。</summary>
    public void LoadMarkdown(string? markdown, string? baseDirectory)
    {
        if (IsDesignTime())
        {
            return;
        }

        try
        {
            _markdownText = markdown ?? string.Empty;
            var html = _renderService.RenderHtml(_markdownText, baseDirectory);
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
        _markdownText = string.Empty;
        if (IsDesignTime())
        {
            return;
        }

        NavigateHtml(_renderService.RenderHtml("_暂无预览内容_"));
    }

    public void ShowStatus(string message)
    {
        if (IsDesignTime())
        {
            return;
        }

        NavigateHtml(_renderService.RenderHtml($"**{message}**"));
    }

    /// <summary>刷新当前内容（文件则重新读盘，否则重渲染内存中的 Markdown）。</summary>
    public void RefreshView()
    {
        if (!string.IsNullOrWhiteSpace(CurrentFilePath) && File.Exists(CurrentFilePath))
        {
            LoadMarkdownFile(CurrentFilePath);
            return;
        }

        LoadMarkdown(_markdownText, Path.GetDirectoryName(CurrentFilePath));
    }

    private void NavigateHtml(string html)
    {
        CleanupTempFile();

        var tempDir = Path.GetTempPath();
        try
        {
            Directory.CreateDirectory(tempDir);
        }
        catch
        {
            tempDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp");
            Directory.CreateDirectory(tempDir);
        }

        _tempHtmlPath = Path.Combine(tempDir, $"WebMarkdownView-{Guid.NewGuid():N}.html");
        File.WriteAllText(_tempHtmlPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        _pendingHtml = html;
        webBrowser.Navigate(_tempHtmlPath);
    }

    private void WebBrowser_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
    {
        if (_pendingHtml is null || webBrowser.Document is null)
        {
            return;
        }

        try
        {
            if (string.Equals(e.Url?.AbsoluteUri, "about:blank", StringComparison.OrdinalIgnoreCase))
            {
                webBrowser.Document.OpenNew(true);
                webBrowser.Document.Write(_pendingHtml);
                webBrowser.Refresh();
            }
        }
        catch
        {
            // 已通过 Navigate 展示
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

    private static bool IsDesignTime() =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;
}
