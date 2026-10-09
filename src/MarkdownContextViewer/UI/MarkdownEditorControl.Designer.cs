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
        quickAccessStrip = new ToolStrip();
        btnSave = new ToolStripButton();
        btnUndo = new ToolStripButton();
        btnRedo = new ToolStripButton();
        sepQuick = new ToolStripSeparator();
        sepQuickEngine = new ToolStripSeparator();
        ribbonTabs = new TabControl();
        tabStart = new TabPage();
        stripStart = new ToolStrip();
        lblFont = new ToolStripLabel();
        btnBold = new ToolStripButton();
        btnItalic = new ToolStripButton();
        btnStrike = new ToolStripButton();
        btnInlineCode = new ToolStripButton();
        sepFont = new ToolStripSeparator();
        lblParagraph = new ToolStripLabel();
        btnHeading1 = new ToolStripButton();
        btnHeading2 = new ToolStripButton();
        btnHeading3 = new ToolStripButton();
        btnQuote = new ToolStripButton();
        sepParagraph = new ToolStripSeparator();
        lblLists = new ToolStripLabel();
        btnBulletList = new ToolStripButton();
        btnOrderedList = new ToolStripButton();
        btnTaskList = new ToolStripButton();
        tabInsert = new TabPage();
        stripInsert = new ToolStrip();
        lblInsert = new ToolStripLabel();
        btnLink = new ToolStripButton();
        btnImage = new ToolStripButton();
        btnCodeBlock = new ToolStripButton();
        btnTable = new ToolStripButton();
        btnHr = new ToolStripButton();
        tabView = new TabPage();
        stripView = new ToolStrip();
        lblLayout = new ToolStripLabel();
        btnEditorOnly = new ToolStripButton();
        btnPreviewOnly = new ToolStripButton();
        btnSplit = new ToolStripButton();
        sepView = new ToolStripSeparator();
        lblEngine = new ToolStripLabel();
        btnEngineWeb = new ToolStripButton();
        btnEngineWinForms = new ToolStripButton();
        mainSplit = new SplitContainer();
        editor = new RichTextBox();
        preview = new WebMarkdownView();
        winFormsPreview = new WinFormsMarkdownView();
        ribbonHost.SuspendLayout();
        quickAccessStrip.SuspendLayout();
        ribbonTabs.SuspendLayout();
        tabStart.SuspendLayout();
        stripStart.SuspendLayout();
        tabInsert.SuspendLayout();
        stripInsert.SuspendLayout();
        tabView.SuspendLayout();
        stripView.SuspendLayout();
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
        ribbonHost.Controls.Add(quickAccessStrip);
        ribbonHost.Dock = DockStyle.Top;
        ribbonHost.Name = "ribbonHost";
        ribbonHost.Padding = new Padding(4, 4, 4, 0);
        ribbonHost.Size = new Size(1100, 96);
        ribbonHost.TabIndex = 1;
        // 
        // quickAccessStrip
        // 
        quickAccessStrip.BackColor = Color.FromArgb(243, 246, 251);
        quickAccessStrip.GripStyle = ToolStripGripStyle.Hidden;
        quickAccessStrip.Items.AddRange(new ToolStripItem[] {
            btnSave,
            btnUndo,
            btnRedo,
            sepQuick,
            btnPreviewOnly,
            btnEditorOnly,
            btnSplit,
            sepQuickEngine,
            btnEngineWeb,
            btnEngineWinForms});
        quickAccessStrip.Location = new Point(4, 4);
        quickAccessStrip.Name = "quickAccessStrip";
        quickAccessStrip.Padding = new Padding(2, 1, 1, 1);
        quickAccessStrip.RenderMode = ToolStripRenderMode.System;
        quickAccessStrip.Size = new Size(1092, 27);
        quickAccessStrip.TabIndex = 1;
        // 
        // btnSave
        // 
        btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(36, 24);
        btnSave.Text = "保存";
        btnSave.ToolTipText = "保存 (Ctrl+S)";
        btnSave.Click += btnSave_Click;
        // 
        // btnUndo
        // 
        btnUndo.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnUndo.Name = "btnUndo";
        btnUndo.Size = new Size(36, 24);
        btnUndo.Text = "撤销";
        btnUndo.ToolTipText = "撤销 (Ctrl+Z)";
        btnUndo.Click += btnUndo_Click;
        // 
        // btnRedo
        // 
        btnRedo.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRedo.Name = "btnRedo";
        btnRedo.Size = new Size(36, 24);
        btnRedo.Text = "重做";
        btnRedo.ToolTipText = "重做 (Ctrl+Y)";
        btnRedo.Click += btnRedo_Click;
        // 
        // ribbonTabs
        // 
        ribbonTabs.Controls.Add(tabStart);
        ribbonTabs.Controls.Add(tabInsert);
        ribbonTabs.Controls.Add(tabView);
        ribbonTabs.Dock = DockStyle.Fill;
        ribbonTabs.Location = new Point(4, 31);
        ribbonTabs.Name = "ribbonTabs";
        ribbonTabs.SelectedIndex = 0;
        ribbonTabs.Size = new Size(1092, 65);
        ribbonTabs.TabIndex = 0;
        // 
        // tabStart
        // 
        tabStart.Controls.Add(stripStart);
        tabStart.Location = new Point(4, 26);
        tabStart.Name = "tabStart";
        tabStart.Padding = new Padding(0);
        tabStart.Size = new Size(1084, 35);
        tabStart.TabIndex = 0;
        tabStart.Text = "开始";
        tabStart.UseVisualStyleBackColor = true;
        // 
        // stripStart
        // 
        stripStart.CanOverflow = true;
        stripStart.Dock = DockStyle.Fill;
        stripStart.GripStyle = ToolStripGripStyle.Hidden;
        stripStart.Items.AddRange(new ToolStripItem[]
        {
            lblFont, btnBold, btnItalic, btnStrike, btnInlineCode,
            sepFont,
            lblParagraph, btnHeading1, btnHeading2, btnHeading3, btnQuote,
            sepParagraph,
            lblLists, btnBulletList, btnOrderedList, btnTaskList
        });
        stripStart.Location = new Point(0, 0);
        stripStart.Name = "stripStart";
        stripStart.Padding = new Padding(4, 2, 1, 2);
        stripStart.RenderMode = ToolStripRenderMode.System;
        stripStart.Size = new Size(1084, 35);
        stripStart.TabIndex = 0;
        // 
        // lblFont
        // 
        lblFont.ForeColor = Color.FromArgb(96, 104, 116);
        lblFont.Name = "lblFont";
        lblFont.Size = new Size(32, 28);
        lblFont.Text = "字体";
        // 
        // btnBold
        // 
        btnBold.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnBold.Name = "btnBold";
        btnBold.Size = new Size(36, 28);
        btnBold.Text = "粗体";
        btnBold.ToolTipText = "粗体 (Ctrl+B)";
        btnBold.Click += btnBold_Click;
        // 
        // btnItalic
        // 
        btnItalic.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnItalic.Name = "btnItalic";
        btnItalic.Size = new Size(36, 28);
        btnItalic.Text = "斜体";
        btnItalic.ToolTipText = "斜体 (Ctrl+I)";
        btnItalic.Click += btnItalic_Click;
        // 
        // btnStrike
        // 
        btnStrike.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnStrike.Name = "btnStrike";
        btnStrike.Size = new Size(48, 28);
        btnStrike.Text = "删除线";
        btnStrike.ToolTipText = "删除线 (Ctrl+Shift+X)";
        btnStrike.Click += btnStrike_Click;
        // 
        // btnInlineCode
        // 
        btnInlineCode.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnInlineCode.Name = "btnInlineCode";
        btnInlineCode.Size = new Size(60, 28);
        btnInlineCode.Text = "行内代码";
        btnInlineCode.ToolTipText = "行内代码 (Ctrl+`)";
        btnInlineCode.Click += btnInlineCode_Click;
        // 
        // sepFont
        // 
        sepFont.Name = "sepFont";
        sepFont.Size = new Size(6, 31);
        // 
        // lblParagraph
        // 
        lblParagraph.ForeColor = Color.FromArgb(96, 104, 116);
        lblParagraph.Name = "lblParagraph";
        lblParagraph.Size = new Size(32, 28);
        lblParagraph.Text = "段落";
        // 
        // btnHeading1
        // 
        btnHeading1.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnHeading1.Name = "btnHeading1";
        btnHeading1.Size = new Size(29, 28);
        btnHeading1.Text = "H1";
        btnHeading1.ToolTipText = "一级标题 (Ctrl+1)";
        btnHeading1.Click += btnHeading1_Click;
        // 
        // btnHeading2
        // 
        btnHeading2.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnHeading2.Name = "btnHeading2";
        btnHeading2.Size = new Size(29, 28);
        btnHeading2.Text = "H2";
        btnHeading2.ToolTipText = "二级标题 (Ctrl+2)";
        btnHeading2.Click += btnHeading2_Click;
        // 
        // btnHeading3
        // 
        btnHeading3.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnHeading3.Name = "btnHeading3";
        btnHeading3.Size = new Size(29, 28);
        btnHeading3.Text = "H3";
        btnHeading3.ToolTipText = "三级标题 (Ctrl+3)";
        btnHeading3.Click += btnHeading3_Click;
        // 
        // btnQuote
        // 
        btnQuote.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnQuote.Name = "btnQuote";
        btnQuote.Size = new Size(36, 28);
        btnQuote.Text = "引用";
        btnQuote.ToolTipText = "引用（支持多行，再次点击取消）";
        btnQuote.Click += btnQuote_Click;
        // 
        // sepParagraph
        // 
        sepParagraph.Name = "sepParagraph";
        sepParagraph.Size = new Size(6, 31);
        // 
        // lblLists
        // 
        lblLists.ForeColor = Color.FromArgb(96, 104, 116);
        lblLists.Name = "lblLists";
        lblLists.Size = new Size(32, 28);
        lblLists.Text = "列表";
        // 
        // btnBulletList
        // 
        btnBulletList.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnBulletList.Name = "btnBulletList";
        btnBulletList.Size = new Size(36, 28);
        btnBulletList.Text = "无序";
        btnBulletList.ToolTipText = "无序列表 (Ctrl+Shift+8)";
        btnBulletList.Click += btnBulletList_Click;
        // 
        // btnOrderedList
        // 
        btnOrderedList.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnOrderedList.Name = "btnOrderedList";
        btnOrderedList.Size = new Size(36, 28);
        btnOrderedList.Text = "有序";
        btnOrderedList.ToolTipText = "有序列表 (Ctrl+Shift+7)";
        btnOrderedList.Click += btnOrderedList_Click;
        // 
        // btnTaskList
        // 
        btnTaskList.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnTaskList.Name = "btnTaskList";
        btnTaskList.Size = new Size(36, 28);
        btnTaskList.Text = "任务";
        btnTaskList.ToolTipText = "任务列表";
        btnTaskList.Click += btnTaskList_Click;
        // 
        // tabInsert
        // 
        tabInsert.Controls.Add(stripInsert);
        tabInsert.Location = new Point(4, 26);
        tabInsert.Name = "tabInsert";
        tabInsert.Padding = new Padding(0);
        tabInsert.Size = new Size(1084, 35);
        tabInsert.TabIndex = 1;
        tabInsert.Text = "插入";
        tabInsert.UseVisualStyleBackColor = true;
        // 
        // stripInsert
        // 
        stripInsert.CanOverflow = true;
        stripInsert.Dock = DockStyle.Fill;
        stripInsert.GripStyle = ToolStripGripStyle.Hidden;
        stripInsert.Items.AddRange(new ToolStripItem[]
        {
            lblInsert, btnLink, btnImage, btnCodeBlock, btnTable, btnHr
        });
        stripInsert.Location = new Point(0, 0);
        stripInsert.Name = "stripInsert";
        stripInsert.Padding = new Padding(4, 2, 1, 2);
        stripInsert.RenderMode = ToolStripRenderMode.System;
        stripInsert.Size = new Size(1084, 35);
        stripInsert.TabIndex = 0;
        // 
        // lblInsert
        // 
        lblInsert.ForeColor = Color.FromArgb(96, 104, 116);
        lblInsert.Name = "lblInsert";
        lblInsert.Size = new Size(32, 28);
        lblInsert.Text = "常用";
        // 
        // btnLink
        // 
        btnLink.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnLink.Name = "btnLink";
        btnLink.Size = new Size(36, 28);
        btnLink.Text = "链接";
        btnLink.ToolTipText = "链接 (Ctrl+K)，选中文本将作为链接文字";
        btnLink.Click += btnLink_Click;
        // 
        // btnImage
        // 
        btnImage.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnImage.Name = "btnImage";
        btnImage.Size = new Size(36, 28);
        btnImage.Text = "图片";
        btnImage.ToolTipText = "图片";
        btnImage.Click += btnImage_Click;
        // 
        // btnCodeBlock
        // 
        btnCodeBlock.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnCodeBlock.Name = "btnCodeBlock";
        btnCodeBlock.Size = new Size(48, 28);
        btnCodeBlock.Text = "代码块";
        btnCodeBlock.ToolTipText = "代码块 (Ctrl+Shift+K)";
        btnCodeBlock.Click += btnCodeBlock_Click;
        // 
        // btnTable
        // 
        btnTable.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnTable.Name = "btnTable";
        btnTable.Size = new Size(36, 28);
        btnTable.Text = "表格";
        btnTable.ToolTipText = "插入表格";
        btnTable.Click += btnTable_Click;
        // 
        // btnHr
        // 
        btnHr.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnHr.Name = "btnHr";
        btnHr.Size = new Size(48, 28);
        btnHr.Text = "分隔线";
        btnHr.ToolTipText = "分隔线";
        btnHr.Click += btnHr_Click;
        // 
        // tabView
        // 
        tabView.Controls.Add(stripView);
        tabView.Location = new Point(4, 26);
        tabView.Name = "tabView";
        tabView.Padding = new Padding(0);
        tabView.Size = new Size(1084, 35);
        tabView.TabIndex = 2;
        tabView.Text = "视图";
        tabView.UseVisualStyleBackColor = true;
        // 
        // stripView
        // 
        stripView.CanOverflow = true;
        stripView.Dock = DockStyle.Fill;
        stripView.GripStyle = ToolStripGripStyle.Hidden;
        stripView.Items.AddRange(new ToolStripItem[]
        {
            lblLayout, lblEngine
        });
        stripView.Location = new Point(0, 0);
        stripView.Name = "stripView";
        stripView.Padding = new Padding(4, 2, 1, 2);
        stripView.RenderMode = ToolStripRenderMode.System;
        stripView.Size = new Size(1084, 35);
        stripView.TabIndex = 0;
        // 
        // lblLayout
        // 
        lblLayout.ForeColor = Color.FromArgb(96, 104, 116);
        lblLayout.Name = "lblLayout";
        lblLayout.Size = new Size(280, 28);
        lblLayout.Text = "布局与引擎切换在上方快捷栏";
        // 
        // sepQuick / 布局按钮（挂在快捷栏，预览模式也能切换）
        // 
        sepQuick.Name = "sepQuick";
        sepQuick.Size = new Size(6, 24);
        // 
        // btnPreviewOnly
        // 
        btnPreviewOnly.CheckOnClick = true;
        btnPreviewOnly.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnPreviewOnly.Name = "btnPreviewOnly";
        btnPreviewOnly.Size = new Size(48, 24);
        btnPreviewOnly.Text = "仅预览";
        btnPreviewOnly.ToolTipText = "仅预览（Esc）";
        btnPreviewOnly.Click += btnPreviewOnly_Click;
        // 
        // btnEditorOnly
        // 
        btnEditorOnly.CheckOnClick = true;
        btnEditorOnly.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnEditorOnly.Name = "btnEditorOnly";
        btnEditorOnly.Size = new Size(48, 24);
        btnEditorOnly.Text = "仅编辑";
        btnEditorOnly.ToolTipText = "仅编辑（Ctrl+E）";
        btnEditorOnly.Click += btnEditorOnly_Click;
        // 
        // btnSplit
        // 
        btnSplit.CheckOnClick = true;
        btnSplit.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSplit.Name = "btnSplit";
        btnSplit.Size = new Size(36, 24);
        btnSplit.Text = "分栏";
        btnSplit.ToolTipText = "编辑/预览分栏";
        btnSplit.Click += btnSplit_Click;
        // 
        // sepQuickEngine
        // 
        sepQuickEngine.Name = "sepQuickEngine";
        sepQuickEngine.Size = new Size(6, 24);
        // 
        // sepView
        // 
        sepView.Name = "sepView";
        sepView.Size = new Size(6, 31);
        // 
        // lblEngine
        // 
        lblEngine.ForeColor = Color.FromArgb(96, 104, 116);
        lblEngine.Name = "lblEngine";
        lblEngine.Size = new Size(200, 28);
        lblEngine.Text = "快捷键：Ctrl+E 编辑，Esc 预览";
        // 
        // btnEngineWeb
        // 
        btnEngineWeb.CheckOnClick = true;
        btnEngineWeb.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnEngineWeb.Name = "btnEngineWeb";
        btnEngineWeb.Size = new Size(100, 24);
        btnEngineWeb.Text = "Web（还原度高）★";
        btnEngineWeb.ToolTipText = "WebBrowser：Markdown 排版还原度高，启动与内存开销更大";
        btnEngineWeb.Click += btnEngineWeb_Click;
        // 
        // btnEngineWinForms
        // 
        btnEngineWinForms.CheckOnClick = true;
        btnEngineWinForms.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnEngineWinForms.Name = "btnEngineWinForms";
        btnEngineWinForms.Size = new Size(120, 24);
        btnEngineWinForms.Text = "WinForms（更快更省）";
        btnEngineWinForms.ToolTipText = "RichTextBox：启动快、更省内存，适合大文件；Markdown 还原度有限";
        btnEngineWinForms.Click += btnEngineWinForms_Click;
        // 
        // mainSplit
        // 
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.Location = new Point(0, 96);
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
        mainSplit.Size = new Size(1100, 544);
        mainSplit.SplitterDistance = 550;
        mainSplit.TabIndex = 0;
        // 
        // editor
        // 
        editor.AcceptsTab = true;
        editor.DetectUrls = false;
        editor.Dock = DockStyle.Fill;
        editor.Font = new Font("Consolas", 11F);
        editor.HideSelection = false;
        editor.Location = new Point(0, 0);
        editor.Name = "editor";
        editor.ScrollBars = RichTextBoxScrollBars.Both;
        editor.Size = new Size(550, 544);
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
        preview.Name = "preview";
        preview.Size = new Size(546, 544);
        preview.TabIndex = 1;
        // 
        // winFormsPreview
        // 
        winFormsPreview.Dock = DockStyle.Fill;
        winFormsPreview.Location = new Point(0, 0);
        winFormsPreview.Name = "winFormsPreview";
        winFormsPreview.Size = new Size(546, 544);
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
        Size = new Size(1100, 640);
        ribbonHost.ResumeLayout(false);
        ribbonHost.PerformLayout();
        quickAccessStrip.ResumeLayout(false);
        quickAccessStrip.PerformLayout();
        ribbonTabs.ResumeLayout(false);
        tabStart.ResumeLayout(false);
        tabStart.PerformLayout();
        stripStart.ResumeLayout(false);
        stripStart.PerformLayout();
        tabInsert.ResumeLayout(false);
        tabInsert.PerformLayout();
        stripInsert.ResumeLayout(false);
        stripInsert.PerformLayout();
        tabView.ResumeLayout(false);
        tabView.PerformLayout();
        stripView.ResumeLayout(false);
        stripView.PerformLayout();
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel ribbonHost = null!;
    private ToolStrip quickAccessStrip = null!;
    private ToolStripButton btnSave = null!;
    private ToolStripButton btnUndo = null!;
    private ToolStripButton btnRedo = null!;
    private ToolStripSeparator sepQuick = null!;
    private ToolStripSeparator sepQuickEngine = null!;
    private TabControl ribbonTabs = null!;
    private TabPage tabStart = null!;
    private ToolStrip stripStart = null!;
    private ToolStripLabel lblFont = null!;
    private ToolStripButton btnBold = null!;
    private ToolStripButton btnItalic = null!;
    private ToolStripButton btnStrike = null!;
    private ToolStripButton btnInlineCode = null!;
    private ToolStripSeparator sepFont = null!;
    private ToolStripLabel lblParagraph = null!;
    private ToolStripButton btnHeading1 = null!;
    private ToolStripButton btnHeading2 = null!;
    private ToolStripButton btnHeading3 = null!;
    private ToolStripButton btnQuote = null!;
    private ToolStripSeparator sepParagraph = null!;
    private ToolStripLabel lblLists = null!;
    private ToolStripButton btnBulletList = null!;
    private ToolStripButton btnOrderedList = null!;
    private ToolStripButton btnTaskList = null!;
    private TabPage tabInsert = null!;
    private ToolStrip stripInsert = null!;
    private ToolStripLabel lblInsert = null!;
    private ToolStripButton btnLink = null!;
    private ToolStripButton btnImage = null!;
    private ToolStripButton btnCodeBlock = null!;
    private ToolStripButton btnTable = null!;
    private ToolStripButton btnHr = null!;
    private TabPage tabView = null!;
    private ToolStrip stripView = null!;
    private ToolStripLabel lblLayout = null!;
    private ToolStripButton btnEditorOnly = null!;
    private ToolStripButton btnPreviewOnly = null!;
    private ToolStripButton btnSplit = null!;
    private ToolStripSeparator sepView = null!;
    private ToolStripLabel lblEngine = null!;
    private ToolStripButton btnEngineWeb = null!;
    private ToolStripButton btnEngineWinForms = null!;
    private SplitContainer mainSplit = null!;
    private RichTextBox editor = null!;
    private WebMarkdownView preview = null!;
    private WinFormsMarkdownView winFormsPreview = null!;
}
