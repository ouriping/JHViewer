using System.ComponentModel;
using System.Text;

namespace MarkdownContextViewer;

public partial class MarkdownEditorControl : UserControl
{
    private const int PreviewDebounceMs = 200;

    private readonly MarkdownRenderService _renderService;
    private readonly System.Windows.Forms.Timer _previewDebounceTimer;
    private string? _tempHtmlPath;

    public event EventHandler? SaveRequested;

    public MarkdownEditorControl()
        : this(new MarkdownRenderService())
    {
    }

    public MarkdownEditorControl(MarkdownRenderService renderService)
    {
        _renderService = renderService ?? throw new ArgumentNullException(nameof(renderService));

        InitializeComponent();

        components ??= new Container();
        _previewDebounceTimer = new System.Windows.Forms.Timer(components)
        {
            Interval = PreviewDebounceMs
        };
        _previewDebounceTimer.Tick += PreviewDebounceTimer_Tick;

        Disposed += (_, _) => CleanupTempFile();
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
        RefreshPreviewImmediate();
    }

    public void SetSplitMode() => ShowPanels(true, true);
    public void SetEditorMode() => ShowPanels(true, false);
    public void SetPreviewMode() => ShowPanels(false, true);

    private void btnSave_Click(object sender, EventArgs e) => SaveRequested?.Invoke(this, EventArgs.Empty);

    private void btnUndo_Click(object sender, EventArgs e) => UndoEditor();

    private void btnRedo_Click(object sender, EventArgs e) => RedoEditor();

    private void btnBold_Click(object sender, EventArgs e) => WrapSelection("**", "**");

    private void btnItalic_Click(object sender, EventArgs e) => WrapSelection("*", "*");

    private void btnInlineCode_Click(object sender, EventArgs e) => WrapSelection("`", "`");

    private void btnHeading_Click(object sender, EventArgs e) => InsertLinePrefix("# ");

    private void btnQuote_Click(object sender, EventArgs e) => InsertLinePrefix("> ");

    private void btnList_Click(object sender, EventArgs e) => InsertLinePrefix("- ");

    private void btnLink_Click(object sender, EventArgs e) => InsertAtCaret("[文本](https://example.com)");

    private void btnImage_Click(object sender, EventArgs e) => InsertAtCaret("![说明](https://example.com/image.png)");

    private void btnCodeBlock_Click(object sender, EventArgs e) => InsertAtCaret("```\ncode\n```\n");

    private void btnEditorOnly_Click(object sender, EventArgs e) => SetEditorMode();

    private void btnPreviewOnly_Click(object sender, EventArgs e) => SetPreviewMode();

    private void btnSplit_Click(object sender, EventArgs e) => SetSplitMode();

    private void editor_TextChanged(object sender, EventArgs e) => SchedulePreviewRefresh();

    private void editor_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.Control)
        {
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.S:
                SaveRequested?.Invoke(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
                break;
            case Keys.B:
                WrapSelection("**", "**");
                e.SuppressKeyPress = true;
                break;
            case Keys.I:
                WrapSelection("*", "*");
                e.SuppressKeyPress = true;
                break;
            case Keys.K:
                InsertAtCaret("[文本](https://example.com)");
                e.SuppressKeyPress = true;
                break;
        }
    }

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

    private void UndoEditor()
    {
        if (editor.CanUndo)
        {
            editor.Undo();
        }

        editor.Focus();
    }

    private void RedoEditor()
    {
        if (editor.CanRedo)
        {
            editor.Redo();
        }

        editor.Focus();
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
            // 临时文件清理失败可忽略
        }
        finally
        {
            _tempHtmlPath = null;
        }
    }

    private void WrapSelection(string prefix, string suffix)
    {
        var start = editor.SelectionStart;
        var selected = editor.SelectedText;

        editor.SelectedText = prefix + selected + suffix;
        editor.SelectionStart = start + prefix.Length;
        editor.SelectionLength = selected.Length;
        editor.Focus();
    }

    private void InsertAtCaret(string value)
    {
        var start = editor.SelectionStart;
        editor.SelectionLength = 0;
        editor.SelectedText = value;
        editor.SelectionStart = start + value.Length;
        editor.SelectionLength = 0;
        editor.Focus();
    }

    private void InsertLinePrefix(string prefix)
    {
        var start = editor.SelectionStart;
        var lineStart = editor.Text.LastIndexOf('\n', Math.Max(0, start - 1));
        var insertPos = lineStart < 0 ? 0 : lineStart + 1;

        editor.SelectionStart = insertPos;
        editor.SelectionLength = 0;
        editor.SelectedText = prefix;
        editor.SelectionStart = start + prefix.Length;
        editor.SelectionLength = 0;
        editor.Focus();
    }

    private void ShowPanels(bool showEditor, bool showPreview)
    {
        mainSplit.Panel1Collapsed = !showEditor;
        mainSplit.Panel2Collapsed = !showPreview;
    }

    private static bool IsDesignTime() =>
        LicenseManager.UsageMode == LicenseUsageMode.Designtime;
}
