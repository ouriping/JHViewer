namespace MarkdownContextViewer;

partial class PreviewRegistrationForm
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
        rootLayout = new TableLayoutPanel();
        lblIntro = new Label();
        pnlFormatsHost = new FlowLayoutPanel();
        panelQuick = new FlowLayoutPanel();
        btnSelectAll = new Button();
        btnSelectNone = new Button();
        btnSelectDefault = new Button();
        lblEngineHint = new Label();
        panelLog = new Panel();
        logLayout = new TableLayoutPanel();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        txtLog = new TextBox();
        panelButtons = new FlowLayoutPanel();
        btnClose = new Button();
        btnDisable = new Button();
        btnEnable = new Button();
        rootLayout.SuspendLayout();
        panelQuick.SuspendLayout();
        panelLog.SuspendLayout();
        logLayout.SuspendLayout();
        panelButtons.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(lblIntro, 0, 0);
        rootLayout.Controls.Add(pnlFormatsHost, 0, 1);
        rootLayout.Controls.Add(panelQuick, 0, 2);
        rootLayout.Controls.Add(lblEngineHint, 0, 3);
        rootLayout.Controls.Add(panelLog, 0, 4);
        rootLayout.Controls.Add(panelButtons, 0, 5);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Margin = new Padding(4);
        rootLayout.Name = "rootLayout";
        rootLayout.Padding = new Padding(15, 14, 15, 14);
        rootLayout.RowCount = 6;
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.Size = new Size(926, 753);
        rootLayout.TabIndex = 0;
        // 
        // lblIntro
        // 
        lblIntro.AutoSize = true;
        lblIntro.Dock = DockStyle.Fill;
        lblIntro.Location = new Point(19, 14);
        lblIntro.Margin = new Padding(4, 0, 4, 9);
        lblIntro.MaximumSize = new Size(874, 0);
        lblIntro.Name = "lblIntro";
        lblIntro.Size = new Size(874, 20);
        lblIntro.TabIndex = 0;
        lblIntro.Text = "选择要在资源管理器“预览窗格”中启用的文件类型。注册会部署预览组件并写入扩展名关联；下方显示脚本操作与进度。";
        // 
        // pnlFormatsHost
        // 
        pnlFormatsHost.AutoSize = true;
        pnlFormatsHost.Dock = DockStyle.Fill;
        pnlFormatsHost.FlowDirection = FlowDirection.TopDown;
        pnlFormatsHost.Location = new Point(19, 47);
        pnlFormatsHost.Margin = new Padding(4);
        pnlFormatsHost.Name = "pnlFormatsHost";
        pnlFormatsHost.Size = new Size(888, 1);
        pnlFormatsHost.TabIndex = 1;
        pnlFormatsHost.WrapContents = false;
        // 
        // panelQuick
        // 
        panelQuick.AutoSize = true;
        panelQuick.Controls.Add(btnSelectAll);
        panelQuick.Controls.Add(btnSelectNone);
        panelQuick.Controls.Add(btnSelectDefault);
        panelQuick.Dock = DockStyle.Fill;
        panelQuick.Location = new Point(19, 51);
        panelQuick.Margin = new Padding(4, 0, 4, 5);
        panelQuick.Name = "panelQuick";
        panelQuick.Size = new Size(888, 43);
        panelQuick.TabIndex = 2;
        // 
        // btnSelectAll
        // 
        btnSelectAll.AutoSize = true;
        btnSelectAll.Location = new Point(4, 4);
        btnSelectAll.Margin = new Padding(4);
        btnSelectAll.Name = "btnSelectAll";
        btnSelectAll.Size = new Size(63, 35);
        btnSelectAll.TabIndex = 0;
        btnSelectAll.Text = "全选";
        btnSelectAll.UseVisualStyleBackColor = true;
        btnSelectAll.Click += btnSelectAll_Click;
        // 
        // btnSelectNone
        // 
        btnSelectNone.AutoSize = true;
        btnSelectNone.Location = new Point(75, 4);
        btnSelectNone.Margin = new Padding(4);
        btnSelectNone.Name = "btnSelectNone";
        btnSelectNone.Size = new Size(82, 35);
        btnSelectNone.TabIndex = 1;
        btnSelectNone.Text = "全不选";
        btnSelectNone.UseVisualStyleBackColor = true;
        btnSelectNone.Click += btnSelectNone_Click;
        // 
        // btnSelectDefault
        // 
        btnSelectDefault.AutoSize = true;
        btnSelectDefault.Location = new Point(165, 4);
        btnSelectDefault.Margin = new Padding(4);
        btnSelectDefault.Name = "btnSelectDefault";
        btnSelectDefault.Size = new Size(102, 35);
        btnSelectDefault.TabIndex = 2;
        btnSelectDefault.Text = "恢复默认";
        btnSelectDefault.UseVisualStyleBackColor = true;
        btnSelectDefault.Click += btnSelectDefault_Click;
        // 
        // lblEngineHint
        // 
        lblEngineHint.AutoSize = true;
        lblEngineHint.Dock = DockStyle.Fill;
        lblEngineHint.ForeColor = SystemColors.GrayText;
        lblEngineHint.Location = new Point(19, 99);
        lblEngineHint.Margin = new Padding(4, 0, 4, 9);
        lblEngineHint.Name = "lblEngineHint";
        lblEngineHint.Size = new Size(888, 20);
        lblEngineHint.TabIndex = 3;
        lblEngineHint.Text = "当前预览引擎：…";
        // 
        // panelLog
        // 
        panelLog.Controls.Add(logLayout);
        panelLog.Dock = DockStyle.Fill;
        panelLog.Location = new Point(19, 132);
        panelLog.Margin = new Padding(4);
        panelLog.Name = "panelLog";
        panelLog.Padding = new Padding(0, 5, 0, 5);
        panelLog.Size = new Size(888, 543);
        panelLog.TabIndex = 4;
        // 
        // logLayout
        // 
        logLayout.ColumnCount = 1;
        logLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logLayout.Controls.Add(progressBar, 0, 0);
        logLayout.Controls.Add(lblStatus, 0, 1);
        logLayout.Controls.Add(txtLog, 0, 2);
        logLayout.Dock = DockStyle.Fill;
        logLayout.Location = new Point(0, 5);
        logLayout.Margin = new Padding(4);
        logLayout.Name = "logLayout";
        logLayout.RowCount = 3;
        logLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
        logLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        logLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        logLayout.Size = new Size(888, 533);
        logLayout.TabIndex = 0;
        // 
        // progressBar
        // 
        progressBar.Dock = DockStyle.Fill;
        progressBar.Location = new Point(4, 4);
        progressBar.Margin = new Padding(4);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(880, 25);
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.TabIndex = 0;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.Location = new Point(4, 33);
        lblStatus.Margin = new Padding(4, 0, 4, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(880, 26);
        lblStatus.TabIndex = 1;
        lblStatus.Text = "就绪";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtLog
        // 
        txtLog.BackColor = Color.FromArgb(250, 250, 250);
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.Location = new Point(4, 63);
        txtLog.Margin = new Padding(4);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(880, 466);
        txtLog.TabIndex = 2;
        txtLog.WordWrap = false;
        // 
        // panelButtons
        // 
        panelButtons.AutoSize = true;
        panelButtons.Controls.Add(btnClose);
        panelButtons.Controls.Add(btnDisable);
        panelButtons.Controls.Add(btnEnable);
        panelButtons.Dock = DockStyle.Fill;
        panelButtons.FlowDirection = FlowDirection.RightToLeft;
        panelButtons.Location = new Point(19, 683);
        panelButtons.Margin = new Padding(4);
        panelButtons.Name = "panelButtons";
        panelButtons.Padding = new Padding(0, 9, 0, 0);
        panelButtons.Size = new Size(888, 52);
        panelButtons.TabIndex = 5;
        // 
        // btnClose
        // 
        btnClose.AutoSize = true;
        btnClose.DialogResult = DialogResult.Cancel;
        btnClose.Location = new Point(806, 13);
        btnClose.Margin = new Padding(4);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(78, 35);
        btnClose.TabIndex = 0;
        btnClose.Text = "关闭";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;
        // 
        // btnDisable
        // 
        btnDisable.AutoSize = true;
        btnDisable.Location = new Point(642, 13);
        btnDisable.Margin = new Padding(4);
        btnDisable.Name = "btnDisable";
        btnDisable.Size = new Size(156, 35);
        btnDisable.TabIndex = 1;
        btnDisable.Text = "取消预览窗格";
        btnDisable.UseVisualStyleBackColor = true;
        btnDisable.Click += btnDisable_Click;
        // 
        // btnEnable
        // 
        btnEnable.AutoSize = true;
        btnEnable.Location = new Point(496, 13);
        btnEnable.Margin = new Padding(4);
        btnEnable.Name = "btnEnable";
        btnEnable.Size = new Size(138, 35);
        btnEnable.TabIndex = 2;
        btnEnable.Text = "注册并启用";
        btnEnable.UseVisualStyleBackColor = true;
        btnEnable.Click += btnEnable_Click;
        // 
        // PreviewRegistrationForm
        // 
        AcceptButton = btnEnable;
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnClose;
        ClientSize = new Size(926, 753);
        Controls.Add(rootLayout);
        Font = new Font("Microsoft YaHei UI", 9F);
        Margin = new Padding(4);
        MaximizeBox = false;
        MinimumSize = new Size(818, 651);
        Name = "PreviewRegistrationForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "启用资源管理器预览窗格";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        panelQuick.ResumeLayout(false);
        panelQuick.PerformLayout();
        panelLog.ResumeLayout(false);
        logLayout.ResumeLayout(false);
        logLayout.PerformLayout();
        panelButtons.ResumeLayout(false);
        panelButtons.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel rootLayout = null!;
    private Label lblIntro = null!;
    private FlowLayoutPanel pnlFormatsHost = null!;
    private FlowLayoutPanel panelQuick = null!;
    private Button btnSelectAll = null!;
    private Button btnSelectNone = null!;
    private Button btnSelectDefault = null!;
    private Label lblEngineHint = null!;
    private Panel panelLog = null!;
    private TableLayoutPanel logLayout = null!;
    private ProgressBar progressBar = null!;
    private Label lblStatus = null!;
    private TextBox txtLog = null!;
    private FlowLayoutPanel panelButtons = null!;
    private Button btnClose = null!;
    private Button btnDisable = null!;
    private Button btnEnable = null!;
}
