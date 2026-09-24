namespace MarkdownContextViewer;

public class MarkdownEditorControl : UserControl
{
    private readonly MarkdownRenderService _renderService;

    private readonly Panel _ribbonHost;
    private readonly FlowLayoutPanel _quickAccessBar;
    private readonly TabControl _ribbonTabs;
    private readonly SplitContainer _mainSplit;
    private readonly RichTextBox _editor;
    private readonly WebBrowser _preview;

    public event EventHandler? SaveRequested;

    public MarkdownEditorControl(MarkdownRenderService renderService)
    {
        _renderService = renderService;

        _ribbonHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 168,
            BackColor = Color.FromArgb(243, 246, 251),
            Padding = new Padding(6, 6, 6, 0)
        };

        _quickAccessBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            WrapContents = false,
            AutoSize = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0)
        };

        _ribbonTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal,
            Multiline = false
        };

        _mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 480
        };

        _editor = new RichTextBox
        {
            ScrollBars = RichTextBoxScrollBars.Both,
            WordWrap = false,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 11),
            AcceptsTab = true,
            DetectUrls = false
        };

        _preview = new WebBrowser
        {
            Dock = DockStyle.Fill,
            ScriptErrorsSuppressed = true
        };

        _editor.TextChanged += (_, _) => RefreshPreview();
        _editor.KeyDown += Editor_KeyDown;

        _mainSplit.Panel1.Controls.Add(_editor);
        _mainSplit.Panel2.Controls.Add(_preview);

        BuildQuickAccessBar();
        BuildRibbonTabs();

        _ribbonHost.Controls.Add(_ribbonTabs);
        _ribbonHost.Controls.Add(_quickAccessBar);
        Controls.Add(_mainSplit);
        Controls.Add(_ribbonHost);
        Dock = DockStyle.Fill;
    }

    public string MarkdownText
    {
        get => _editor.Text;
        set
        {
            _editor.Text = value;
            RefreshPreview();
        }
    }

    public void SetSplitMode() => ShowPanels(true, true);
    public void SetEditorMode() => ShowPanels(true, false);
    public void SetPreviewMode() => ShowPanels(false, true);

    private void BuildQuickAccessBar()
    {
        _quickAccessBar.Controls.Add(CreateQuickButton("保存", () => SaveRequested?.Invoke(this, EventArgs.Empty)));
        _quickAccessBar.Controls.Add(CreateQuickButton("撤销", UndoEditor));
        _quickAccessBar.Controls.Add(CreateQuickButton("重做", RedoEditor));
    }

    private Button CreateQuickButton(string text, Action action)
    {
        var btn = new Button
        {
            Text = text,
            Width = 72,
            Height = 28,
            Margin = new Padding(0, 0, 6, 0)
        };
        btn.Click += (_, _) => action();
        return btn;
    }

    private void BuildRibbonTabs()
    {
        _ribbonTabs.TabPages.Add(CreateStartTab());
        _ribbonTabs.TabPages.Add(CreateInsertTab());
        _ribbonTabs.TabPages.Add(CreateViewTab());
    }

    private TabPage CreateStartTab()
    {
        return CreateRibbonTab(
            "开始",
            CreateGroup("字体",
                ("粗体", () => WrapSelection("**", "**")),
                ("斜体", () => WrapSelection("*", "*")),
                ("行内代码", () => WrapSelection("`", "`"))
            ),
            CreateGroup("段落",
                ("标题", () => InsertLinePrefix("# ")),
                ("引用", () => InsertLinePrefix("> ")),
                ("列表", () => InsertLinePrefix("- "))
            )
        );
    }

    private TabPage CreateInsertTab()
    {
        return CreateRibbonTab(
            "插入",
            CreateGroup("常用",
                ("链接", () => InsertAtCaret("[文本](https://example.com)")),
                ("图片", () => InsertAtCaret("![说明](https://example.com/image.png)")),
                ("代码块", () => InsertAtCaret("```\ncode\n```\n"))
            )
        );
    }

    private TabPage CreateViewTab()
    {
        return CreateRibbonTab(
            "视图",
            CreateGroup("布局",
                ("仅编辑", SetEditorMode),
                ("仅预览", SetPreviewMode),
                ("分栏", SetSplitMode)
            )
        );
    }

    private static TabPage CreateRibbonTab(string title, params GroupBox[] groups)
    {
        var page = new TabPage(title);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8, 6, 8, 6)
        };

        foreach (var group in groups)
        {
            flow.Controls.Add(group);
        }

        page.Controls.Add(flow);
        return page;
    }

    private static GroupBox CreateGroup(string title, params (string text, Action action)[] actions)
    {
        var group = new GroupBox
        {
            Text = title,
            Width = 330,
            Height = 72,
            Margin = new Padding(6, 0, 6, 0)
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(6, 4, 6, 6)
        };

        foreach (var (text, action) in actions)
        {
            var btn = new Button
            {
                Text = text,
                Width = 96,
                Height = 28,
                Margin = new Padding(4)
            };
            btn.Click += (_, _) => action();
            panel.Controls.Add(btn);
        }

        group.Controls.Add(panel);
        return group;
    }

    private void Editor_KeyDown(object? sender, KeyEventArgs e)
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

    private void UndoEditor()
    {
        if (_editor.CanUndo)
        {
            _editor.Undo();
        }

        _editor.Focus();
    }

    private void RedoEditor()
    {
        if (_editor.CanRedo)
        {
            _editor.Redo();
        }

        _editor.Focus();
    }

    private void RefreshPreview()
    {
        _preview.DocumentText = _renderService.RenderHtml(_editor.Text);
    }

    private void WrapSelection(string prefix, string suffix)
    {
        var start = _editor.SelectionStart;
        var len = _editor.SelectionLength;
        var selected = _editor.SelectedText;

        var replacement = prefix + selected + suffix;
        _editor.Text = _editor.Text.Remove(start, len).Insert(start, replacement);
        _editor.SelectionStart = start + prefix.Length;
        _editor.SelectionLength = selected.Length;
        _editor.Focus();
    }

    private void InsertAtCaret(string value)
    {
        var start = _editor.SelectionStart;
        _editor.Text = _editor.Text.Insert(start, value);
        _editor.SelectionStart = start + value.Length;
        _editor.SelectionLength = 0;
        _editor.Focus();
    }

    private void InsertLinePrefix(string prefix)
    {
        var start = _editor.SelectionStart;
        var lineStart = _editor.Text.LastIndexOf('\n', Math.Max(0, start - 1));
        var insertPos = lineStart < 0 ? 0 : lineStart + 1;

        _editor.Text = _editor.Text.Insert(insertPos, prefix);
        _editor.SelectionStart = start + prefix.Length;
        _editor.SelectionLength = 0;
        _editor.Focus();
    }

    private void ShowPanels(bool showEditor, bool showPreview)
    {
        _mainSplit.Panel1Collapsed = !showEditor;
        _mainSplit.Panel2Collapsed = !showPreview;
    }
}
