using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SharpShell.SharpPreviewHandler;

namespace MarkdownPreview.Handler;

/// <summary>
/// SharpShell 预览宿主：顶部工具栏 + WebView2 渲染。界面在设计器文件中。
/// </summary>
public partial class MarkdownPreviewControl : PreviewHandlerControl
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_ACTIVATE = 1;

    private readonly PreviewHistoryStore _history = new PreviewHistoryStore();
    private readonly CancellationTokenSource _updateCts = new CancellationTokenSource();

    private WebView2 _webView;
    private System.Windows.Forms.Timer _reloadTimer;
    private FileSystemWatcher _watcher;
    private string _pendingHtml;
    private string _currentPath;
    private string _currentHtml = "";
    private bool _initStarted;
    private bool _webReady;
    private bool _loggedMouseActivate;
    private double _zoom = 1.0;
    private UpdateState _updateState = UpdateState.Checking;
    private string _remoteTag;

    public MarkdownPreviewControl()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        lblVersion.Text = PreviewAbout.VersionText;
        lblAuthor.Text = PreviewAbout.Author;
        _webView = new WebView2 { Dock = DockStyle.Fill, Name = "webViewPreview" };
        panelPreview.Controls.Add(_webView);
        _reloadTimer = new System.Windows.Forms.Timer { Interval = 450 };
        _reloadTimer.Tick += (_, __) =>
        {
            _reloadTimer.Stop();
            ReloadCurrent();
        };
        HandleCreated += (_, __) => BeginInitWebView();
        UpdateActions();
        BeginUpdateCheck();
        HandlerLog.Info("预览控件已创建。");
    }

    public void ShowFile(string path)
    {
        LogOperation("打开文件", path);
        LoadFile(path, recordHistory: true);
    }

    public void ShowMarkdown(string markdown)
    {
        _currentPath = null;
        StopWatching();
        ShowHtml(MarkdownHtmlRenderer.Render(markdown));
        lblFile.Text = "未打开文件";
        lblFile.ToolTipText = "";
        UpdateActions();
    }

    internal void ReleaseRuntime()
    {
        if (_reloadTimer != null)
        {
            _reloadTimer.Stop();
            _reloadTimer.Dispose();
            _reloadTimer = null;
        }

        StopWatching();
        try
        {
            _updateCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        _updateCts.Dispose();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            if (!_loggedMouseActivate)
            {
                _loggedMouseActivate = true;
                HandlerLog.Info("操作: 预览控件收到鼠标激活，改为 MA_ACTIVATE");
            }

            m.Result = (IntPtr)MA_ACTIVATE;
            return;
        }

        base.WndProc(ref m);
    }

    private void actionStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        LogOperation("点击", e.ClickedItem == null ? "(空)" : e.ClickedItem.Text);
    }

    private void btnRecent_DropDownOpening(object sender, EventArgs e)
    {
        LogOperation("打开最近");
        FillRecentMenu();
    }

    private void btnHistory_Click(object sender, EventArgs e)
    {
        LogOperation("打开记录管理器");
        OpenHistoryManager();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        LogOperation("刷新");
        ReloadCurrent();
    }

    private void btnEditor_Click(object sender, EventArgs e)
    {
        LogOperation("打开编辑器");
        OpenEditor(_currentPath);
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
        LogOperation("导出 HTML");
        ExportHtml();
    }

    private void mnuCopyPath_Click(object sender, EventArgs e)
    {
        LogOperation("复制路径");
        CopyPath();
    }

    private void mnuOpenFolder_Click(object sender, EventArgs e)
    {
        LogOperation("打开所在文件夹");
        OpenFolder();
    }

    private void mnuOpenBrowser_Click(object sender, EventArgs e)
    {
        LogOperation("在浏览器中打开");
        OpenInBrowser();
    }

    private void mnuZoomIn_Click(object sender, EventArgs e)
    {
        LogOperation("放大");
        ChangeZoom(0.1);
    }

    private void mnuZoomOut_Click(object sender, EventArgs e)
    {
        LogOperation("缩小");
        ChangeZoom(-0.1);
    }

    private void mnuZoomReset_Click(object sender, EventArgs e)
    {
        LogOperation("重置缩放");
        _zoom = 1;
        ApplyZoom();
    }

    private void mnuPrint_Click(object sender, EventArgs e)
    {
        LogOperation("打印");
        Print();
    }

    private void lblHelp_Click(object sender, EventArgs e)
    {
        LogOperation("打开帮助", PreviewAbout.HelpUrl);
        OpenUrl(PreviewAbout.HelpUrl);
    }

    private void lblUpdate_Click(object sender, EventArgs e)
    {
        LogOperation("点击更新", _updateState.ToString());
        OnUpdateClick();
    }

    private void LogOperation(string action, string detail = null)
    {
        HandlerLog.Info(
            "操作: " + action +
            (string.IsNullOrEmpty(detail) ? "" : " | " + detail) +
            " | file=" + (_currentPath ?? "(无)"));
    }

    private void LoadFile(string path, bool recordHistory)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            HandlerLog.Info("操作: 文件不存在 | " + path);
            ShowMarkdown("**无法读取 Markdown 文件。**");
            return;
        }

        try
        {
            var markdown = ReadAllTextShared(path);
            _currentPath = path;
            ShowHtml(MarkdownHtmlRenderer.Render(markdown));
            lblFile.Text = Path.GetFileName(path);
            lblFile.ToolTipText = path;
            if (recordHistory)
            {
                _history.Add(path);
            }

            Watch(path);
            UpdateActions();
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取 Markdown 失败。", ex);
            ShowMarkdown("**预览失败：** " + ex.Message);
        }
    }

    private void ReloadCurrent()
    {
        if (string.IsNullOrWhiteSpace(_currentPath) || !File.Exists(_currentPath))
        {
            HandlerLog.Info("操作: 刷新被跳过，没有当前文件");
            return;
        }

        try
        {
            var markdown = ReadAllTextShared(_currentPath);
            ShowHtml(MarkdownHtmlRenderer.Render(markdown));
            HandlerLog.Info("操作: 刷新完成 | " + _currentPath);
        }
        catch (IOException)
        {
            HandlerLog.Info("操作: 刷新时文件被占用 | " + _currentPath);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("刷新预览失败。", ex);
        }
    }

    private static string ReadAllTextShared(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            return reader.ReadToEnd();
        }
    }

    private void ShowHtml(string html)
    {
        _currentHtml = html ?? "";
        _pendingHtml = _currentHtml;
        if (IsHandleCreated)
        {
            BeginInitWebView();
        }

        TryNavigate();
    }

    private void BeginInitWebView()
    {
        if (_initStarted || _webView == null)
        {
            return;
        }

        _initStarted = true;
        _ = InitWebViewAsync();
    }

    private async Task InitWebViewAsync()
    {
        try
        {
            var userData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "MarkdownContextViewer",
                "WebView2");
            Directory.CreateDirectory(userData);

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userData).ConfigureAwait(true);
            await _webView.EnsureCoreWebView2Async(env).ConfigureAwait(true);
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webReady = true;
            ApplyZoom();
            HandlerLog.Info("WebView2 initialized.");
            TryNavigate();
            UpdateActions();
        }
        catch (Exception ex)
        {
            HandlerLog.Error("WebView2 init failed.", ex);
            panelPreview.Controls.Clear();
            panelPreview.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "WebView2 初始化失败：\r\n" + ex.Message + "\r\n请确认已安装 WebView2 Runtime。"
            });
        }
    }

    private void TryNavigate()
    {
        if (_pendingHtml == null || _webView == null || _webView.CoreWebView2 == null)
        {
            return;
        }

        try
        {
            _webView.CoreWebView2.NavigateToString(_pendingHtml);
            HandlerLog.Info($"NavigateToString length={_pendingHtml.Length}");
        }
        catch (Exception ex)
        {
            HandlerLog.Error("NavigateToString failed.", ex);
        }
    }

    private void FillRecentMenu()
    {
        btnRecent.DropDownItems.Clear();
        var paths = _history.Load();
        var count = 0;
        foreach (var path in paths)
        {
            if (count >= 20)
            {
                break;
            }

            if (!File.Exists(path))
            {
                continue;
            }

            var item = new ToolStripMenuItem(FormatRecent(path)) { ToolTipText = path, Tag = path };
            item.Click += (_, __) =>
            {
                LogOperation("选择最近文件", (string)item.Tag);
                LoadFile((string)item.Tag, recordHistory: true);
            };
            btnRecent.DropDownItems.Add(item);
            count++;
        }

        if (count == 0)
        {
            btnRecent.DropDownItems.Add(new ToolStripMenuItem("暂无记录") { Enabled = false });
        }

        btnRecent.DropDownItems.Add(new ToolStripSeparator());
        var manage = new ToolStripMenuItem("打开记录管理器…");
        manage.Click += (_, __) =>
        {
            LogOperation("从最近菜单打开记录管理器");
            OpenHistoryManager();
        };
        btnRecent.DropDownItems.Add(manage);
        HandlerLog.Info("操作: 最近菜单项数量=" + count);
    }

    private static string FormatRecent(string path)
    {
        var name = Path.GetFileName(path);
        var dir = Path.GetFileName(Path.GetDirectoryName(path) ?? "");
        return string.IsNullOrEmpty(dir) ? name : name + "  (" + dir + ")";
    }

    private void OpenHistoryManager()
    {
        using (var form = new PreviewHistoryForm(_history, path =>
        {
            LogOperation("从记录管理器查看", path);
            LoadFile(path, recordHistory: true);
        }))
        {
            form.ShowDialog(FindForm());
        }
    }

    private void OpenEditor(string path)
    {
        if (!EditorLauncher.TryLaunch(path, out var error))
        {
            HandlerLog.Info("操作: 打开编辑器失败 | " + error);
            MessageBox.Show(this, error, "Markdown 预览", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        HandlerLog.Info("操作: 已启动编辑器 | " + path);
    }

    private void ExportHtml()
    {
        if (string.IsNullOrWhiteSpace(_currentPath))
        {
            HandlerLog.Info("操作: 导出被跳过，没有当前文件");
            return;
        }

        var name = Path.GetFileNameWithoutExtension(_currentPath);
        using (var dialog = new SaveFileDialog
        {
            Title = "导出 HTML 单文件",
            Filter = "HTML 文件 (*.html)|*.html",
            FileName = string.IsNullOrWhiteSpace(name) ? "document.html" : name + ".html",
            InitialDirectory = Path.GetDirectoryName(_currentPath) ?? "",
            OverwritePrompt = true
        })
        {
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                HandlerLog.Info("操作: 导出已取消");
                return;
            }

            try
            {
                HtmlSingleFileExporter.Export(_currentHtml, _currentPath, dialog.FileName);
                HandlerLog.Info("操作: 导出完成 | " + dialog.FileName);
                MessageBox.Show(this, "已导出：\n" + dialog.FileName, "Markdown 预览", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                HandlerLog.Error("导出 HTML 失败。", ex);
                MessageBox.Show(this, "导出失败：" + ex.Message, "Markdown 预览", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void CopyPath()
    {
        if (string.IsNullOrWhiteSpace(_currentPath))
        {
            HandlerLog.Info("操作: 复制路径被跳过，没有当前文件");
            return;
        }

        try
        {
            Clipboard.SetText(_currentPath);
            HandlerLog.Info("操作: 已复制路径 | " + _currentPath);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("复制路径失败。", ex);
        }
    }

    private void OpenFolder()
    {
        if (string.IsNullOrWhiteSpace(_currentPath) || !File.Exists(_currentPath))
        {
            HandlerLog.Info("操作: 打开文件夹被跳过");
            return;
        }

        Process.Start("explorer.exe", "/select,\"" + _currentPath + "\"");
        HandlerLog.Info("操作: 已打开文件夹 | " + _currentPath);
    }

    private void OpenInBrowser()
    {
        if (string.IsNullOrEmpty(_currentHtml))
        {
            HandlerLog.Info("操作: 浏览器打开被跳过，没有 HTML");
            return;
        }

        try
        {
            var temp = Path.Combine(Path.GetTempPath(), "MarkdownContextViewer-" + Guid.NewGuid().ToString("N") + ".html");
            File.WriteAllText(temp, _currentHtml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            Process.Start(new ProcessStartInfo { FileName = temp, UseShellExecute = true });
            HandlerLog.Info("操作: 已用浏览器打开 | " + temp);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("在浏览器中打开失败。", ex);
            MessageBox.Show(this, "无法在浏览器中打开：" + ex.Message, "Markdown 预览", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Print()
    {
        try
        {
            if (_webView == null || _webView.CoreWebView2 == null)
            {
                HandlerLog.Info("操作: 打印被跳过，WebView2 未就绪");
                return;
            }

            _webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
            HandlerLog.Info("操作: 已打开打印界面");
        }
        catch (Exception ex)
        {
            HandlerLog.Error("打印失败。", ex);
        }
    }

    private void ChangeZoom(double delta)
    {
        _zoom = Math.Max(0.5, Math.Min(2.5, _zoom + delta));
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        if (!_webReady || _webView == null)
        {
            HandlerLog.Info("操作: 缩放被跳过，WebView2 未就绪");
            return;
        }

        try
        {
            _webView.ZoomFactor = _zoom;
            HandlerLog.Info("操作: 缩放=" + _zoom.ToString("0.0"));
        }
        catch (Exception ex)
        {
            HandlerLog.Error("设置缩放失败。", ex);
        }
    }

    private void UpdateActions()
    {
        var hasFile = !string.IsNullOrWhiteSpace(_currentPath) && File.Exists(_currentPath);
        btnRefresh.Enabled = hasFile;
        btnEditor.Enabled = hasFile;
        btnExport.Enabled = hasFile && !string.IsNullOrEmpty(_currentHtml);
        mnuCopyPath.Enabled = hasFile;
        mnuOpenFolder.Enabled = hasFile;
        mnuOpenBrowser.Enabled = !string.IsNullOrEmpty(_currentHtml);
        mnuPrint.Enabled = _webReady;
    }

    private void Watch(string path)
    {
        StopWatching();
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        try
        {
            _watcher = new FileSystemWatcher(dir, name)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnWatchedFileChanged;
            _watcher.Renamed += (_, __) => OnWatchedFileChanged(null, null);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("监视文件变化失败。", ex);
        }
    }

    private void OnWatchedFileChanged(object sender, FileSystemEventArgs e)
    {
        if (IsDisposed || _reloadTimer == null)
        {
            return;
        }

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (_reloadTimer == null)
                {
                    return;
                }

                _reloadTimer.Stop();
                _reloadTimer.Start();
            }));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void StopWatching()
    {
        if (_watcher == null)
        {
            return;
        }

        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }

    private async void BeginUpdateCheck()
    {
        try
        {
            var result = await PreviewAbout.CheckUpdateAsync(_updateCts.Token).ConfigureAwait(true);
            if (IsDisposed)
            {
                return;
            }

            _updateState = result.State;
            _remoteTag = result.RemoteTag;
            switch (result.State)
            {
                case UpdateState.UpdateAvailable:
                    lblUpdate.Text = "可更新 " + result.RemoteTag;
                    break;
                case UpdateState.UpToDate:
                    lblUpdate.Text = "已是最新";
                    break;
                case UpdateState.NoRelease:
                    lblUpdate.Text = "暂无发布";
                    break;
                default:
                    lblUpdate.Text = "检查失败";
                    break;
            }

            HandlerLog.Info("操作: 更新检查结果=" + result.State + " tag=" + (result.RemoteTag ?? ""));
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            HandlerLog.Error("更新状态显示失败。", ex);
            if (!IsDisposed)
            {
                _updateState = UpdateState.Failed;
                lblUpdate.Text = "检查失败";
            }
        }
    }

    private void OnUpdateClick()
    {
        if (_updateState == UpdateState.Failed)
        {
            _updateState = UpdateState.Checking;
            lblUpdate.Text = "检查更新…";
            BeginUpdateCheck();
            return;
        }

        if (_updateState == UpdateState.UpdateAvailable || _updateState == UpdateState.NoRelease || _updateState == UpdateState.UpToDate)
        {
            OpenUrl(PreviewAbout.ReleasesUrl);
        }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            HandlerLog.Info("操作: 已打开链接 | " + url);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("打开链接失败。", ex);
        }
    }
}
