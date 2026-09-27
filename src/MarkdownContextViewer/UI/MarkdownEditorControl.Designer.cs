namespace MarkdownContextViewer;

partial class MarkdownEditorControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        ribbonHost = new Panel();
        quickAccessBar = new FlowLayoutPanel();
        btnSave = new Button();
        btnUndo = new Button();
        btnRedo = new Button();
        ribbonTabs = new TabControl();
        tabStart = new TabPage();
        flowStart = new FlowLayoutPanel();
        grpFont = new GroupBox();
        flowFont = new FlowLayoutPanel();
        btnBold = new Button();
        btnItalic = new Button();
        btnInlineCode = new Button();
        grpParagraph = new GroupBox();
        flowParagraph = new FlowLayoutPanel();
        btnHeading = new Button();
        btnQuote = new Button();
        btnList = new Button();
        tabInsert = new TabPage();
        flowInsert = new FlowLayoutPanel();
        grpCommon = new GroupBox();
        flowCommon = new FlowLayoutPanel();
        btnLink = new Button();
        btnImage = new Button();
        btnCodeBlock = new Button();
        tabView = new TabPage();
        flowView = new FlowLayoutPanel();
        grpLayout = new GroupBox();
        flowLayout = new FlowLayoutPanel();
        btnEditorOnly = new Button();
        btnPreviewOnly = new Button();
        btnSplit = new Button();
        mainSplit = new SplitContainer();
        editor = new RichTextBox();
        preview = new WebBrowser();
        winFormsPreview = new WinFormsMarkdownView();
        ribbonHost.SuspendLayout();
        quickAccessBar.SuspendLayout();
        ribbonTabs.SuspendLayout();
        tabStart.SuspendLayout();
        flowStart.SuspendLayout();
        grpFont.SuspendLayout();
        flowFont.SuspendLayout();
        grpParagraph.SuspendLayout();
        flowParagraph.SuspendLayout();
        tabInsert.SuspendLayout();
        flowInsert.SuspendLayout();
        grpCommon.SuspendLayout();
        flowCommon.SuspendLayout();
        tabView.SuspendLayout();
        flowView.SuspendLayout();
        grpLayout.SuspendLayout();
        flowLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
        mainSplit.Panel1.SuspendLayout();
        mainSplit.Panel2.SuspendLayout();
        mainSplit.SuspendLayout();
        SuspendLayout();
        // 
        // ribbonHost
        // 
        ribbonHost.BackColor = Color.FromArgb(243, 246, 251);
        ribbonHost.Controls.Add(ribbonTabs);
        ribbonHost.Controls.Add(quickAccessBar);
        ribbonHost.Dock = DockStyle.Top;
        ribbonHost.Location = new Point(0, 0);
        ribbonHost.Name = "ribbonHost";
        ribbonHost.Padding = new Padding(6, 6, 6, 0);
        ribbonHost.Size = new Size(960, 168);
        ribbonHost.TabIndex = 1;
        // 
        // quickAccessBar
        // 
        quickAccessBar.Controls.Add(btnSave);
        quickAccessBar.Controls.Add(btnUndo);
        quickAccessBar.Controls.Add(btnRedo);
        quickAccessBar.Dock = DockStyle.Top;
        quickAccessBar.FlowDirection = FlowDirection.LeftToRight;
        quickAccessBar.Location = new Point(6, 6);
        quickAccessBar.Name = "quickAccessBar";
        quickAccessBar.Size = new Size(948, 34);
        quickAccessBar.TabIndex = 1;
        quickAccessBar.WrapContents = false;
        // 
        // btnSave
        // 
        btnSave.Location = new Point(0, 0);
        btnSave.Margin = new Padding(0, 0, 6, 0);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(72, 28);
        btnSave.TabIndex = 0;
        btnSave.Text = "保存";
        btnSave.Click += btnSave_Click;
        // 
        // btnUndo
        // 
        btnUndo.Location = new Point(78, 0);
        btnUndo.Margin = new Padding(0, 0, 6, 0);
        btnUndo.Name = "btnUndo";
        btnUndo.Size = new Size(72, 28);
        btnUndo.TabIndex = 1;
        btnUndo.Text = "撤销";
        btnUndo.Click += btnUndo_Click;
        // 
        // btnRedo
        // 
        btnRedo.Location = new Point(156, 0);
        btnRedo.Margin = new Padding(0, 0, 6, 0);
        btnRedo.Name = "btnRedo";
        btnRedo.Size = new Size(72, 28);
        btnRedo.TabIndex = 2;
        btnRedo.Text = "重做";
        btnRedo.Click += btnRedo_Click;
        // 
        // ribbonTabs
        // 
        ribbonTabs.Controls.Add(tabStart);
        ribbonTabs.Controls.Add(tabInsert);
        ribbonTabs.Controls.Add(tabView);
        ribbonTabs.Dock = DockStyle.Fill;
        ribbonTabs.Location = new Point(6, 40);
        ribbonTabs.Multiline = false;
        ribbonTabs.Name = "ribbonTabs";
        ribbonTabs.SelectedIndex = 0;
        ribbonTabs.Size = new Size(948, 128);
        ribbonTabs.TabIndex = 0;
        // 
        // tabStart
        // 
        tabStart.Controls.Add(flowStart);
        tabStart.Location = new Point(4, 26);
        tabStart.Name = "tabStart";
        tabStart.Padding = new Padding(3);
        tabStart.Size = new Size(940, 98);
        tabStart.TabIndex = 0;
        tabStart.Text = "开始";
        tabStart.UseVisualStyleBackColor = true;
        // 
        // flowStart
        // 
        flowStart.AutoScroll = true;
        flowStart.Controls.Add(grpFont);
        flowStart.Controls.Add(grpParagraph);
        flowStart.Dock = DockStyle.Fill;
        flowStart.FlowDirection = FlowDirection.LeftToRight;
        flowStart.Location = new Point(3, 3);
        flowStart.Name = "flowStart";
        flowStart.Padding = new Padding(8, 6, 8, 6);
        flowStart.Size = new Size(934, 92);
        flowStart.TabIndex = 0;
        flowStart.WrapContents = false;
        // 
        // grpFont
        // 
        grpFont.Controls.Add(flowFont);
        grpFont.Location = new Point(14, 6);
        grpFont.Margin = new Padding(6, 0, 6, 0);
        grpFont.Name = "grpFont";
        grpFont.Size = new Size(330, 72);
        grpFont.TabIndex = 0;
        grpFont.TabStop = false;
        grpFont.Text = "字体";
        // 
        // flowFont
        // 
        flowFont.Controls.Add(btnBold);
        flowFont.Controls.Add(btnItalic);
        flowFont.Controls.Add(btnInlineCode);
        flowFont.Dock = DockStyle.Fill;
        flowFont.FlowDirection = FlowDirection.LeftToRight;
        flowFont.Location = new Point(3, 19);
        flowFont.Name = "flowFont";
        flowFont.Padding = new Padding(6, 4, 6, 6);
        flowFont.Size = new Size(324, 50);
        flowFont.TabIndex = 0;
        flowFont.WrapContents = false;
        // 
        // btnBold
        // 
        btnBold.Location = new Point(10, 4);
        btnBold.Margin = new Padding(4);
        btnBold.Name = "btnBold";
        btnBold.Size = new Size(96, 28);
        btnBold.TabIndex = 0;
        btnBold.Text = "粗体";
        btnBold.Click += btnBold_Click;
        // 
        // btnItalic
        // 
        btnItalic.Location = new Point(114, 4);
        btnItalic.Margin = new Padding(4);
        btnItalic.Name = "btnItalic";
        btnItalic.Size = new Size(96, 28);
        btnItalic.TabIndex = 1;
        btnItalic.Text = "斜体";
        btnItalic.Click += btnItalic_Click;
        // 
        // btnInlineCode
        // 
        btnInlineCode.Location = new Point(218, 4);
        btnInlineCode.Margin = new Padding(4);
        btnInlineCode.Name = "btnInlineCode";
        btnInlineCode.Size = new Size(96, 28);
        btnInlineCode.TabIndex = 2;
        btnInlineCode.Text = "行内代码";
        btnInlineCode.Click += btnInlineCode_Click;
        // 
        // grpParagraph
        // 
        grpParagraph.Controls.Add(flowParagraph);
        grpParagraph.Location = new Point(356, 6);
        grpParagraph.Margin = new Padding(6, 0, 6, 0);
        grpParagraph.Name = "grpParagraph";
        grpParagraph.Size = new Size(330, 72);
        grpParagraph.TabIndex = 1;
        grpParagraph.TabStop = false;
        grpParagraph.Text = "段落";
        // 
        // flowParagraph
        // 
        flowParagraph.Controls.Add(btnHeading);
        flowParagraph.Controls.Add(btnQuote);
        flowParagraph.Controls.Add(btnList);
        flowParagraph.Dock = DockStyle.Fill;
        flowParagraph.FlowDirection = FlowDirection.LeftToRight;
        flowParagraph.Location = new Point(3, 19);
        flowParagraph.Name = "flowParagraph";
        flowParagraph.Padding = new Padding(6, 4, 6, 6);
        flowParagraph.Size = new Size(324, 50);
        flowParagraph.TabIndex = 0;
        flowParagraph.WrapContents = false;
        // 
        // btnHeading
        // 
        btnHeading.Location = new Point(10, 4);
        btnHeading.Margin = new Padding(4);
        btnHeading.Name = "btnHeading";
        btnHeading.Size = new Size(96, 28);
        btnHeading.TabIndex = 0;
        btnHeading.Text = "标题";
        btnHeading.Click += btnHeading_Click;
        // 
        // btnQuote
        // 
        btnQuote.Location = new Point(114, 4);
        btnQuote.Margin = new Padding(4);
        btnQuote.Name = "btnQuote";
        btnQuote.Size = new Size(96, 28);
        btnQuote.TabIndex = 1;
        btnQuote.Text = "引用";
        btnQuote.Click += btnQuote_Click;
        // 
        // btnList
        // 
        btnList.Location = new Point(218, 4);
        btnList.Margin = new Padding(4);
        btnList.Name = "btnList";
        btnList.Size = new Size(96, 28);
        btnList.TabIndex = 2;
        btnList.Text = "列表";
        btnList.Click += btnList_Click;
        // 
        // tabInsert
        // 
        tabInsert.Controls.Add(flowInsert);
        tabInsert.Location = new Point(4, 26);
        tabInsert.Name = "tabInsert";
        tabInsert.Padding = new Padding(3);
        tabInsert.Size = new Size(940, 98);
        tabInsert.TabIndex = 1;
        tabInsert.Text = "插入";
        tabInsert.UseVisualStyleBackColor = true;
        // 
        // flowInsert
        // 
        flowInsert.AutoScroll = true;
        flowInsert.Controls.Add(grpCommon);
        flowInsert.Dock = DockStyle.Fill;
        flowInsert.FlowDirection = FlowDirection.LeftToRight;
        flowInsert.Location = new Point(3, 3);
        flowInsert.Name = "flowInsert";
        flowInsert.Padding = new Padding(8, 6, 8, 6);
        flowInsert.Size = new Size(934, 92);
        flowInsert.TabIndex = 0;
        flowInsert.WrapContents = false;
        // 
        // grpCommon
        // 
        grpCommon.Controls.Add(flowCommon);
        grpCommon.Location = new Point(14, 6);
        grpCommon.Margin = new Padding(6, 0, 6, 0);
        grpCommon.Name = "grpCommon";
        grpCommon.Size = new Size(330, 72);
        grpCommon.TabIndex = 0;
        grpCommon.TabStop = false;
        grpCommon.Text = "常用";
        // 
        // flowCommon
        // 
        flowCommon.Controls.Add(btnLink);
        flowCommon.Controls.Add(btnImage);
        flowCommon.Controls.Add(btnCodeBlock);
        flowCommon.Dock = DockStyle.Fill;
        flowCommon.FlowDirection = FlowDirection.LeftToRight;
        flowCommon.Location = new Point(3, 19);
        flowCommon.Name = "flowCommon";
        flowCommon.Padding = new Padding(6, 4, 6, 6);
        flowCommon.Size = new Size(324, 50);
        flowCommon.TabIndex = 0;
        flowCommon.WrapContents = false;
        // 
        // btnLink
        // 
        btnLink.Location = new Point(10, 4);
        btnLink.Margin = new Padding(4);
        btnLink.Name = "btnLink";
        btnLink.Size = new Size(96, 28);
        btnLink.TabIndex = 0;
        btnLink.Text = "链接";
        btnLink.Click += btnLink_Click;
        // 
        // btnImage
        // 
        btnImage.Location = new Point(114, 4);
        btnImage.Margin = new Padding(4);
        btnImage.Name = "btnImage";
        btnImage.Size = new Size(96, 28);
        btnImage.TabIndex = 1;
        btnImage.Text = "图片";
        btnImage.Click += btnImage_Click;
        // 
        // btnCodeBlock
        // 
        btnCodeBlock.Location = new Point(218, 4);
        btnCodeBlock.Margin = new Padding(4);
        btnCodeBlock.Name = "btnCodeBlock";
        btnCodeBlock.Size = new Size(96, 28);
        btnCodeBlock.TabIndex = 2;
        btnCodeBlock.Text = "代码块";
        btnCodeBlock.Click += btnCodeBlock_Click;
        // 
        // tabView
        // 
        tabView.Controls.Add(flowView);
        tabView.Location = new Point(4, 26);
        tabView.Name = "tabView";
        tabView.Padding = new Padding(3);
        tabView.Size = new Size(940, 98);
        tabView.TabIndex = 2;
        tabView.Text = "视图";
        tabView.UseVisualStyleBackColor = true;
        // 
        // flowView
        // 
        flowView.AutoScroll = true;
        flowView.Controls.Add(grpLayout);
        flowView.Dock = DockStyle.Fill;
        flowView.FlowDirection = FlowDirection.LeftToRight;
        flowView.Location = new Point(3, 3);
        flowView.Name = "flowView";
        flowView.Padding = new Padding(8, 6, 8, 6);
        flowView.Size = new Size(934, 92);
        flowView.TabIndex = 0;
        flowView.WrapContents = false;
        // 
        // grpLayout
        // 
        grpLayout.Controls.Add(flowLayout);
        grpLayout.Location = new Point(14, 6);
        grpLayout.Margin = new Padding(6, 0, 6, 0);
        grpLayout.Name = "grpLayout";
        grpLayout.Size = new Size(330, 72);
        grpLayout.TabIndex = 0;
        grpLayout.TabStop = false;
        grpLayout.Text = "布局";
        // 
        // flowLayout
        // 
        flowLayout.Controls.Add(btnEditorOnly);
        flowLayout.Controls.Add(btnPreviewOnly);
        flowLayout.Controls.Add(btnSplit);
        flowLayout.Dock = DockStyle.Fill;
        flowLayout.FlowDirection = FlowDirection.LeftToRight;
        flowLayout.Location = new Point(3, 19);
        flowLayout.Name = "flowLayout";
        flowLayout.Padding = new Padding(6, 4, 6, 6);
        flowLayout.Size = new Size(324, 50);
        flowLayout.TabIndex = 0;
        flowLayout.WrapContents = false;
        // 
        // btnEditorOnly
        // 
        btnEditorOnly.Location = new Point(10, 4);
        btnEditorOnly.Margin = new Padding(4);
        btnEditorOnly.Name = "btnEditorOnly";
        btnEditorOnly.Size = new Size(96, 28);
        btnEditorOnly.TabIndex = 0;
        btnEditorOnly.Text = "仅编辑";
        btnEditorOnly.Click += btnEditorOnly_Click;
        // 
        // btnPreviewOnly
        // 
        btnPreviewOnly.Location = new Point(114, 4);
        btnPreviewOnly.Margin = new Padding(4);
        btnPreviewOnly.Name = "btnPreviewOnly";
        btnPreviewOnly.Size = new Size(96, 28);
        btnPreviewOnly.TabIndex = 1;
        btnPreviewOnly.Text = "仅预览";
        btnPreviewOnly.Click += btnPreviewOnly_Click;
        // 
        // btnSplit
        // 
        btnSplit.Location = new Point(218, 4);
        btnSplit.Margin = new Padding(4);
        btnSplit.Name = "btnSplit";
        btnSplit.Size = new Size(96, 28);
        btnSplit.TabIndex = 2;
        btnSplit.Text = "分栏";
        btnSplit.Click += btnSplit_Click;
        // 
        // mainSplit
        // 
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.Location = new Point(0, 168);
        mainSplit.Name = "mainSplit";
        mainSplit.Orientation = Orientation.Vertical;
        // 
        // mainSplit.Panel1
        // 
        mainSplit.Panel1.Controls.Add(editor);
        // 
        // mainSplit.Panel2
        // 
        mainSplit.Panel2.Controls.Add(winFormsPreview);
        mainSplit.Panel2.Controls.Add(preview);
        mainSplit.Size = new Size(960, 472);
        mainSplit.SplitterDistance = 480;
        mainSplit.TabIndex = 0;
        // 
        // editor
        // 
        editor.AcceptsTab = true;
        editor.DetectUrls = false;
        editor.Dock = DockStyle.Fill;
        editor.Font = new Font("Consolas", 11F);
        editor.Location = new Point(0, 0);
        editor.Name = "editor";
        editor.ScrollBars = RichTextBoxScrollBars.Both;
        editor.Size = new Size(480, 472);
        editor.TabIndex = 0;
        editor.Text = "";
        editor.WordWrap = false;
        editor.TextChanged += editor_TextChanged;
        editor.KeyDown += editor_KeyDown;
        // 
        // preview
        // 
        preview.Dock = DockStyle.Fill;
        preview.Location = new Point(0, 0);
        preview.MinimumSize = new Size(20, 20);
        preview.Name = "preview";
        preview.ScriptErrorsSuppressed = true;
        preview.Size = new Size(476, 472);
        preview.TabIndex = 1;
        // 
        // winFormsPreview
        // 
        winFormsPreview.Dock = DockStyle.Fill;
        winFormsPreview.Location = new Point(0, 0);
        winFormsPreview.Name = "winFormsPreview";
        winFormsPreview.Size = new Size(476, 472);
        winFormsPreview.TabIndex = 0;
        winFormsPreview.Visible = false;
        // 
        // MarkdownEditorControl
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(mainSplit);
        Controls.Add(ribbonHost);
        Name = "MarkdownEditorControl";
        Size = new Size(960, 640);
        ribbonHost.ResumeLayout(false);
        quickAccessBar.ResumeLayout(false);
        ribbonTabs.ResumeLayout(false);
        tabStart.ResumeLayout(false);
        flowStart.ResumeLayout(false);
        grpFont.ResumeLayout(false);
        flowFont.ResumeLayout(false);
        grpParagraph.ResumeLayout(false);
        flowParagraph.ResumeLayout(false);
        tabInsert.ResumeLayout(false);
        flowInsert.ResumeLayout(false);
        grpCommon.ResumeLayout(false);
        flowCommon.ResumeLayout(false);
        tabView.ResumeLayout(false);
        flowView.ResumeLayout(false);
        grpLayout.ResumeLayout(false);
        flowLayout.ResumeLayout(false);
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel ribbonHost = null!;
    private FlowLayoutPanel quickAccessBar = null!;
    private Button btnSave = null!;
    private Button btnUndo = null!;
    private Button btnRedo = null!;
    private TabControl ribbonTabs = null!;
    private TabPage tabStart = null!;
    private FlowLayoutPanel flowStart = null!;
    private GroupBox grpFont = null!;
    private FlowLayoutPanel flowFont = null!;
    private Button btnBold = null!;
    private Button btnItalic = null!;
    private Button btnInlineCode = null!;
    private GroupBox grpParagraph = null!;
    private FlowLayoutPanel flowParagraph = null!;
    private Button btnHeading = null!;
    private Button btnQuote = null!;
    private Button btnList = null!;
    private TabPage tabInsert = null!;
    private FlowLayoutPanel flowInsert = null!;
    private GroupBox grpCommon = null!;
    private FlowLayoutPanel flowCommon = null!;
    private Button btnLink = null!;
    private Button btnImage = null!;
    private Button btnCodeBlock = null!;
    private TabPage tabView = null!;
    private FlowLayoutPanel flowView = null!;
    private GroupBox grpLayout = null!;
    private FlowLayoutPanel flowLayout = null!;
    private Button btnEditorOnly = null!;
    private Button btnPreviewOnly = null!;
    private Button btnSplit = null!;
    private SplitContainer mainSplit = null!;
    private RichTextBox editor = null!;
    private WebBrowser preview = null!;
    private WinFormsMarkdownView winFormsPreview = null!;
}
