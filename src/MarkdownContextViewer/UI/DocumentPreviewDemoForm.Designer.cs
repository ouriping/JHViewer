namespace MarkdownContextViewer;

partial class DocumentPreviewDemoForm
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
        toolStrip = new ToolStrip();
        btnOpen = new ToolStripButton();
        btnRefresh = new ToolStripButton();
        sep1 = new ToolStripSeparator();
        btnDemoJson = new ToolStripButton();
        btnDemoXml = new ToolStripButton();
        btnFolder = new ToolStripButton();
        sep2 = new ToolStripSeparator();
        btnLayout = new ToolStripDropDownButton();
        mnuTreeOnly = new ToolStripMenuItem();
        mnuFileOnly = new ToolStripMenuItem();
        mnuBoth = new ToolStripMenuItem();
        lblPath = new ToolStripLabel();
        splitMain = new SplitContainer();
        treeView = new TreeView();
        rawView = new RichTextBox();
        toolStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        SuspendLayout();
        // 
        // toolStrip
        // 
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Items.AddRange(new ToolStripItem[] {
            btnOpen,
            btnRefresh,
            sep1,
            btnDemoJson,
            btnDemoXml,
            btnFolder,
            sep2,
            btnLayout,
            lblPath});
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.RenderMode = ToolStripRenderMode.System;
        toolStrip.Size = new Size(960, 27);
        toolStrip.TabIndex = 0;
        toolStrip.Text = "toolStrip";
        // 
        // btnOpen
        // 
        btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new Size(48, 24);
        btnOpen.Text = "打开…";
        btnOpen.Click += btnOpen_Click;
        // 
        // btnRefresh
        // 
        btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(36, 24);
        btnRefresh.Text = "刷新";
        btnRefresh.Click += btnRefresh_Click;
        // 
        // sep1
        // 
        sep1.Name = "sep1";
        sep1.Size = new Size(6, 27);
        // 
        // btnDemoJson
        // 
        btnDemoJson.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDemoJson.Name = "btnDemoJson";
        btnDemoJson.Size = new Size(72, 24);
        btnDemoJson.Text = "JSON 演示";
        btnDemoJson.Click += btnDemoJson_Click;
        // 
        // btnDemoXml
        // 
        btnDemoXml.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnDemoXml.Name = "btnDemoXml";
        btnDemoXml.Size = new Size(68, 24);
        btnDemoXml.Text = "XML 演示";
        btnDemoXml.Click += btnDemoXml_Click;
        // 
        // btnFolder
        // 
        btnFolder.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnFolder.Name = "btnFolder";
        btnFolder.Size = new Size(60, 24);
        btnFolder.Text = "示例目录";
        btnFolder.Click += btnFolder_Click;
        // 
        // sep2
        // 
        sep2.Name = "sep2";
        sep2.Size = new Size(6, 27);
        // 
        // btnLayout
        // 
        btnLayout.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnLayout.DropDownItems.AddRange(new ToolStripItem[] {
            mnuTreeOnly,
            mnuFileOnly,
            mnuBoth});
        btnLayout.Name = "btnLayout";
        btnLayout.Size = new Size(100, 24);
        btnLayout.Text = "显示：树和文件";
        btnLayout.ToolTipText = "切换只看树、只看文件，或两边都显示";
        // 
        // mnuTreeOnly
        // 
        mnuTreeOnly.Name = "mnuTreeOnly";
        mnuTreeOnly.Size = new Size(160, 22);
        mnuTreeOnly.Text = "只显示树结构";
        mnuTreeOnly.Click += mnuTreeOnly_Click;
        // 
        // mnuFileOnly
        // 
        mnuFileOnly.Name = "mnuFileOnly";
        mnuFileOnly.Size = new Size(160, 22);
        mnuFileOnly.Text = "只显示文件";
        mnuFileOnly.Click += mnuFileOnly_Click;
        // 
        // mnuBoth
        // 
        mnuBoth.Checked = true;
        mnuBoth.CheckState = CheckState.Checked;
        mnuBoth.Name = "mnuBoth";
        mnuBoth.Size = new Size(160, 22);
        mnuBoth.Text = "都显示";
        mnuBoth.Click += mnuBoth_Click;
        // 
        // lblPath
        // 
        lblPath.AutoSize = false;
        lblPath.Name = "lblPath";
        lblPath.Size = new Size(420, 24);
        lblPath.Text = "未打开文件";
        lblPath.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 27);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(treeView);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(rawView);
        splitMain.Size = new Size(960, 613);
        splitMain.SplitterDistance = 320;
        splitMain.TabIndex = 1;
        // 
        // treeView
        // 
        treeView.Dock = DockStyle.Fill;
        treeView.Font = new Font("Microsoft YaHei UI", 9.5F);
        treeView.HideSelection = false;
        treeView.Location = new Point(0, 0);
        treeView.Name = "treeView";
        treeView.Size = new Size(320, 613);
        treeView.TabIndex = 0;
        // 
        // rawView
        // 
        rawView.BackColor = Color.White;
        rawView.BorderStyle = BorderStyle.None;
        rawView.DetectUrls = false;
        rawView.Dock = DockStyle.Fill;
        rawView.Font = new Font("Consolas", 10F);
        rawView.Location = new Point(0, 0);
        rawView.Name = "rawView";
        rawView.ReadOnly = true;
        rawView.Size = new Size(636, 613);
        rawView.TabIndex = 0;
        rawView.Text = "";
        rawView.WordWrap = false;
        // 
        // DocumentPreviewDemoForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 640);
        Controls.Add(splitMain);
        Controls.Add(toolStrip);
        Font = new Font("Microsoft YaHei UI", 9F);
        Name = "DocumentPreviewDemoForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "结构化预览演示（JSON / XML）";
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private ToolStrip toolStrip = null!;
    private ToolStripButton btnOpen = null!;
    private ToolStripButton btnRefresh = null!;
    private ToolStripSeparator sep1 = null!;
    private ToolStripButton btnDemoJson = null!;
    private ToolStripButton btnDemoXml = null!;
    private ToolStripButton btnFolder = null!;
    private ToolStripSeparator sep2 = null!;
    private ToolStripDropDownButton btnLayout = null!;
    private ToolStripMenuItem mnuTreeOnly = null!;
    private ToolStripMenuItem mnuFileOnly = null!;
    private ToolStripMenuItem mnuBoth = null!;
    private ToolStripLabel lblPath = null!;
    private SplitContainer splitMain = null!;
    private TreeView treeView = null!;
    private RichTextBox rawView = null!;
}
