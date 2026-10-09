using System.ComponentModel;
using System.Text;

namespace MarkdownContextViewer;

/// <summary>
/// Markdown 编辑用户控件。
/// Ribbon：TabControl + ToolStrip；命令：<see cref="MarkdownTextCommands"/>；本类只做编排与快捷键。
/// </summary>
public partial class MarkdownEditorControl : UserControl
{
    private const int PreviewDebounceMs = 200;

    private readonly MarkdownRenderService _renderService;
    private readonly System.Windows.Forms.Timer _previewDebounceTimer;
    private MarkdownTextCommands _commands = null!;
    private string? _tempHtmlPath;

    public event EventHandler? SaveRequested;
    public event EventHandler<PreviewEngineKind>? PreviewEngineChanged;

    public MarkdownEditorControl()
        : this(new MarkdownRenderService())
    {
    }

    public MarkdownEditorControl(MarkdownRenderService renderService)
    {
        _renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));

        InitializeComponent();
        _commands = new MarkdownTextCommands(editor);

        components ??= new Container();
        _previewDebounceTimer = new System.Windows.Forms.Timer(components)
        {
            Interval = PreviewDebounceMs
        };
        _previewDebounceTimer.Tick += PreviewDebounceTimer_Tick;

        Disposed += (_, _) => CleanupTempFile();
        PreviewKeyDown += MarkdownEditorControl_PreviewKeyDown;
        editor.PreviewKeyDown += Editor_PreviewKeyDown;
        SyncEngineButtons(PreviewEngineSetting.Load());
        // 默认仅预览；FrmMain 打开文件时也会再调一次 SetPreviewMode()
        SetPreviewMode();
    }

    public string MarkdownText
    {
        get => editor.Text;
        set
        {
            editor.Text = value ?? string.Empty;
            RefreshPreviewImmediate();
        }
    }

    public void UsePreviewEngine(PreviewEngineKind kind)
    {
        var useWinForms = kind == PreviewEngineKind.WinForms;
        winFormsPreview.Visible = useWinForms;
        preview.Visible = !useWinForms;
        SyncEngineButtons(kind);
        RefreshPreviewImmediate();
    }

    public void SetSplitMode() => ShowPanels(true, true);

    public void SetEditorMode() => ShowPanels(true, false);

    public void SetPreviewMode() => ShowPanels(false, true);

    private void btnSave_Click(object? sender, EventArgs e) => RequestSave();

    private void btnUndo_Click(object? sender, EventArgs e) => _commands.Undo();

    private void btnRedo_Click(object? sender, EventArgs e) => _commands.Redo();

    private void btnBold_Click(object? sender, EventArgs e) => _commands.ToggleBold();

    private void btnItalic_Click(object? sender, EventArgs e) => _commands.ToggleItalic();

    private void btnStrike_Click(object? sender, EventArgs e) => _commands.ToggleStrikethrough();

    private void btnInlineCode_Click(object? sender, EventArgs e) => _commands.ToggleInlineCode();

    private void btnHeading1_Click(object? sender, EventArgs e) => _commands.SetHeading(1);

    private void btnHeading2_Click(object? sender, EventArgs e) => _commands.SetHeading(2);

    private void btnHeading3_Click(object? sender, EventArgs e) => _commands.SetHeading(3);

    private void btnQuote_Click(object? sender, EventArgs e) => _commands.ToggleQuote();

    private void btnBulletList_Click(object? sender, EventArgs e) => _commands.ToggleBulletList();

    private void btnOrderedList_Click(object? sender, EventArgs e) => _commands.ToggleOrderedList();

    private void btnTaskList_Click(object? sender, EventArgs e) => _commands.ToggleTaskList();

    private void btnLink_Click(object? sender, EventArgs e) => _commands.InsertLink();

    private void btnImage_Click(object? sender, EventArgs e) => _commands.InsertImage();

    private void btnCodeBlock_Click(object? sender, EventArgs e) => _commands.InsertCodeBlock();

    private void btnTable_Click(object? sender, EventArgs e) => _commands.InsertTable();

    private void btnHr_Click(object? sender, EventArgs e) => _commands.InsertHorizontalRule();

    private void btnEditorOnly_Click(object? sender, EventArgs e) => SetEditorMode();

    private void btnPreviewOnly_Click(object? sender, EventArgs e) => SetPreviewMode();

    private void btnSplit_Click(object? sender, EventArgs e) => SetSplitMode();

    private void btnEngineWeb_Click(object? sender, EventArgs e) =>
        ChangePreviewEngine(PreviewEngineKind.WebView);

    private void btnEngineWinForms_Click(object? sender, EventArgs e) =>
        ChangePreviewEngine(PreviewEngineKind.WinForms);

    private void ChangePreviewEngine(PreviewEngineKind kind)
    {
        PreviewEngineSetting.Save(kind);
        UsePreviewEngine(kind);
        PreviewEngineChanged?.Invoke(this, kind);
    }

    private void editor_TextChanged(object? sender, EventArgs e) => SchedulePreviewRefresh();

    private void MarkdownEditorControl_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.Control && e.KeyCode == Keys.E))
        {
            e.IsInputKey = true;
        }
    }

    private void Editor_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.Control && e.KeyCode == Keys.E))
        {
            e.IsInputKey = true;
        }
    }

    private void editor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            SetPreviewMode();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.Control && e.KeyCode == Keys.E && !e.Shift)
        {
            SetEditorMode();
            editor.Focus();
            e.SuppressKeyPress = true;
            return;
        }

        if (!e.Control)
        {
            return;
        }

        if (e.Shift)
        {
            switch (e.KeyCode)
            {
                case Keys.X:
                    _commands.ToggleStrikethrough();
                    e.SuppressKeyPress = true;
                    break;
                case Keys.K:
                    _commands.InsertCodeBlock();
                    e.SuppressKeyPress = true;
                    break;
                case Keys.D8:
                    _commands.ToggleBulletList();
                    e.SuppressKeyPress = true;
                    break;
                case Keys.D7:
                    _commands.ToggleOrderedList();
                    e.SuppressKeyPress = true;
                    break;
            }

            return;
        }

        switch (e.KeyCode)
        {
            case Keys.S:
                RequestSave();
                e.SuppressKeyPress = true;
                break;
            case Keys.B:
                _commands.ToggleBold();
                e.SuppressKeyPress = true;
                break;
            case Keys.I:
                _commands.ToggleItalic();
                e.SuppressKeyPress = true;
                break;
            case Keys.K:
                _commands.InsertLink();
                e.SuppressKeyPress = true;
                break;
            case Keys.Oemtilde:
                _commands.ToggleInlineCode();
                e.SuppressKeyPress = true;
                break;
            case Keys.D1:
                _commands.SetHeading(1);
                e.SuppressKeyPress = true;
                break;
            case Keys.D2:
                _commands.SetHeading(2);
                e.SuppressKeyPress = true;
                break;
            case Keys.D3:
                _commands.SetHeading(3);
                e.SuppressKeyPress = true;
                break;
        }
    }

    private void RequestSave() => SaveRequested?.Invoke(this, EventArgs.Empty);

    private void PreviewDebounceTimer_Tick(object? sender, EventArgs e)
    {
        _previewDebounceTimer.Stop();
        ApplyPreview();
    }

    private void SchedulePreviewRefresh()
    {
        if (IsDesignTime())
        {
            return;
        }

        _previewDebounceTimer.Stop();
        _previewDebounceTimer.Start();
    }

    private void RefreshPreviewImmediate()
    {
        if (IsDesignTime())
        {
            return;
        }

        _previewDebounceTimer.Stop();
        ApplyPreview();
    }

    private void ApplyPreview()
    {
        if (winFormsPreview.Visible)
        {
            winFormsPreview.ShowMarkdown(editor.Text);
            return;
        }

        NavigateHtml(_renderService.RenderHtml(editor.Text));
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

        _tempHtmlPath = Path.Combine(tempDir, $"MarkdownEditorPreview-{Guid.NewGuid():N}.html");
        File.WriteAllText(_tempHtmlPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        preview.Navigate(_tempHtmlPath);
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

    private void ShowPanels(bool showEditor, bool showPreview)
    {
        mainSplit.Panel1Collapsed = !showEditor;
        mainSplit.Panel2Collapsed = !showPreview;
        SyncViewModeButtons(showEditor, showPreview);
        ApplyChromeForMode(showEditor);
    }

    private void ApplyChromeForMode(bool showEditor)
    {
        // 仅预览：收起 Ribbon 编辑区，只留快捷栏（含布局/引擎切换）
        ribbonTabs.Visible = showEditor;
        ribbonHost.Height = showEditor ? 96 : 36;
        btnUndo.Visible = showEditor;
        btnRedo.Visible = showEditor;
        btnSave.Enabled = true;
    }

    private void SyncViewModeButtons(bool showEditor, bool showPreview)
    {
        btnEditorOnly.Checked = showEditor && !showPreview;
        btnPreviewOnly.Checked = !showEditor && showPreview;
        btnSplit.Checked = showEditor && showPreview;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            SetPreviewMode();
            return true;
        }

        if (keyData == (Keys.Control | Keys.E))
        {
            SetEditorMode();
            if (!mainSplit.Panel1Collapsed)
            {
                editor.Focus();
            }

            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SyncEngineButtons(PreviewEngineKind kind)
    {
        btnEngineWeb.Checked = kind == PreviewEngineKind.WebView;
        btnEngineWinForms.Checked = kind == PreviewEngineKind.WinForms;
    }

    private static bool IsDesignTime() =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;
}
