using System.ComponentModel;

namespace MarkdownContextViewer;

/// <summary>
/// 启用/取消资源管理器预览窗格：勾选格式、显示脚本日志与进度。
/// 界面在设计器文件中；格式分组复选框在运行时填充到 pnlFormatsHost。
/// </summary>
internal sealed partial class PreviewRegistrationForm : Form
{
    private readonly ExplorerIntegrationService _explorerService = null!;
    private readonly Dictionary<string, CheckBox> _extensionChecks = new(StringComparer.OrdinalIgnoreCase);
    private bool _busy;

    public PreviewRegistrationForm()
    {
        InitializeComponent();
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        BuildFormatGroups();
    }

    public PreviewRegistrationForm(ExplorerIntegrationService explorerService)
        : this()
    {
        _explorerService = explorerService ?? throw new ArgumentNullException(nameof(explorerService));
        LoadSavedSelection();
        RefreshEngineHint();
    }

    private void BuildFormatGroups()
    {
        pnlFormatsHost.SuspendLayout();
        pnlFormatsHost.Controls.Clear();
        _extensionChecks.Clear();

        foreach (var group in PreviewFormatCatalog.Groups)
        {
            var box = new GroupBox
            {
                Text = group.Title,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(640, 0),
                Padding = new Padding(10, 8, 10, 8),
                Margin = new Padding(0, 0, 0, 8)
            };

            var inner = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            inner.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Text = group.Description,
                Margin = new Padding(0, 0, 0, 4)
            });

            var checkRow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0)
            };

            foreach (var ext in group.Extensions)
            {
                var chk = new CheckBox
                {
                    AutoSize = true,
                    Text = ext,
                    Checked = true,
                    Tag = ext,
                    Margin = new Padding(0, 0, 16, 0)
                };
                _extensionChecks[ext] = chk;
                checkRow.Controls.Add(chk);
            }

            inner.Controls.Add(checkRow);
            box.Controls.Add(inner);
            pnlFormatsHost.Controls.Add(box);
        }

        pnlFormatsHost.ResumeLayout(true);
    }

    private void RefreshEngineHint()
    {
        var engine = PreviewEngineSetting.Load();
        var name = engine == PreviewEngineKind.WinForms ? "WinForms（更快更省）" : "Web（还原度高）";
        lblEngineHint.Text = $"当前预览引擎：{name}（在主窗口“查看”菜单切换；JSON/XML 推荐 WinForms）";
    }

    private void LoadSavedSelection()
    {
        ApplySelection(PreviewRegistrationSetting.Load().Extensions);
    }

    private void ApplySelection(IEnumerable<string> extensions)
    {
        var set = new HashSet<string>(
            extensions.Select(x => x.Trim().ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);
        foreach (var (ext, chk) in _extensionChecks)
        {
            chk.Checked = set.Contains(ext);
        }
    }

    private void SetAllChecks(bool value)
    {
        foreach (var chk in _extensionChecks.Values)
        {
            chk.Checked = value;
        }
    }

    private List<string> GetSelectedExtensions() =>
        _extensionChecks
            .Where(kv => kv.Value.Checked)
            .Select(kv => kv.Key)
            .ToList();

    private async void btnEnable_Click(object? sender, EventArgs e) => await RunEnableAsync();

    private async void btnDisable_Click(object? sender, EventArgs e) => await RunDisableAsync();

    private void btnClose_Click(object? sender, EventArgs e) => Close();

    private void btnSelectAll_Click(object? sender, EventArgs e) => SetAllChecks(true);

    private void btnSelectNone_Click(object? sender, EventArgs e) => SetAllChecks(false);

    private void btnSelectDefault_Click(object? sender, EventArgs e) =>
        ApplySelection(PreviewRegistrationOptions.CreateDefault().Extensions);

    private async Task RunEnableAsync()
    {
        if (_busy)
        {
            return;
        }

        var selected = GetSelectedExtensions();
        if (selected.Count == 0)
        {
            MessageBox.Show(this, "请至少勾选一种文件格式。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        PreviewRegistrationSetting.Save(new PreviewRegistrationOptions { Extensions = selected });
        var progress = CreateProgress();
        await RunOperationAsync(
            "正在注册并启用预览窗格…",
            () => _explorerService.EnablePreview(selected, progress));
    }

    private async Task RunDisableAsync()
    {
        if (_busy)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            "确定取消资源管理器预览窗格注册吗？将清除所有已关联扩展名。",
            "确认",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var progress = CreateProgress();
        await RunOperationAsync(
            "正在取消预览窗格…",
            () => _explorerService.DisablePreview(progress));
    }

    private async Task RunOperationAsync(string startMessage, Action work)
    {
        _busy = true;
        SetUiEnabled(false);
        progressBar.Value = 0;
        txtLog.Clear();
        AppendLog(startMessage);
        lblStatus.Text = startMessage;

        try
        {
            await Task.Run(work);
            AppendLog("—— 完成 ——");
            lblStatus.Text = "完成";
            if (progressBar.Value < 100)
            {
                progressBar.Value = 100;
            }
        }
        catch (Exception ex)
        {
            AppendLog("错误：" + ex.Message);
            lblStatus.Text = "失败";
            MessageBox.Show(this, ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            SetUiEnabled(true);
            RefreshEngineHint();
        }
    }

    private IProgress<PreviewRegistrationProgress> CreateProgress() =>
        new Progress<PreviewRegistrationProgress>(ApplyProgress);

    private void ApplyProgress(PreviewRegistrationProgress p)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(() => ApplyProgress(p));
            }
            catch (ObjectDisposedException)
            {
            }

            return;
        }

        if (p.Percent >= 0)
        {
            progressBar.Value = Math.Clamp(p.Percent, 0, 100);
        }

        if (!string.IsNullOrWhiteSpace(p.Message))
        {
            AppendLog(p.Message);
            if (p.Percent >= 0)
            {
                lblStatus.Text = p.Message;
            }
        }
    }

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        if (txtLog.TextLength > 0)
        {
            txtLog.AppendText(Environment.NewLine);
        }

        txtLog.AppendText(line);
    }

    private void SetUiEnabled(bool enabled)
    {
        btnEnable.Enabled = enabled;
        btnDisable.Enabled = enabled;
        btnSelectAll.Enabled = enabled;
        btnSelectNone.Enabled = enabled;
        btnSelectDefault.Enabled = enabled;
        foreach (var chk in _extensionChecks.Values)
        {
            chk.Enabled = enabled;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_busy)
        {
            e.Cancel = true;
            MessageBox.Show(this, "注册操作进行中，请稍候。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        base.OnFormClosing(e);
    }
}
