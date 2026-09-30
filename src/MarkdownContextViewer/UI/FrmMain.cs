using System.ComponentModel;
using System.Runtime.InteropServices;
using MarkdownPreview.Shell;

namespace MarkdownContextViewer;

public partial class FrmMain : Form
{
    private const string AppName = "MarkdownContextViewer";

    private readonly List<string> _history = [];
    private readonly string[] _startupArgs;
    private readonly HistoryService _historyService = null!;
    private readonly ExplorerIntegrationService _explorerService = null!;
    private readonly MarkdownRenderService _markdownRenderService = null!;
    private readonly SiteExportService _siteExportService = null!;
    private readonly DirectorySiteExportService _directorySiteExportService = null!;
    private readonly LocalWebHostService _localWebHostService = null!;

    private PreviewEngineKind _previewEngine = PreviewEngineKind.WebView;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SwRestore = 9;

    public FrmMain(string[]? args = null)
    {
        _startupArgs = args ?? [];
        InitializeComponent();

        // 设计器只需要界面布局，避免在构造阶段读写文件/注册表导致无法打开设计器。
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        var historyFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName,
            "history.json");

        _historyService = new HistoryService(historyFilePath);
        _explorerService = new ExplorerIntegrationService();
        _markdownRenderService = new MarkdownRenderService();
        _siteExportService = new SiteExportService(_markdownRenderService);
        _directorySiteExportService = new DirectorySiteExportService(_markdownRenderService);
        _localWebHostService = new LocalWebHostService();

        WireEvents();

        _previewEngine = PreviewEngineSetting.Load();
        ApplyEngineToOpenEditors();
        RefreshStatus();
        LoadHistory();
        OpenFilesFromArgs();
    }

    /// <summary>
    /// 单实例 IPC：已有窗口收到后续启动传入的路径（可为空，仅置前）。
    /// </summary>
    public void OpenExternalFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var normalized = path.Trim().Trim('"');
            if (normalized.Length == 0
                || string.Equals(normalized, SingleInstanceService.ActivateToken, StringComparison.Ordinal))
            {
                continue;
            }

            if (File.Exists(normalized)
                && Path.GetExtension(normalized).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                OpenMarkdownFile(normalized);
            }
        }

        ActivateMainWindow();
    }

    private void WireEvents()
    {
        lstHistory.DoubleClick += (_, _) => OpenFromHistory();
        tabsMarkdown.MouseClick += TabsMarkdown_MouseClick;
    }

    private void OpenFilesFromArgs()
    {
        foreach (var arg in _startupArgs)
        {
            var path = arg.Trim('"');
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path);
                if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    OpenStructuredPreview(path);
                }
                else if (ext.Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    OpenMarkdownFile(path);
                }
            }
        }
    }

    private void OpenFilesByDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "文档|*.md;*.json;*.xml|Markdown (*.md)|*.md|JSON (*.json)|*.json|XML (*.xml)|*.xml|所有文件|*.*",
            Multiselect = true,
            Title = "选择要打开的文件"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        foreach (var file in dialog.FileNames)
        {
            OpenDocumentFile(file);
        }
    }

    private void OpenDocumentFile(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            OpenStructuredPreview(filePath);
            return;
        }

        OpenMarkdownFile(filePath);
    }

    private void OpenStructuredPreview(string? preferredPath = null)
    {
        using var form = new DocumentPreviewDemoForm();
        if (!string.IsNullOrWhiteSpace(preferredPath) && File.Exists(preferredPath))
        {
            form.LoadFile(preferredPath);
        }

        form.ShowDialog(this);
    }

    private void OpenDemoStructured(string fileName)
    {
        var dir = PreviewDemoPaths.FindDirectory();
        if (dir is null)
        {
            MessageBox.Show(
                "未找到 samples/preview-demo。请确认仓库中存在该目录，或重新编译以复制示例到输出目录。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        OpenStructuredPreview(Path.Combine(dir, fileName));
    }

    private void OpenDemoFolder()
    {
        var dir = PreviewDemoPaths.FindDirectory();
        if (dir is null)
        {
            MessageBox.Show("未找到 samples/preview-demo 目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = dir,
            UseShellExecute = true
        });
    }

    private void OpenMarkdownFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show($"文件不存在：{filePath}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (TabPage page in tabsMarkdown.TabPages)
        {
            if (string.Equals(page.Tag as string, filePath, StringComparison.OrdinalIgnoreCase))
            {
                tabsMarkdown.SelectedTab = page;
                return;
            }
        }

        var markdown = File.ReadAllText(filePath);
        var tab = BuildMarkdownTab(filePath, markdown);

        tabsMarkdown.TabPages.Add(tab);
        tabsMarkdown.SelectedTab = tab;

        AddHistory(filePath);
    }

    private TabPage BuildMarkdownTab(string filePath, string markdown)
    {
        var editorControl = new MarkdownEditorControl(_markdownRenderService)
        {
            Dock = DockStyle.Fill,
            MarkdownText = markdown
        };

        editorControl.UsePreviewEngine(_previewEngine);
        editorControl.SaveRequested += (_, _) => SaveCurrentTab();
        editorControl.PreviewEngineChanged += (_, kind) => SetPreviewEngine(kind);

        var tab = new TabPage(Path.GetFileName(filePath))
        {
            Tag = filePath
        };
        tab.Controls.Add(editorControl);

        return tab;
    }

    private void SaveCurrentTab()
    {
        if (tabsMarkdown.SelectedTab is null)
        {
            MessageBox.Show("请先打开一个 Markdown 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var filePath = tabsMarkdown.SelectedTab.Tag as string;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var editorControl = tabsMarkdown.SelectedTab.Controls.OfType<MarkdownEditorControl>().FirstOrDefault();
        if (editorControl is null)
        {
            return;
        }

        File.WriteAllText(filePath, editorControl.MarkdownText);
        AddHistory(filePath);
        MessageBox.Show("保存成功。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OpenPublishCenter()
    {
        using var form = new PublishCenterForm(
            GetCurrentDocumentForPublish,
            _siteExportService,
            _directorySiteExportService,
            _localWebHostService);

        form.ShowDialog(this);
    }

    private (string filePath, string markdownText)? GetCurrentDocumentForPublish()
    {
        if (tabsMarkdown.SelectedTab is null)
        {
            return null;
        }

        var filePath = tabsMarkdown.SelectedTab.Tag as string;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var editorControl = tabsMarkdown.SelectedTab.Controls.OfType<MarkdownEditorControl>().FirstOrDefault();
        if (editorControl is null)
        {
            return null;
        }

        return (filePath, editorControl.MarkdownText);
    }

    private void OpenFromHistory()
    {
        if (lstHistory.SelectedItem is string filePath)
        {
            OpenMarkdownFile(filePath);
        }
    }

    private void LoadHistory()
    {
        _history.Clear();
        _history.AddRange(_historyService.LoadExistingPaths());
        RefreshHistoryList();
    }

    private void AddHistory(string filePath)
    {
        _historyService.AddRecent(_history, filePath);
        RefreshHistoryList();
        _historyService.Save(_history);
    }

    private void RefreshHistoryList()
    {
        lstHistory.BeginUpdate();
        lstHistory.Items.Clear();

        foreach (var item in _history)
        {
            lstHistory.Items.Add(item);
        }

        lstHistory.EndUpdate();
    }

    private void EnableExplorerPreview()
    {
        using var form = new PreviewRegistrationForm(_explorerService);
        form.ShowDialog(this);
        RefreshStatus();
    }

    private void DisableExplorerPreview()
    {
        using var form = new PreviewRegistrationForm(_explorerService);
        form.ShowDialog(this);
        RefreshStatus();
    }

    private void SetPreviewEngine(PreviewEngineKind kind)
    {
        _previewEngine = kind;
        PreviewEngineSetting.Save(kind);
        ApplyEngineToOpenEditors();
        RefreshStatus();
    }

    private void ApplyEngineToOpenEditors()
    {
        foreach (TabPage page in tabsMarkdown.TabPages)
        {
            var editor = page.Controls.OfType<MarkdownEditorControl>().FirstOrDefault();
            editor?.UsePreviewEngine(_previewEngine);
        }

        mnuEngineWebView.Checked = _previewEngine == PreviewEngineKind.WebView;
        mnuEngineWinForms.Checked = _previewEngine == PreviewEngineKind.WinForms;
        lblEngine.Text = _previewEngine == PreviewEngineKind.WinForms
            ? "引擎：WinForms（更快更省）"
            : "引擎：Web（还原度高）";
    }

    private void RefreshStatus()
    {
        lblStatus.Text = _explorerService.IsPreviewEnabled()
            ? "资源管理器预览：已启用"
            : "资源管理器预览：未启用";
    }

    private void RegisterContextMenu()
    {
        try
        {
            EditorPathStore.Save(Application.ExecutablePath);
            _explorerService.RegisterContextMenu(Application.ExecutablePath);
            MessageBox.Show("右键菜单注册成功（当前用户）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"注册失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActivateMainWindow()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            ShowWindow(Handle, SwRestore);
        }

        Show();
        Activate();
        BringToFront();
        SetForegroundWindow(Handle);
    }

    private void UnregisterContextMenu()
    {
        try
        {
            _explorerService.UnregisterContextMenu();
            MessageBox.Show("右键菜单已卸载（当前用户）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"卸载失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TabsMarkdown_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        for (var i = 0; i < tabsMarkdown.TabCount; i++)
        {
            var rect = tabsMarkdown.GetTabRect(i);
            if (!rect.Contains(e.Location))
            {
                continue;
            }

            var menu = new ContextMenuStrip();
            menu.Items.Add("关闭标签", null, (_, _) => tabsMarkdown.TabPages.RemoveAt(i));
            menu.Show(tabsMarkdown, e.Location);
            break;
        }
    }

    private void OpenMarkdownPreviewWindow()
    {
        using var form = new MarkdownPreviewForm();
        if (tabsMarkdown.SelectedTab?.Tag is string filePath && File.Exists(filePath))
        {
            form.LoadFile(filePath);
        }

        form.ShowDialog(this);
    }

    private async void KillPrevhost()
    {
        using var dlg = new OperationProgressForm("结束预览宿主", "正在结束 prevhost…", enableCancelAfterMs: 2000);
        var killed = 0;
        try
        {
            var task = dlg.RunAsync((progress, token) =>
            {
                killed = _explorerService.KillPrevhost(progress, token);
            });
            dlg.ShowDialog(this);
            await task;
            MessageBox.Show(
                killed > 0
                    ? $"已结束 {killed} 个 prevhost.exe 进程。\n请回到资源管理器重新选中文件。"
                    : "当前没有运行中的 prevhost.exe。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("已取消等待。部分 prevhost 可能仍在运行。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"结束失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RestartExplorer()
    {
        var confirm = MessageBox.Show(
            "将结束预览宿主并重启资源管理器（任务栏会短暂消失后恢复）。\n" +
            "操作约数秒；若卡住可点「取消等待」。\n是否继续？",
            "确认",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        using var dlg = new OperationProgressForm("重启资源管理器", "正在重启…", enableCancelAfterMs: 2500);
        try
        {
            var task = dlg.RunAsync((progress, token) =>
                _explorerService.RestartExplorer(progress, token, timeoutMs: 8000));
            dlg.ShowDialog(this);
            await task;
            MessageBox.Show(
                "资源管理器已重启。请重新打开预览窗格并选中文件。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show(
                "已取消等待。若任务栏未恢复，请按 Ctrl+Shift+Esc，运行新任务：explorer.exe",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"重启失败：{ex.Message}\n\n若任务栏消失，请按 Ctrl+Shift+Esc，运行新任务：explorer.exe",
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void DiagnosePreview()
    {
        using var dlg = new OperationProgressForm("预览诊断", "正在收集诊断信息…", enableCancelAfterMs: 2000);
        string? report = null;
        try
        {
            var task = dlg.RunAsync((progress, token) =>
            {
                progress.Report(new PreviewRegistrationProgress(20, "读取注册表与进程…"));
                token.ThrowIfCancellationRequested();
                report = _explorerService.DiagnosePreview();
                progress.Report(new PreviewRegistrationProgress(100, "诊断完成"));
            });
            dlg.ShowDialog(this);
            await task;
            if (!string.IsNullOrWhiteSpace(report))
            {
                MessageBox.Show(report, "预览诊断", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("诊断已取消。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"诊断失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void mnuOpen_Click(object sender, EventArgs e) => OpenFilesByDialog();
    private void mnuSave_Click(object sender, EventArgs e) => SaveCurrentTab();
    private void mnuPublish_Click(object sender, EventArgs e) => OpenPublishCenter();
    private void mnuExit_Click(object sender, EventArgs e) => Close();
    private void mnuRegisterContext_Click(object sender, EventArgs e) => RegisterContextMenu();
    private void mnuUnregisterContext_Click(object sender, EventArgs e) => UnregisterContextMenu();
    private void mnuEnablePreview_Click(object sender, EventArgs e) => EnableExplorerPreview();
    private void mnuDisablePreview_Click(object sender, EventArgs e) => DisableExplorerPreview();
    private void mnuKillPrevhost_Click(object sender, EventArgs e) => KillPrevhost();
    private void mnuRestartExplorer_Click(object sender, EventArgs e) => RestartExplorer();
    private void mnuDiagnosePreview_Click(object sender, EventArgs e) => DiagnosePreview();
    private void mnuEngineWebView_Click(object sender, EventArgs e) => SetPreviewEngine(PreviewEngineKind.WebView);
    private void mnuEngineWinForms_Click(object sender, EventArgs e) => SetPreviewEngine(PreviewEngineKind.WinForms);
    private void mnuPreviewWindow_Click(object sender, EventArgs e) => OpenMarkdownPreviewWindow();
    private void mnuPreviewDemo_Click(object sender, EventArgs e) => OpenStructuredPreview();
    private void mnuOpenDemoJson_Click(object sender, EventArgs e) => OpenDemoStructured("book-catalog.json");
    private void mnuOpenDemoXml_Click(object sender, EventArgs e) => OpenDemoStructured("library.xml");
    private void mnuOpenDemoFolder_Click(object sender, EventArgs e) => OpenDemoFolder();

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _localWebHostService.Dispose();
        base.OnFormClosed(e);
    }
}
