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
        tabs.SuspendLayout();
        tabSingle.SuspendLayout();
        panelSingle.SuspendLayout();
        tabDirectory.SuspendLayout();
        panelDirectory.SuspendLayout();
        panelDirectoryRow.SuspendLayout();
        SuspendLayout();
        // 
        // tabs
        // 
        tabs.Controls.Add(tabSingle);
        tabs.Controls.Add(tabDirectory);
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
}
