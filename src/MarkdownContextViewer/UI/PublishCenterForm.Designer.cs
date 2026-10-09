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
        panelButtons = new Panel();
        btnPrimary = new Button();
        btnCancel = new Button();
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
        panelRoot.SuspendLayout();
        panelButtons.SuspendLayout();
        grpOptions.SuspendLayout();
        grpTarget.SuspendLayout();
        grpSource.SuspendLayout();
        panelDirRow.SuspendLayout();
        SuspendLayout();
        // 
        // panelRoot
        // 
        panelRoot.Controls.Add(panelButtons);
        panelRoot.Controls.Add(lblHint);
        panelRoot.Controls.Add(grpOptions);
        panelRoot.Controls.Add(grpTarget);
        panelRoot.Controls.Add(grpSource);
        panelRoot.Dock = DockStyle.Fill;
        panelRoot.Location = new Point(0, 0);
        panelRoot.Name = "panelRoot";
        panelRoot.Padding = new Padding(16, 12, 16, 12);
        panelRoot.Size = new Size(620, 600);
        panelRoot.TabIndex = 0;
        // 
        // panelButtons
        // 
        panelButtons.Controls.Add(btnPrimary);
        panelButtons.Controls.Add(btnCancel);
        panelButtons.Dock = DockStyle.Bottom;
        panelButtons.Location = new Point(16, 548);
        panelButtons.Name = "panelButtons";
        panelButtons.Size = new Size(588, 40);
        panelButtons.TabIndex = 4;
        // 
        // btnPrimary
        // 
        btnPrimary.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnPrimary.Location = new Point(488, 4);
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
        btnCancel.Location = new Point(388, 4);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(92, 32);
        btnCancel.TabIndex = 0;
        btnCancel.Text = "取消";
        // 
        // lblHint
        // 
        lblHint.Dock = DockStyle.Top;
        lblHint.ForeColor = SystemColors.GrayText;
        lblHint.Location = new Point(16, 450);
        lblHint.Name = "lblHint";
        lblHint.Padding = new Padding(4, 8, 4, 0);
        lblHint.Size = new Size(588, 40);
        lblHint.TabIndex = 3;
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
        grpOptions.Size = new Size(588, 140);
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
        btnOpenWiki.Size = new Size(564, 32);
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
        chkChmCompile.Size = new Size(564, 28);
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
        chkOpenAfter.Size = new Size(564, 28);
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
        grpTarget.Size = new Size(588, 150);
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
        radioWiki.Size = new Size(564, 28);
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
        radioChm.Size = new Size(564, 28);
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
        radioSingleHtml.Size = new Size(564, 28);
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
        radioSite.Size = new Size(564, 28);
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
        grpSource.Size = new Size(588, 148);
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
        panelDirRow.Size = new Size(564, 36);
        panelDirRow.TabIndex = 3;
        // 
        // txtDirectory
        // 
        txtDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDirectory.Location = new Point(24, 6);
        txtDirectory.Name = "txtDirectory";
        txtDirectory.PlaceholderText = "包含 Markdown 的文档根目录";
        txtDirectory.Size = new Size(428, 27);
        txtDirectory.TabIndex = 0;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(460, 4);
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
        radioFolder.Size = new Size(564, 28);
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
        lblCurrentDoc.Size = new Size(564, 24);
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
        radioCurrent.Size = new Size(564, 28);
        radioCurrent.TabIndex = 0;
        radioCurrent.TabStop = true;
        radioCurrent.Text = "当前文档";
        radioCurrent.CheckedChanged += SourceOrTargetChanged;
        // 
        // PublishCenterForm
        // 
        AcceptButton = btnPrimary;
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(620, 600);
        Controls.Add(panelRoot);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "PublishCenterForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "导出";
        panelRoot.ResumeLayout(false);
        panelButtons.ResumeLayout(false);
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        grpTarget.ResumeLayout(false);
        grpTarget.PerformLayout();
        grpSource.ResumeLayout(false);
        grpSource.PerformLayout();
        panelDirRow.ResumeLayout(false);
        panelDirRow.PerformLayout();
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
    private Label lblHint = null!;
    private Panel panelButtons = null!;
    private Button btnPrimary = null!;
    private Button btnCancel = null!;
}
