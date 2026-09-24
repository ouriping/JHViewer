namespace MarkdownContextViewer;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        btnOpen = new Button();
        btnSaveCurrent = new Button();
        btnExportWeb = new Button();
        btnRegisterContext = new Button();
        btnUnregisterContext = new Button();
        chkExplorerPreview = new CheckBox();
        btnMarkdownPreview = new Button();
        btnKillPrevhost = new Button();
        btnRestartExplorer = new Button();
        btnDiagnosePreview = new Button();
        mainSplitContainer = new SplitContainer();
        lstHistory = new ListBox();
        lblHistory = new Label();
        tabsMarkdown = new TabControl();
        topPanel = new Panel();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        topPanel.SuspendLayout();
        SuspendLayout();
        // 
        // btnOpen
        // 
        btnOpen.Location = new Point(13, 12);
        btnOpen.Margin = new Padding(4);
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new Size(129, 35);
        btnOpen.TabIndex = 0;
        btnOpen.Text = "打开 .md";
        btnOpen.UseVisualStyleBackColor = true;
        btnOpen.Click += btnOpen_Click;
        // 
        // btnSaveCurrent
        // 
        btnSaveCurrent.Location = new Point(154, 12);
        btnSaveCurrent.Margin = new Padding(4);
        btnSaveCurrent.Name = "btnSaveCurrent";
        btnSaveCurrent.Size = new Size(129, 35);
        btnSaveCurrent.TabIndex = 1;
        btnSaveCurrent.Text = "保存当前";
        btnSaveCurrent.UseVisualStyleBackColor = true;
        btnSaveCurrent.Click += btnSaveCurrent_Click;
        // 
        // btnExportWeb
        // 
        btnExportWeb.Location = new Point(296, 12);
        btnExportWeb.Margin = new Padding(4);
        btnExportWeb.Name = "btnExportWeb";
        btnExportWeb.Size = new Size(190, 35);
        btnExportWeb.TabIndex = 2;
        btnExportWeb.Text = "发布中心";
        btnExportWeb.UseVisualStyleBackColor = true;
        btnExportWeb.Click += btnExportWeb_Click;
        // 
        // btnRegisterContext
        // 
        btnRegisterContext.Location = new Point(496, 12);
        btnRegisterContext.Margin = new Padding(4);
        btnRegisterContext.Name = "btnRegisterContext";
        btnRegisterContext.Size = new Size(167, 35);
        btnRegisterContext.TabIndex = 2;
        btnRegisterContext.Text = "注册右键菜单";
        btnRegisterContext.UseVisualStyleBackColor = true;
        btnRegisterContext.Click += btnRegisterContext_Click;
        // 
        // btnUnregisterContext
        // 
        btnUnregisterContext.Location = new Point(676, 12);
        btnUnregisterContext.Margin = new Padding(4);
        btnUnregisterContext.Name = "btnUnregisterContext";
        btnUnregisterContext.Size = new Size(167, 35);
        btnUnregisterContext.TabIndex = 3;
        btnUnregisterContext.Text = "卸载右键菜单";
        btnUnregisterContext.UseVisualStyleBackColor = true;
        btnUnregisterContext.Click += btnUnregisterContext_Click_1;
        // 
        // chkExplorerPreview
        // 
        chkExplorerPreview.AutoSize = true;
        chkExplorerPreview.Location = new Point(860, 18);
        chkExplorerPreview.Margin = new Padding(4);
        chkExplorerPreview.Name = "chkExplorerPreview";
        chkExplorerPreview.Size = new Size(252, 24);
        chkExplorerPreview.TabIndex = 4;
        chkExplorerPreview.Text = "资源管理器预览窗格渲染 .md";
        chkExplorerPreview.UseVisualStyleBackColor = true;
        chkExplorerPreview.CheckedChanged += ChkExplorerPreview_CheckedChanged;
        // 
        // btnMarkdownPreview
        // 
        btnMarkdownPreview.Location = new Point(1120, 12);
        btnMarkdownPreview.Margin = new Padding(4);
        btnMarkdownPreview.Name = "btnMarkdownPreview";
        btnMarkdownPreview.Size = new Size(150, 35);
        btnMarkdownPreview.TabIndex = 5;
        btnMarkdownPreview.Text = "预览窗体";
        btnMarkdownPreview.UseVisualStyleBackColor = true;
        btnMarkdownPreview.Click += btnMarkdownPreview_Click;
        // 
        // btnKillPrevhost
        // 
        btnKillPrevhost.Location = new Point(13, 52);
        btnKillPrevhost.Margin = new Padding(4);
        btnKillPrevhost.Name = "btnKillPrevhost";
        btnKillPrevhost.Size = new Size(170, 32);
        btnKillPrevhost.TabIndex = 6;
        btnKillPrevhost.Text = "结束预览宿主";
        btnKillPrevhost.UseVisualStyleBackColor = true;
        btnKillPrevhost.Click += btnKillPrevhost_Click;
        // 
        // btnRestartExplorer
        // 
        btnRestartExplorer.Location = new Point(195, 52);
        btnRestartExplorer.Margin = new Padding(4);
        btnRestartExplorer.Name = "btnRestartExplorer";
        btnRestartExplorer.Size = new Size(170, 32);
        btnRestartExplorer.TabIndex = 7;
        btnRestartExplorer.Text = "重启资源管理器";
        btnRestartExplorer.UseVisualStyleBackColor = true;
        btnRestartExplorer.Click += btnRestartExplorer_Click;
        // 
        // btnDiagnosePreview
        // 
        btnDiagnosePreview.Location = new Point(377, 52);
        btnDiagnosePreview.Margin = new Padding(4);
        btnDiagnosePreview.Name = "btnDiagnosePreview";
        btnDiagnosePreview.Size = new Size(170, 32);
        btnDiagnosePreview.TabIndex = 8;
        btnDiagnosePreview.Text = "预览诊断";
        btnDiagnosePreview.UseVisualStyleBackColor = true;
        btnDiagnosePreview.Click += btnDiagnosePreview_Click;
        // 
        // mainSplitContainer
        // 
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Location = new Point(0, 96);
        mainSplitContainer.Margin = new Padding(4);
        mainSplitContainer.Name = "mainSplitContainer";
        // 
        // mainSplitContainer.Panel1
        // 
        mainSplitContainer.Panel1.Controls.Add(lstHistory);
        mainSplitContainer.Panel1.Controls.Add(lblHistory);
        // 
        // mainSplitContainer.Panel2
        // 
        mainSplitContainer.Panel2.Controls.Add(tabsMarkdown);
        mainSplitContainer.Size = new Size(1290, 635);
        mainSplitContainer.SplitterDistance = 247;
        mainSplitContainer.SplitterWidth = 5;
        mainSplitContainer.TabIndex = 1;
        // 
        // lstHistory
        // 
        lstHistory.Dock = DockStyle.Fill;
        lstHistory.FormattingEnabled = true;
        lstHistory.HorizontalScrollbar = true;
        lstHistory.Location = new Point(0, 20);
        lstHistory.Margin = new Padding(4);
        lstHistory.Name = "lstHistory";
        lstHistory.Size = new Size(247, 615);
        lstHistory.TabIndex = 1;
        // 
        // lblHistory
        // 
        lblHistory.Dock = DockStyle.Top;
        lblHistory.Location = new Point(0, 0);
        lblHistory.Margin = new Padding(4, 0, 4, 0);
        lblHistory.Name = "lblHistory";
        lblHistory.Size = new Size(247, 20);
        lblHistory.TabIndex = 0;
        lblHistory.Text = "历史打开文件（双击可打开）";
        // 
        // tabsMarkdown
        // 
        tabsMarkdown.Dock = DockStyle.Fill;
        tabsMarkdown.Location = new Point(0, 0);
        tabsMarkdown.Margin = new Padding(4);
        tabsMarkdown.Name = "tabsMarkdown";
        tabsMarkdown.SelectedIndex = 0;
        tabsMarkdown.Size = new Size(894, 635);
        tabsMarkdown.TabIndex = 0;
        // 
        // topPanel
        // 
        topPanel.Controls.Add(btnOpen);
        topPanel.Controls.Add(btnSaveCurrent);
        topPanel.Controls.Add(btnExportWeb);
        topPanel.Controls.Add(btnRegisterContext);
        topPanel.Controls.Add(btnUnregisterContext);
        topPanel.Controls.Add(chkExplorerPreview);
        topPanel.Controls.Add(btnMarkdownPreview);
        topPanel.Controls.Add(btnKillPrevhost);
        topPanel.Controls.Add(btnRestartExplorer);
        topPanel.Controls.Add(btnDiagnosePreview);
        topPanel.Dock = DockStyle.Top;
        topPanel.Location = new Point(0, 0);
        topPanel.Margin = new Padding(4);
        topPanel.Name = "topPanel";
        topPanel.Padding = new Padding(13, 12, 13, 0);
        topPanel.Size = new Size(1290, 96);
        topPanel.TabIndex = 2;
        // 
        // FrmMain
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1290, 694);
        Controls.Add(mainSplitContainer);
        Controls.Add(topPanel);
        Margin = new Padding(4);
        Name = "FrmMain";
        Text = "Markdown 浏览器与编辑器";
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        topPanel.ResumeLayout(false);
        topPanel.PerformLayout();
        ResumeLayout(false);
    }

    private Button btnOpen = null!;
    private Button btnSaveCurrent = null!;
    private Button btnExportWeb = null!;
    private Button btnRegisterContext = null!;
    private Button btnUnregisterContext = null!;
    private CheckBox chkExplorerPreview = null!;
    private Button btnMarkdownPreview = null!;
    private Button btnKillPrevhost = null!;
    private Button btnRestartExplorer = null!;
    private Button btnDiagnosePreview = null!;
    private SplitContainer mainSplitContainer = null!;
    private Label lblHistory = null!;
    private ListBox lstHistory = null!;
    private TabControl tabsMarkdown = null!;
    private Panel topPanel = null!;
}
