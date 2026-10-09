namespace MarkdownContextViewer;

partial class PublishCenterForm
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
        tabs = new TabControl();
        tabSingle = new TabPage();
        panelSingle = new Panel();
        btnExportCurrent = new Button();
        lblSingle = new Label();
        tabDirectory = new TabPage();
        panelDirectory = new Panel();
        btnExportDirectory = new Button();
        panelDirectoryRow = new Panel();
        txtDirectory = new TextBox();
        btnBrowse = new Button();
        lblDirectory = new Label();
        tabWiki = new TabPage();
        panelWiki = new Panel();
        btnOpenWiki = new Button();
        btnStartWiki = new Button();
        panelWikiRow = new Panel();
        txtWikiDirectory = new TextBox();
        btnWikiBrowse = new Button();
        lblWiki = new Label();
        tabChm = new TabPage();
        panelChm = new Panel();
        btnExportChm = new Button();
        chkChmCompile = new CheckBox();
        panelChmRow = new Panel();
        txtChmDirectory = new TextBox();
        btnChmBrowse = new Button();
        lblChm = new Label();
        tabs.SuspendLayout();
        tabSingle.SuspendLayout();
        panelSingle.SuspendLayout();
        tabDirectory.SuspendLayout();
        panelDirectory.SuspendLayout();
        panelDirectoryRow.SuspendLayout();
        tabWiki.SuspendLayout();
        panelWiki.SuspendLayout();
        panelWikiRow.SuspendLayout();
        tabChm.SuspendLayout();
        panelChm.SuspendLayout();
        panelChmRow.SuspendLayout();
        SuspendLayout();
        // 
        // tabs
        // 
        tabs.Controls.Add(tabSingle);
        tabs.Controls.Add(tabDirectory);
        tabs.Controls.Add(tabWiki);
        tabs.Controls.Add(tabChm);
        tabs.Dock = DockStyle.Fill;
        tabs.Location = new Point(0, 0);
        tabs.Margin = new Padding(4, 4, 4, 4);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(957, 448);
        tabs.TabIndex = 0;
        // 
        // tabSingle
        // 
        tabSingle.Controls.Add(panelSingle);
        tabSingle.Location = new Point(4, 29);
        tabSingle.Margin = new Padding(4, 4, 4, 4);
        tabSingle.Name = "tabSingle";
        tabSingle.Padding = new Padding(4, 4, 4, 4);
        tabSingle.Size = new Size(949, 415);
        tabSingle.TabIndex = 0;
        tabSingle.Text = "快速发布（当前文档）";
        tabSingle.UseVisualStyleBackColor = true;
        // 
        // panelSingle
        // 
        panelSingle.Controls.Add(btnExportCurrent);
        panelSingle.Controls.Add(lblSingle);
        panelSingle.Dock = DockStyle.Fill;
        panelSingle.Location = new Point(4, 4);
        panelSingle.Margin = new Padding(4, 4, 4, 4);
        panelSingle.Name = "panelSingle";
        panelSingle.Padding = new Padding(21, 19, 21, 19);
        panelSingle.Size = new Size(941, 407);
        panelSingle.TabIndex = 0;
        // 
        // btnExportCurrent
        // 
        btnExportCurrent.Dock = DockStyle.Top;
        btnExportCurrent.Location = new Point(21, 101);
        btnExportCurrent.Margin = new Padding(4, 4, 4, 4);
        btnExportCurrent.Name = "btnExportCurrent";
        btnExportCurrent.Size = new Size(899, 45);
        btnExportCurrent.TabIndex = 0;
        btnExportCurrent.Text = "导出当前文档并访问";
        btnExportCurrent.Click += btnExportCurrent_Click;
        // 
        // lblSingle
        // 
        lblSingle.Dock = DockStyle.Top;
        lblSingle.Location = new Point(21, 19);
        lblSingle.Margin = new Padding(4, 0, 4, 0);
        lblSingle.Name = "lblSingle";
        lblSingle.Size = new Size(899, 82);
        lblSingle.TabIndex = 1;
        lblSingle.Text = "将当前标签页的 Markdown 导出到程序目录 site-output，并启动本地网站访问。";
        // 
        // tabDirectory
        // 
        tabDirectory.Controls.Add(panelDirectory);
        tabDirectory.Location = new Point(4, 29);
        tabDirectory.Margin = new Padding(4, 4, 4, 4);
        tabDirectory.Name = "tabDirectory";
        tabDirectory.Padding = new Padding(4, 4, 4, 4);
        tabDirectory.Size = new Size(949, 415);
        tabDirectory.TabIndex = 1;
        tabDirectory.Text = "目录站点";
        tabDirectory.UseVisualStyleBackColor = true;
        // 
        // panelDirectory
        // 
        panelDirectory.Controls.Add(btnExportDirectory);
        panelDirectory.Controls.Add(panelDirectoryRow);
        panelDirectory.Controls.Add(lblDirectory);
        panelDirectory.Dock = DockStyle.Fill;
        panelDirectory.Location = new Point(4, 4);
        panelDirectory.Margin = new Padding(4, 4, 4, 4);
        panelDirectory.Name = "panelDirectory";
        panelDirectory.Padding = new Padding(21, 19, 21, 19);
        panelDirectory.Size = new Size(941, 407);
        panelDirectory.TabIndex = 0;
        // 
        // btnExportDirectory
        // 
        btnExportDirectory.Dock = DockStyle.Top;
        btnExportDirectory.Location = new Point(21, 125);
        btnExportDirectory.Margin = new Padding(4, 4, 4, 4);
        btnExportDirectory.Name = "btnExportDirectory";
        btnExportDirectory.Size = new Size(899, 45);
        btnExportDirectory.TabIndex = 0;
        btnExportDirectory.Text = "导出目录站点并访问";
        btnExportDirectory.Click += btnExportDirectory_Click;
        // 
        // panelDirectoryRow
        // 
        panelDirectoryRow.Controls.Add(txtDirectory);
        panelDirectoryRow.Controls.Add(btnBrowse);
        panelDirectoryRow.Dock = DockStyle.Top;
        panelDirectoryRow.Location = new Point(21, 78);
        panelDirectoryRow.Margin = new Padding(4, 4, 4, 4);
        panelDirectoryRow.Name = "panelDirectoryRow";
        panelDirectoryRow.Size = new Size(899, 47);
        panelDirectoryRow.TabIndex = 1;
        // 
        // txtDirectory
        // 
        txtDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDirectory.Location = new Point(4, 9);
        txtDirectory.Margin = new Padding(4, 4, 4, 4);
        txtDirectory.Name = "txtDirectory";
        txtDirectory.Size = new Size(717, 27);
        txtDirectory.TabIndex = 0;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(735, 7);
        btnBrowse.Margin = new Padding(4, 4, 4, 4);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(154, 33);
        btnBrowse.TabIndex = 1;
        btnBrowse.Text = "选择目录";
        btnBrowse.Click += btnBrowse_Click;
        // 
        // lblDirectory
        // 
        lblDirectory.Dock = DockStyle.Top;
        lblDirectory.Location = new Point(21, 19);
        lblDirectory.Margin = new Padding(4, 0, 4, 0);
        lblDirectory.Name = "lblDirectory";
        lblDirectory.Size = new Size(899, 59);
        lblDirectory.TabIndex = 2;
        lblDirectory.Text = "选择一个包含 .md 的目录，按目录结构导出站点并访问。";
        // 
        // tabWiki
        // 
        tabWiki.Controls.Add(panelWiki);
        tabWiki.Location = new Point(4, 29);
        tabWiki.Margin = new Padding(4, 4, 4, 4);
        tabWiki.Name = "tabWiki";
        tabWiki.Padding = new Padding(4, 4, 4, 4);
        tabWiki.Size = new Size(949, 415);
        tabWiki.TabIndex = 2;
        tabWiki.Text = "MarkdownWiki";
        tabWiki.UseVisualStyleBackColor = true;
        // 
        // panelWiki
        // 
        panelWiki.Controls.Add(btnOpenWiki);
        panelWiki.Controls.Add(btnStartWiki);
        panelWiki.Controls.Add(panelWikiRow);
        panelWiki.Controls.Add(lblWiki);
        panelWiki.Dock = DockStyle.Fill;
        panelWiki.Location = new Point(4, 4);
        panelWiki.Margin = new Padding(4, 4, 4, 4);
        panelWiki.Name = "panelWiki";
        panelWiki.Padding = new Padding(21, 19, 21, 19);
        panelWiki.Size = new Size(941, 407);
        panelWiki.TabIndex = 0;
        // 
        // btnOpenWiki
        // 
        btnOpenWiki.Dock = DockStyle.Top;
        btnOpenWiki.Location = new Point(21, 170);
        btnOpenWiki.Margin = new Padding(4, 4, 4, 4);
        btnOpenWiki.Name = "btnOpenWiki";
        btnOpenWiki.Size = new Size(899, 45);
        btnOpenWiki.TabIndex = 0;
        btnOpenWiki.Text = "打开 Wiki 浏览器（http://127.0.0.1:5099）";
        btnOpenWiki.Click += btnOpenWiki_Click;
        // 
        // btnStartWiki
        // 
        btnStartWiki.Dock = DockStyle.Top;
        btnStartWiki.Location = new Point(21, 125);
        btnStartWiki.Margin = new Padding(4, 4, 4, 4);
        btnStartWiki.Name = "btnStartWiki";
        btnStartWiki.Size = new Size(899, 45);
        btnStartWiki.TabIndex = 1;
        btnStartWiki.Text = "启动 MarkdownWiki（可编辑帮助站）";
        btnStartWiki.Click += btnStartWiki_Click;
        // 
        // panelWikiRow
        // 
        panelWikiRow.Controls.Add(txtWikiDirectory);
        panelWikiRow.Controls.Add(btnWikiBrowse);
        panelWikiRow.Dock = DockStyle.Top;
        panelWikiRow.Location = new Point(21, 78);
        panelWikiRow.Margin = new Padding(4, 4, 4, 4);
        panelWikiRow.Name = "panelWikiRow";
        panelWikiRow.Size = new Size(899, 47);
        panelWikiRow.TabIndex = 2;
        // 
        // txtWikiDirectory
        // 
        txtWikiDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtWikiDirectory.Location = new Point(4, 9);
        txtWikiDirectory.Margin = new Padding(4, 4, 4, 4);
        txtWikiDirectory.Name = "txtWikiDirectory";
        txtWikiDirectory.PlaceholderText = "可选：文档根目录（留空使用 Wiki 默认示例）";
        txtWikiDirectory.Size = new Size(717, 27);
        txtWikiDirectory.TabIndex = 0;
        // 
        // btnWikiBrowse
        // 
        btnWikiBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnWikiBrowse.Location = new Point(735, 7);
        btnWikiBrowse.Margin = new Padding(4, 4, 4, 4);
        btnWikiBrowse.Name = "btnWikiBrowse";
        btnWikiBrowse.Size = new Size(154, 33);
        btnWikiBrowse.TabIndex = 1;
        btnWikiBrowse.Text = "选择目录";
        btnWikiBrowse.Click += btnWikiBrowse_Click;
        // 
        // lblWiki
        // 
        lblWiki.Dock = DockStyle.Top;
        lblWiki.Location = new Point(21, 19);
        lblWiki.Margin = new Padding(4, 0, 4, 0);
        lblWiki.Name = "lblWiki";
        lblWiki.Size = new Size(899, 59);
        lblWiki.TabIndex = 3;
        lblWiki.Text = "启动独立 MarkdownWiki：多 md 树导航、Web 阅读/编辑，保存后立即更新网页。默认端口 5099。";
        // 
        // tabChm
        // 
        tabChm.Controls.Add(panelChm);
        tabChm.Location = new Point(4, 29);
        tabChm.Margin = new Padding(4, 4, 4, 4);
        tabChm.Name = "tabChm";
        tabChm.Padding = new Padding(4, 4, 4, 4);
        tabChm.Size = new Size(949, 415);
        tabChm.TabIndex = 3;
        tabChm.Text = "导出 CHM";
        tabChm.UseVisualStyleBackColor = true;
        // 
        // panelChm
        // 
        panelChm.Controls.Add(btnExportChm);
        panelChm.Controls.Add(chkChmCompile);
        panelChm.Controls.Add(panelChmRow);
        panelChm.Controls.Add(lblChm);
        panelChm.Dock = DockStyle.Fill;
        panelChm.Location = new Point(4, 4);
        panelChm.Margin = new Padding(4, 4, 4, 4);
        panelChm.Name = "panelChm";
        panelChm.Padding = new Padding(21, 19, 21, 19);
        panelChm.Size = new Size(941, 407);
        panelChm.TabIndex = 0;
        // 
        // btnExportChm
        // 
        btnExportChm.Dock = DockStyle.Top;
        btnExportChm.Location = new Point(21, 160);
        btnExportChm.Margin = new Padding(4, 4, 4, 4);
        btnExportChm.Name = "btnExportChm";
        btnExportChm.Size = new Size(899, 45);
        btnExportChm.TabIndex = 0;
        btnExportChm.Text = "导出 CHM（Markdown → HTML Help）";
        btnExportChm.Click += btnExportChm_Click;
        // 
        // chkChmCompile
        // 
        chkChmCompile.AutoSize = true;
        chkChmCompile.Checked = true;
        chkChmCompile.CheckState = CheckState.Checked;
        chkChmCompile.Dock = DockStyle.Top;
        chkChmCompile.Location = new Point(21, 125);
        chkChmCompile.Margin = new Padding(4, 4, 4, 4);
        chkChmCompile.Name = "chkChmCompile";
        chkChmCompile.Padding = new Padding(4, 8, 4, 8);
        chkChmCompile.Size = new Size(899, 40);
        chkChmCompile.TabIndex = 1;
        chkChmCompile.Text = "自动调用 hhc.exe（优先程序目录 tools\\hhc\\hhc.exe；找不到会提示）";
        // 
        // panelChmRow
        // 
        panelChmRow.Controls.Add(txtChmDirectory);
        panelChmRow.Controls.Add(btnChmBrowse);
        panelChmRow.Dock = DockStyle.Top;
        panelChmRow.Location = new Point(21, 78);
        panelChmRow.Margin = new Padding(4, 4, 4, 4);
        panelChmRow.Name = "panelChmRow";
        panelChmRow.Size = new Size(899, 47);
        panelChmRow.TabIndex = 2;
        // 
        // txtChmDirectory
        // 
        txtChmDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtChmDirectory.Location = new Point(4, 9);
        txtChmDirectory.Margin = new Padding(4, 4, 4, 4);
        txtChmDirectory.Name = "txtChmDirectory";
        txtChmDirectory.PlaceholderText = "选择包含 .md 的文档根目录";
        txtChmDirectory.Size = new Size(717, 27);
        txtChmDirectory.TabIndex = 0;
        // 
        // btnChmBrowse
        // 
        btnChmBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnChmBrowse.Location = new Point(735, 7);
        btnChmBrowse.Margin = new Padding(4, 4, 4, 4);
        btnChmBrowse.Name = "btnChmBrowse";
        btnChmBrowse.Size = new Size(154, 33);
        btnChmBrowse.TabIndex = 1;
        btnChmBrowse.Text = "选择目录";
        btnChmBrowse.Click += btnChmBrowse_Click;
        // 
        // lblChm
        // 
        lblChm.Dock = DockStyle.Top;
        lblChm.Location = new Point(21, 19);
        lblChm.Margin = new Padding(4, 0, 4, 0);
        lblChm.Name = "lblChm";
        lblChm.Size = new Size(899, 59);
        lblChm.TabIndex = 3;
        lblChm.Text = "将多份 Markdown 打包为 Windows CHM 帮助文件。输出在文档根旁的 chm-output 目录。";
        // 
        // PublishCenterForm
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(957, 448);
        Controls.Add(tabs);
        Margin = new Padding(4, 4, 4, 4);
        Name = "PublishCenterForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "发布中心";
        tabs.ResumeLayout(false);
        tabSingle.ResumeLayout(false);
        panelSingle.ResumeLayout(false);
        tabDirectory.ResumeLayout(false);
        panelDirectory.ResumeLayout(false);
        panelDirectoryRow.ResumeLayout(false);
        panelDirectoryRow.PerformLayout();
        tabWiki.ResumeLayout(false);
        panelWiki.ResumeLayout(false);
        panelWikiRow.ResumeLayout(false);
        panelWikiRow.PerformLayout();
        tabChm.ResumeLayout(false);
        panelChm.ResumeLayout(false);
        panelChm.PerformLayout();
        panelChmRow.ResumeLayout(false);
        panelChmRow.PerformLayout();
        ResumeLayout(false);
    }

    private TabControl tabs = null!;
    private TabPage tabSingle = null!;
    private Panel panelSingle = null!;
    private Label lblSingle = null!;
    private Button btnExportCurrent = null!;
    private TabPage tabDirectory = null!;
    private Panel panelDirectory = null!;
    private Label lblDirectory = null!;
    private Panel panelDirectoryRow = null!;
    private TextBox txtDirectory = null!;
    private Button btnBrowse = null!;
    private Button btnExportDirectory = null!;
    private TabPage tabWiki = null!;
    private Panel panelWiki = null!;
    private Label lblWiki = null!;
    private Panel panelWikiRow = null!;
    private TextBox txtWikiDirectory = null!;
    private Button btnWikiBrowse = null!;
    private Button btnStartWiki = null!;
    private Button btnOpenWiki = null!;
    private TabPage tabChm = null!;
    private Panel panelChm = null!;
    private Label lblChm = null!;
    private Panel panelChmRow = null!;
    private TextBox txtChmDirectory = null!;
    private Button btnChmBrowse = null!;
    private CheckBox chkChmCompile = null!;
    private Button btnExportChm = null!;
}
