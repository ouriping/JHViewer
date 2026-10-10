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
        panelRoot = new Panel();
        grpFiles = new GroupBox();
        treeMarkdown = new TreeView();
        lblFileSummary = new Label();
        lblHint = new Label();
        grpOptions = new GroupBox();
        btnOpenWiki = new Button();
        chkChmCompile = new CheckBox();
        chkOpenAfter = new CheckBox();
        grpTarget = new GroupBox();
        radioWiki = new RadioButton();
        radioChm = new RadioButton();
        radioSingleHtml = new RadioButton();
        radioSite = new RadioButton();
        grpSource = new GroupBox();
        panelDirRow = new Panel();
        txtDirectory = new TextBox();
        btnBrowse = new Button();
        radioFolder = new RadioButton();
        lblCurrentDoc = new Label();
        radioCurrent = new RadioButton();
        panelButtons = new Panel();
        btnPrimary = new Button();
        btnCancel = new Button();
        panelRoot.SuspendLayout();
        grpFiles.SuspendLayout();
        grpOptions.SuspendLayout();
        grpTarget.SuspendLayout();
        grpSource.SuspendLayout();
        panelDirRow.SuspendLayout();
        panelButtons.SuspendLayout();
        SuspendLayout();
        // 
        // panelRoot
        // 
        panelRoot.Controls.Add(grpFiles);
        panelRoot.Controls.Add(lblHint);
        panelRoot.Controls.Add(grpOptions);
        panelRoot.Controls.Add(grpTarget);
        panelRoot.Controls.Add(grpSource);
        panelRoot.Controls.Add(panelButtons);
        panelRoot.Dock = DockStyle.Fill;
        panelRoot.Location = new Point(0, 0);
        panelRoot.Name = "panelRoot";
        panelRoot.Padding = new Padding(16, 12, 16, 12);
        panelRoot.Size = new Size(740, 830);
        panelRoot.TabIndex = 0;
        // 
        // grpFiles
        // 
        grpFiles.Controls.Add(treeMarkdown);
        grpFiles.Controls.Add(lblFileSummary);
        grpFiles.Dock = DockStyle.Fill;
        grpFiles.Location = new Point(16, 441);
        grpFiles.Name = "grpFiles";
        grpFiles.Padding = new Padding(12, 8, 12, 8);
        grpFiles.Size = new Size(708, 289);
        grpFiles.TabIndex = 3;
        grpFiles.TabStop = false;
        grpFiles.Text = "文档列表";
        // 
        // treeMarkdown
        // 
        treeMarkdown.Dock = DockStyle.Fill;
        treeMarkdown.HideSelection = false;
        treeMarkdown.Location = new Point(12, 52);
        treeMarkdown.Name = "treeMarkdown";
        treeMarkdown.ShowNodeToolTips = true;
        treeMarkdown.Size = new Size(684, 229);
        treeMarkdown.TabIndex = 1;
        // 
        // lblFileSummary
        // 
        lblFileSummary.Dock = DockStyle.Top;
        lblFileSummary.ForeColor = SystemColors.GrayText;
        lblFileSummary.Location = new Point(12, 28);
        lblFileSummary.Name = "lblFileSummary";
        lblFileSummary.Padding = new Padding(2, 0, 2, 4);
        lblFileSummary.Size = new Size(684, 24);
        lblFileSummary.TabIndex = 0;
        lblFileSummary.Text = "选择文档目录后，将在此列出全部 .md 文件。";
        // 
        // lblHint
        // 
        lblHint.Dock = DockStyle.Bottom;
        lblHint.ForeColor = SystemColors.GrayText;
        lblHint.Location = new Point(16, 730);
        lblHint.Name = "lblHint";
        lblHint.Padding = new Padding(4);
        lblHint.Size = new Size(708, 48);
        lblHint.TabIndex = 4;
        lblHint.Text = "将 Markdown 导出为可在浏览器中打开的本地站点。";
        // 
        // grpOptions
        // 
        grpOptions.Controls.Add(btnOpenWiki);
        grpOptions.Controls.Add(chkChmCompile);
        grpOptions.Controls.Add(chkOpenAfter);
        grpOptions.Dock = DockStyle.Top;
        grpOptions.Location = new Point(16, 310);
        grpOptions.Name = "grpOptions";
        grpOptions.Padding = new Padding(12, 8, 12, 8);
        grpOptions.Size = new Size(708, 131);
        grpOptions.TabIndex = 2;
        grpOptions.TabStop = false;
        grpOptions.Text = "选项";
        // 
        // btnOpenWiki
        // 
        btnOpenWiki.Dock = DockStyle.Top;
        btnOpenWiki.Location = new Point(12, 84);
        btnOpenWiki.Name = "btnOpenWiki";
        btnOpenWiki.Padding = new Padding(4, 0, 4, 0);
        btnOpenWiki.Size = new Size(684, 32);
        btnOpenWiki.TabIndex = 2;
        btnOpenWiki.Text = "打开已启动的 Wiki（127.0.0.1:5099）";
        btnOpenWiki.Visible = false;
        btnOpenWiki.Click += btnOpenWiki_Click;
        // 
        // chkChmCompile
        // 
        chkChmCompile.AutoSize = true;
        chkChmCompile.Checked = true;
        chkChmCompile.CheckState = CheckState.Checked;
        chkChmCompile.Dock = DockStyle.Top;
        chkChmCompile.Location = new Point(12, 56);
        chkChmCompile.Name = "chkChmCompile";
        chkChmCompile.Padding = new Padding(4, 2, 4, 2);
        chkChmCompile.Size = new Size(684, 28);
        chkChmCompile.TabIndex = 1;
        chkChmCompile.Text = "自动编译 CHM（使用 tools\\hhc\\hhc.exe）";
        // 
        // chkOpenAfter
        // 
        chkOpenAfter.AutoSize = true;
        chkOpenAfter.Checked = true;
        chkOpenAfter.CheckState = CheckState.Checked;
        chkOpenAfter.Dock = DockStyle.Top;
        chkOpenAfter.Location = new Point(12, 28);
        chkOpenAfter.Name = "chkOpenAfter";
        chkOpenAfter.Padding = new Padding(4, 2, 4, 2);
        chkOpenAfter.Size = new Size(684, 28);
        chkOpenAfter.TabIndex = 0;
        chkOpenAfter.Text = "完成后打开";
        // 
        // grpTarget
        // 
        grpTarget.Controls.Add(radioWiki);
        grpTarget.Controls.Add(radioChm);
        grpTarget.Controls.Add(radioSingleHtml);
        grpTarget.Controls.Add(radioSite);
        grpTarget.Dock = DockStyle.Top;
        grpTarget.Location = new Point(16, 160);
        grpTarget.Name = "grpTarget";
        grpTarget.Padding = new Padding(12, 8, 12, 8);
        grpTarget.Size = new Size(708, 150);
        grpTarget.TabIndex = 1;
        grpTarget.TabStop = false;
        grpTarget.Text = "目标";
        // 
        // radioWiki
        // 
        radioWiki.AutoSize = true;
        radioWiki.Dock = DockStyle.Top;
        radioWiki.Location = new Point(12, 112);
        radioWiki.Name = "radioWiki";
        radioWiki.Padding = new Padding(4, 2, 4, 2);
        radioWiki.Size = new Size(684, 28);
        radioWiki.TabIndex = 3;
        radioWiki.Text = "MarkdownWiki（可编辑帮助站）";
        radioWiki.CheckedChanged += SourceOrTargetChanged;
        // 
        // radioChm
        // 
        radioChm.AutoSize = true;
        radioChm.Dock = DockStyle.Top;
        radioChm.Location = new Point(12, 84);
        radioChm.Name = "radioChm";
        radioChm.Padding = new Padding(4, 2, 4, 2);
        radioChm.Size = new Size(684, 28);
        radioChm.TabIndex = 2;
        radioChm.Text = "帮助文件（CHM）";
        radioChm.CheckedChanged += SourceOrTargetChanged;
        // 
        // radioSingleHtml
        // 
        radioSingleHtml.AutoSize = true;
        radioSingleHtml.Dock = DockStyle.Top;
        radioSingleHtml.Location = new Point(12, 56);
        radioSingleHtml.Name = "radioSingleHtml";
        radioSingleHtml.Padding = new Padding(4, 2, 4, 2);
        radioSingleHtml.Size = new Size(684, 28);
        radioSingleHtml.TabIndex = 1;
        radioSingleHtml.Text = "单文件 docs.html（图片转 Base64）";
        radioSingleHtml.CheckedChanged += SourceOrTargetChanged;
        // 
        // radioSite
        // 
        radioSite.AutoSize = true;
        radioSite.Checked = true;
        radioSite.Dock = DockStyle.Top;
        radioSite.Location = new Point(12, 28);
        radioSite.Name = "radioSite";
        radioSite.Padding = new Padding(4, 2, 4, 2);
        radioSite.Size = new Size(684, 28);
        radioSite.TabIndex = 0;
        radioSite.TabStop = true;
        radioSite.Text = "本地预览站点（HTML）";
        radioSite.CheckedChanged += SourceOrTargetChanged;
        // 
        // grpSource
        // 
        grpSource.Controls.Add(panelDirRow);
        grpSource.Controls.Add(radioFolder);
        grpSource.Controls.Add(lblCurrentDoc);
        grpSource.Controls.Add(radioCurrent);
        grpSource.Dock = DockStyle.Top;
        grpSource.Location = new Point(16, 12);
        grpSource.Name = "grpSource";
        grpSource.Padding = new Padding(12, 8, 12, 8);
        grpSource.Size = new Size(708, 148);
        grpSource.TabIndex = 0;
        grpSource.TabStop = false;
        grpSource.Text = "来源";
        // 
        // panelDirRow
        // 
        panelDirRow.Controls.Add(txtDirectory);
        panelDirRow.Controls.Add(btnBrowse);
        panelDirRow.Dock = DockStyle.Top;
        panelDirRow.Location = new Point(12, 108);
        panelDirRow.Name = "panelDirRow";
        panelDirRow.Padding = new Padding(24, 4, 0, 0);
        panelDirRow.Size = new Size(684, 36);
        panelDirRow.TabIndex = 3;
        // 
        // txtDirectory
        // 
        txtDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDirectory.Location = new Point(24, 6);
        txtDirectory.Name = "txtDirectory";
        txtDirectory.PlaceholderText = "包含 Markdown 的文档根目录";
        txtDirectory.Size = new Size(548, 27);
        txtDirectory.TabIndex = 0;
        txtDirectory.Leave += txtDirectory_Leave;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(580, 4);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(100, 30);
        btnBrowse.TabIndex = 1;
        btnBrowse.Text = "浏览…";
        btnBrowse.Click += btnBrowse_Click;
        // 
        // radioFolder
        // 
        radioFolder.AutoSize = true;
        radioFolder.Dock = DockStyle.Top;
        radioFolder.Location = new Point(12, 80);
        radioFolder.Name = "radioFolder";
        radioFolder.Padding = new Padding(4, 4, 4, 0);
        radioFolder.Size = new Size(684, 28);
        radioFolder.TabIndex = 2;
        radioFolder.Text = "文档目录";
        radioFolder.CheckedChanged += SourceOrTargetChanged;
        // 
        // lblCurrentDoc
        // 
        lblCurrentDoc.AutoEllipsis = true;
        lblCurrentDoc.Dock = DockStyle.Top;
        lblCurrentDoc.ForeColor = SystemColors.GrayText;
        lblCurrentDoc.Location = new Point(12, 56);
        lblCurrentDoc.Name = "lblCurrentDoc";
        lblCurrentDoc.Padding = new Padding(24, 0, 4, 4);
        lblCurrentDoc.Size = new Size(684, 24);
        lblCurrentDoc.TabIndex = 1;
        lblCurrentDoc.Text = "未打开文档";
        // 
        // radioCurrent
        // 
        radioCurrent.AutoSize = true;
        radioCurrent.Checked = true;
        radioCurrent.Dock = DockStyle.Top;
        radioCurrent.Location = new Point(12, 28);
        radioCurrent.Name = "radioCurrent";
        radioCurrent.Padding = new Padding(4, 4, 4, 0);
        radioCurrent.Size = new Size(684, 28);
        radioCurrent.TabIndex = 0;
        radioCurrent.TabStop = true;
        radioCurrent.Text = "当前文档";
        radioCurrent.CheckedChanged += SourceOrTargetChanged;
        // 
        // panelButtons
        // 
        panelButtons.Controls.Add(btnPrimary);
        panelButtons.Controls.Add(btnCancel);
        panelButtons.Dock = DockStyle.Bottom;
        panelButtons.Location = new Point(16, 778);
        panelButtons.Name = "panelButtons";
        panelButtons.Size = new Size(708, 40);
        panelButtons.TabIndex = 5;
        // 
        // btnPrimary
        // 
        btnPrimary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnPrimary.Location = new Point(608, 4);
        btnPrimary.Name = "btnPrimary";
        btnPrimary.Size = new Size(100, 32);
        btnPrimary.TabIndex = 1;
        btnPrimary.Text = "导出";
        btnPrimary.Click += btnPrimary_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(508, 4);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(92, 32);
        btnCancel.TabIndex = 0;
        btnCancel.Text = "取消";
        // 
        // PublishCenterForm
        // 
        AcceptButton = btnPrimary;
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(740, 830);
        Controls.Add(panelRoot);
        MinimizeBox = false;
        MinimumSize = new Size(640, 640);
        Name = "PublishCenterForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "导出";
        panelRoot.ResumeLayout(false);
        grpFiles.ResumeLayout(false);
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        grpTarget.ResumeLayout(false);
        grpTarget.PerformLayout();
        grpSource.ResumeLayout(false);
        grpSource.PerformLayout();
        panelDirRow.ResumeLayout(false);
        panelDirRow.PerformLayout();
        panelButtons.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel panelRoot = null!;
    private GroupBox grpSource = null!;
    private RadioButton radioCurrent = null!;
    private Label lblCurrentDoc = null!;
    private RadioButton radioFolder = null!;
    private Panel panelDirRow = null!;
    private TextBox txtDirectory = null!;
    private Button btnBrowse = null!;
    private GroupBox grpTarget = null!;
    private RadioButton radioSite = null!;
    private RadioButton radioSingleHtml = null!;
    private RadioButton radioChm = null!;
    private RadioButton radioWiki = null!;
    private GroupBox grpOptions = null!;
    private CheckBox chkOpenAfter = null!;
    private CheckBox chkChmCompile = null!;
    private Button btnOpenWiki = null!;
    private GroupBox grpFiles = null!;
    private Label lblFileSummary = null!;
    private TreeView treeMarkdown = null!;
    private Label lblHint = null!;
    private Panel panelButtons = null!;
    private Button btnPrimary = null!;
    private Button btnCancel = null!;
}
