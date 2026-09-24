using MarkdownPreview.Shell;

namespace MarkdownContextViewer;

public partial class FrmMain : Form
{
    private const string AppName = "MarkdownContextViewer";

    private readonly List<string> _history = [];
    private readonly string[] _startupArgs;
    private readonly HistoryService _historyService;
    private readonly ExplorerIntegrationService _explorerService;
    private readonly MarkdownRenderService _markdownRenderService;
    private readonly SiteExportService _siteExportService;
    private readonly DirectorySiteExportService _directorySiteExportService;
    private readonly LocalWebHostService _localWebHostService;

    private bool _isLoadingPreviewSwitch;

    public FrmMain(string[]? args = null)
    {
        _startupArgs = args ?? [];

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

        InitializeComponent();
        WireEvents();

        LoadExplorerPreviewSwitchState();
        LoadHistory();
        OpenFilesFromArgs();
    }

    private void WireEvents()
    {
        lstHistory.DoubleClick += (_, _) => OpenFromHistory();
        tabsMarkdown.MouseClick += TabsMarkdown_MouseClick;
        chkExplorerPreview.CheckedChanged += ChkExplorerPreview_CheckedChanged;
    }

    private void OpenFilesFromArgs()
    {
        foreach (var arg in _startupArgs)
        {
            var path = arg.Trim('"');
            if (File.Exists(path) && Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                OpenMarkdownFile(path);
            }
        }
    }

    private void OpenFilesByDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Markdown 文件 (*.md)|*.md",
            Multiselect = true,
            Title = "选择 Markdown 文件"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        foreach (var file in dialog.FileNames)
        {
            OpenMarkdownFile(file);
        }
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

        editorControl.SaveRequested += (_, _) => SaveCurrentTab();

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

    private void LoadExplorerPreviewSwitchState()
    {
        _isLoadingPreviewSwitch = true;
        chkExplorerPreview.Checked = _explorerService.IsPreviewEnabled();
        _isLoadingPreviewSwitch = false;
    }

    private void ChkExplorerPreview_CheckedChanged(object? sender, EventArgs e)
    {
        if (_isLoadingPreviewSwitch)
        {
            return;
        }

        try
        {
            if (chkExplorerPreview.Checked)
            {
                _explorerService.EnablePreview();
                var deployDir = ExplorerIntegrationService.GetDeployDirectory();
                var mediumIl = _explorerService.IsMediumIntegrityOptOutEnabled();
                MessageBox.Show(
                    "已启用 .md 渲染预览（SharpShell + WebView2）。\n\n" +
                    "说明：资源管理器通过 prevhost 加载预览 DLL，不启动主程序。\n" +
                    $"预览组件已部署到：\n{deployDir}\n\n" +
                    (mediumIl
                        ? "已写入 HKLM DisableLowIL（中等完整性 prevhost）。\n"
                        : "注意：若取消了 UAC，Win11 仍可能用低完整性 prevhost；请再次勾选并同意提权。\n") +
                    "请打开“查看 → 预览窗格”（Alt+P），重新选中 .md 文件。\n" +
                    "需已安装 Microsoft Edge WebView2 Runtime。\n" +
                    "若仍无法预览，请点“结束预览宿主”后再选文件，或查看“预览诊断”。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                _explorerService.DisablePreview();
                MessageBox.Show("已禁用预览，并已结束预览宿主进程。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            _isLoadingPreviewSwitch = true;
            chkExplorerPreview.Checked = !chkExplorerPreview.Checked;
            _isLoadingPreviewSwitch = false;
            MessageBox.Show($"操作失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RegisterContextMenu()
    {
        try
        {
            _explorerService.RegisterContextMenu(Application.ExecutablePath);
            MessageBox.Show("右键菜单注册成功（当前用户）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"注册失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

    private void KillPrevhost()
    {
        try
        {
            var count = _explorerService.KillPrevhost();
            MessageBox.Show(
                count > 0
                    ? $"已结束 {count} 个 prevhost.exe 进程。\n请回到资源管理器重新选中 .md 文件。"
                    : "当前没有运行中的 prevhost.exe。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"结束失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RestartExplorer()
    {
        var confirm = MessageBox.Show(
            "将结束预览宿主并重启资源管理器（任务栏会短暂消失后恢复）。\n是否继续？",
            "确认",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _explorerService.RestartExplorer();
            MessageBox.Show("资源管理器已重启。请重新打开预览窗格并选中 .md 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"重启失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DiagnosePreview()
    {
        try
        {
            var report = _explorerService.DiagnosePreview();
            MessageBox.Show(report, "预览诊断", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"诊断失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnRegisterContext_Click(object sender, EventArgs e) => RegisterContextMenu();
    private void btnUnregisterContext_Click_1(object sender, EventArgs e) => UnregisterContextMenu();
    private void btnOpen_Click(object sender, EventArgs e) => OpenFilesByDialog();
    private void btnSaveCurrent_Click(object sender, EventArgs e) => SaveCurrentTab();
    private void btnExportWeb_Click(object sender, EventArgs e) => OpenPublishCenter();
    private void btnMarkdownPreview_Click(object sender, EventArgs e) => OpenMarkdownPreviewWindow();
    private void btnKillPrevhost_Click(object sender, EventArgs e) => KillPrevhost();
    private void btnRestartExplorer_Click(object sender, EventArgs e) => RestartExplorer();
    private void btnDiagnosePreview_Click(object sender, EventArgs e) => DiagnosePreview();

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _localWebHostService.Dispose();
        base.OnFormClosed(e);
    }
}
