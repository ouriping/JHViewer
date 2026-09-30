namespace MarkdownContextViewer;

/// <summary>
/// 启用/取消资源管理器预览窗格：勾选格式、显示脚本日志与进度。
/// </summary>
internal sealed class PreviewRegistrationForm : Form
{
    private readonly ExplorerIntegrationService _explorerService;
    private readonly Dictionary<string, CheckBox> _extensionChecks = new(StringComparer.OrdinalIgnoreCase);

    private Panel _pnlFormats = null!;
    private Button _btnEnable = null!;
    private Button _btnDisable = null!;
    private Button _btnClose = null!;
    private ProgressBar _progressBar = null!;
    private Label _lblStatus = null!;
    private TextBox _txtLog = null!;
    private Label _lblEngineHint = null!;

    private bool _busy;

    public PreviewRegistrationForm(ExplorerIntegrationService explorerService)
    {
        _explorerService = explorerService ?? throw new ArgumentNullException(nameof(explorerService));
        InitializeUi();
        LoadSavedSelection();
        RefreshEngineHint();
    }

    private void InitializeUi()
    {
        Text = "启用资源管理器预览窗格";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(640, 560);
        Size = new Size(720, 640);
        Font = new Font("Microsoft YaHei UI", 9F);
        MaximizeBox = false;
        ShowInTaskbar = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var intro = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            Text = "选择要在资源管理器“预览窗格”中启用的文件类型。" +
                   "注册会部署预览组件并写入扩展名关联；下方显示脚本操作与进度。",
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(intro, 0, 0);

        _pnlFormats = new Panel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 4)
        };
        var formatLayout = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0)
        };
        _pnlFormats.Controls.Add(formatLayout);

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

            var desc = new Label
            {
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Text = group.Description,
                Margin = new Padding(0, 0, 0, 4)
            };
            inner.Controls.Add(desc);

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
            formatLayout.Controls.Add(box);
        }

        var quickRow = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 0, 0, 4)
        };
        var btnAll = new Button { Text = "全选", AutoSize = true };
        var btnNone = new Button { Text = "全不选", AutoSize = true };
        var btnDefault = new Button { Text = "恢复默认", AutoSize = true };
        btnAll.Click += (_, _) => SetAllChecks(true);
        btnNone.Click += (_, _) => SetAllChecks(false);
        btnDefault.Click += (_, _) => ApplySelection(PreviewRegistrationOptions.CreateDefault().Extensions);
        quickRow.Controls.Add(btnAll);
        quickRow.Controls.Add(btnNone);
        quickRow.Controls.Add(btnDefault);
        formatLayout.Controls.Add(quickRow);

        root.Controls.Add(_pnlFormats, 0, 1);

        _lblEngineHint = new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(_lblEngineHint, 0, 2);

        var logPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 4) };
        var logLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        logLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        logLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        logLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 100,
            Style = ProgressBarStyle.Continuous
        };
        _lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            Text = "就绪",
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(250, 250, 250)
        };

        logLayout.Controls.Add(_progressBar, 0, 0);
        logLayout.Controls.Add(_lblStatus, 0, 1);
        logLayout.Controls.Add(_txtLog, 0, 2);
        logPanel.Controls.Add(logLayout);
        root.Controls.Add(logPanel, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        _btnClose = new Button { Text = "关闭", AutoSize = true, DialogResult = DialogResult.Cancel };
        _btnDisable = new Button { Text = "取消预览窗格", AutoSize = true };
        _btnEnable = new Button { Text = "注册并启用", AutoSize = true };
        _btnEnable.Click += async (_, _) => await RunEnableAsync();
        _btnDisable.Click += async (_, _) => await RunDisableAsync();
        _btnClose.Click += (_, _) => Close();
        buttons.Controls.Add(_btnClose);
        buttons.Controls.Add(_btnDisable);
        buttons.Controls.Add(_btnEnable);
        root.Controls.Add(buttons, 0, 4);

        AcceptButton = _btnEnable;
        CancelButton = _btnClose;
    }

    private void RefreshEngineHint()
    {
        var engine = PreviewEngineSetting.Load();
        var name = engine == PreviewEngineKind.WinForms ? "WinForms（更快更省）" : "Web（还原度高）";
        _lblEngineHint.Text = $"当前预览引擎：{name}（在主窗口“查看”菜单切换；JSON/XML 推荐 WinForms）";
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
        // 必须在 UI 线程创建 Progress，才能捕获 WinForms 同步上下文
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
        _progressBar.Value = 0;
        _txtLog.Clear();
        AppendLog(startMessage);
        _lblStatus.Text = startMessage;

        try
        {
            await Task.Run(work);
            AppendLog("—— 完成 ——");
            _lblStatus.Text = "完成";
            if (_progressBar.Value < 100)
            {
                _progressBar.Value = 100;
            }
        }
        catch (Exception ex)
        {
            AppendLog("错误：" + ex.Message);
            _lblStatus.Text = "失败";
            MessageBox.Show(this, ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busy = false;
            SetUiEnabled(true);
            RefreshEngineHint();
        }
    }

    private IProgress<PreviewRegistrationProgress> CreateProgress()
    {
        return new Progress<PreviewRegistrationProgress>(ApplyProgress);
    }

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
                // ignore
            }

            return;
        }

        if (p.Percent >= 0)
        {
            _progressBar.Value = Math.Clamp(p.Percent, 0, 100);
        }

        if (!string.IsNullOrWhiteSpace(p.Message))
        {
            AppendLog(p.Message);
            if (p.Percent >= 0)
            {
                _lblStatus.Text = p.Message;
            }
        }
    }

    private void AppendLog(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
        if (_txtLog.TextLength > 0)
        {
            _txtLog.AppendText(Environment.NewLine);
        }

        _txtLog.AppendText(line);
    }

    private void SetUiEnabled(bool enabled)
    {
        _btnEnable.Enabled = enabled;
        _btnDisable.Enabled = enabled;
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
