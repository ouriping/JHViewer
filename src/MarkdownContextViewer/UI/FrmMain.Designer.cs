namespace MarkdownContextViewer;

partial class FrmMain
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
        menuStrip = new MenuStrip();
        mnuFile = new ToolStripMenuItem();
        mnuOpen = new ToolStripMenuItem();
        mnuSave = new ToolStripMenuItem();
        mnuExport = new ToolStripMenuItem();
        mnuExportCurrentSite = new ToolStripMenuItem();
        mnuExportFolderSite = new ToolStripMenuItem();
        mnuExportSingleHtml = new ToolStripMenuItem();
        mnuExportChm = new ToolStripMenuItem();
        mnuExportDialog = new ToolStripMenuItem();
        mnuExit = new ToolStripMenuItem();
        mnuTools = new ToolStripMenuItem();
        mnuMarkdownWiki = new ToolStripMenuItem();
        mnuExplorer = new ToolStripMenuItem();
        mnuRegisterContext = new ToolStripMenuItem();
        mnuUnregisterContext = new ToolStripMenuItem();
        mnuEnablePreview = new ToolStripMenuItem();
        mnuDisablePreview = new ToolStripMenuItem();
        mnuKillPrevhost = new ToolStripMenuItem();
        mnuRestartExplorer = new ToolStripMenuItem();
        mnuDiagnosePreview = new ToolStripMenuItem();
        mnuView = new ToolStripMenuItem();
        mnuEngineWebView = new ToolStripMenuItem();
        mnuEngineWinForms = new ToolStripMenuItem();
        mnuPreviewWindow = new ToolStripMenuItem();
        mnuPreviewDemo = new ToolStripMenuItem();
        mnuOpenDemoJson = new ToolStripMenuItem();
        mnuOpenDemoXml = new ToolStripMenuItem();
        mnuOpenDemoFolder = new ToolStripMenuItem();
        toolStrip = new ToolStrip();
        btnOpen = new ToolStripButton();
        btnSaveCurrent = new ToolStripButton();
        btnExportWeb = new ToolStripButton();
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        lblEngine = new ToolStripStatusLabel();
        splitMain = new SplitContainer();
        lblHistory = new Label();
        lstHistory = new ListBox();
        tabsMarkdown = new TabControl();
        menuStrip.SuspendLayout();
        toolStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuExplorer, mnuView, mnuTools });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1100, 25);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip";
        // 
        // mnuFile
        // 
        mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuOpen, mnuSave, mnuExport, new ToolStripSeparator(), mnuExit });
        mnuFile.Name = "mnuFile";
        mnuFile.Size = new Size(44, 21);
        mnuFile.Text = "文件";
        // 
        // mnuOpen
        // 
        mnuOpen.Name = "mnuOpen";
        mnuOpen.ShortcutKeys = Keys.Control | Keys.O;
        mnuOpen.Size = new Size(180, 22);
        mnuOpen.Text = "打开...";
        mnuOpen.Click += mnuOpen_Click;
        // 
        // mnuSave
        // 
        mnuSave.Name = "mnuSave";
        mnuSave.ShortcutKeys = Keys.Control | Keys.S;
        mnuSave.Size = new Size(180, 22);
        mnuSave.Text = "保存";
        mnuSave.Click += mnuSave_Click;
        // 
        // mnuExport
        // 
        mnuExport.DropDownItems.AddRange(new ToolStripItem[] { mnuExportCurrentSite, mnuExportFolderSite, mnuExportSingleHtml, mnuExportChm, new ToolStripSeparator(), mnuExportDialog });
        mnuExport.Name = "mnuExport";
        mnuExport.Size = new Size(180, 22);
        mnuExport.Text = "导出";
        // 
        // mnuExportCurrentSite
        // 
        mnuExportCurrentSite.Name = "mnuExportCurrentSite";
        mnuExportCurrentSite.Size = new Size(240, 22);
        mnuExportCurrentSite.Text = "导出当前文档为站点…";
        mnuExportCurrentSite.Click += mnuExportCurrentSite_Click;
        // 
        // mnuExportFolderSite
        // 
        mnuExportFolderSite.Name = "mnuExportFolderSite";
        mnuExportFolderSite.Size = new Size(240, 22);
        mnuExportFolderSite.Text = "导出文件夹为站点…";
        mnuExportFolderSite.Click += mnuExportFolderSite_Click;
        // 
        // mnuExportSingleHtml
        // 
        mnuExportSingleHtml.Name = "mnuExportSingleHtml";
        mnuExportSingleHtml.Size = new Size(280, 22);
        mnuExportSingleHtml.Text = "导出为单文件 docs.html…";
        mnuExportSingleHtml.Click += mnuExportSingleHtml_Click;
        // 
        // mnuExportChm
        // 
        mnuExportChm.Name = "mnuExportChm";
        mnuExportChm.Size = new Size(280, 22);
        mnuExportChm.Text = "导出为 CHM 帮助文件…";
        mnuExportChm.Click += mnuExportChm_Click;
        // 
        // mnuExportDialog
        // 
        mnuExportDialog.Name = "mnuExportDialog";
        mnuExportDialog.Size = new Size(240, 22);
        mnuExportDialog.Text = "导出…";
        mnuExportDialog.Click += mnuExportDialog_Click;
        // 
        // mnuExit
        // 
        mnuExit.Name = "mnuExit";
        mnuExit.Size = new Size(180, 22);
        mnuExit.Text = "退出";
        mnuExit.Click += mnuExit_Click;
        // 
        // mnuExplorer
        // 
        mnuExplorer.DropDownItems.AddRange(new ToolStripItem[] { mnuRegisterContext, mnuUnregisterContext, new ToolStripSeparator(), mnuEnablePreview, mnuDisablePreview, mnuKillPrevhost, mnuRestartExplorer, mnuDiagnosePreview });
        mnuExplorer.Name = "mnuExplorer";
        mnuExplorer.Size = new Size(80, 21);
        mnuExplorer.Text = "资源管理器";
        // 
        // mnuRegisterContext
        // 
        mnuRegisterContext.Name = "mnuRegisterContext";
        mnuRegisterContext.Size = new Size(280, 22);
        mnuRegisterContext.Text = "注册默认打开与右键菜单";
        mnuRegisterContext.Click += mnuRegisterContext_Click;
        // 
        // mnuUnregisterContext
        // 
        mnuUnregisterContext.Name = "mnuUnregisterContext";
        mnuUnregisterContext.Size = new Size(280, 22);
        mnuUnregisterContext.Text = "取消默认打开与右键菜单";
        mnuUnregisterContext.Click += mnuUnregisterContext_Click;
        // 
        // mnuEnablePreview
        // 
        mnuEnablePreview.Name = "mnuEnablePreview";
        mnuEnablePreview.Size = new Size(220, 22);
        mnuEnablePreview.Text = "启用预览窗格…";
        mnuEnablePreview.Click += mnuEnablePreview_Click;
        // 
        // mnuDisablePreview
        // 
        mnuDisablePreview.Name = "mnuDisablePreview";
        mnuDisablePreview.Size = new Size(280, 22);
        mnuDisablePreview.Text = "取消预览窗格…";
        mnuDisablePreview.Click += mnuDisablePreview_Click;
        // 
        // mnuKillPrevhost
        // 
        mnuKillPrevhost.Name = "mnuKillPrevhost";
        mnuKillPrevhost.Size = new Size(220, 22);
        mnuKillPrevhost.Text = "结束预览宿主";
        mnuKillPrevhost.Click += mnuKillPrevhost_Click;
        // 
        // mnuRestartExplorer
        // 
        mnuRestartExplorer.Name = "mnuRestartExplorer";
        mnuRestartExplorer.Size = new Size(220, 22);
        mnuRestartExplorer.Text = "重启资源管理器";
        mnuRestartExplorer.Click += mnuRestartExplorer_Click;
        // 
        // mnuDiagnosePreview
        // 
        mnuDiagnosePreview.Name = "mnuDiagnosePreview";
        mnuDiagnosePreview.Size = new Size(220, 22);
        mnuDiagnosePreview.Text = "预览诊断";
        mnuDiagnosePreview.Click += mnuDiagnosePreview_Click;
        // 
        // mnuView
        // 
        mnuView.DropDownItems.AddRange(new ToolStripItem[] { mnuEngineWebView, mnuEngineWinForms, new ToolStripSeparator(), mnuPreviewWindow, new ToolStripSeparator(), mnuPreviewDemo, mnuOpenDemoJson, mnuOpenDemoXml, mnuOpenDemoFolder });
        mnuView.Name = "mnuView";
        mnuView.Size = new Size(44, 21);
        mnuView.Text = "视图";
        // 
        // mnuEngineWebView
        // 
        mnuEngineWebView.Name = "mnuEngineWebView";
        mnuEngineWebView.Size = new Size(280, 22);
        mnuEngineWebView.Text = "预览引擎：Web（还原度高）★";
        mnuEngineWebView.Click += mnuEngineWebView_Click;
        // 
        // mnuEngineWinForms
        // 
        mnuEngineWinForms.Name = "mnuEngineWinForms";
        mnuEngineWinForms.Size = new Size(280, 22);
        mnuEngineWinForms.Text = "预览引擎：WinForms（更快更省）";
        mnuEngineWinForms.Click += mnuEngineWinForms_Click;
        // 
        // mnuPreviewWindow
        // 
        mnuPreviewWindow.Name = "mnuPreviewWindow";
        mnuPreviewWindow.Size = new Size(280, 22);
        mnuPreviewWindow.Text = "独立预览窗体";
        mnuPreviewWindow.Click += mnuPreviewWindow_Click;
        // 
        // mnuPreviewDemo
        // 
        mnuPreviewDemo.Name = "mnuPreviewDemo";
        mnuPreviewDemo.Size = new Size(280, 22);
        mnuPreviewDemo.Text = "预览演示…（JSON/XML）";
        mnuPreviewDemo.Click += mnuPreviewDemo_Click;
        // 
        // mnuOpenDemoJson
        // 
        mnuOpenDemoJson.Name = "mnuOpenDemoJson";
        mnuOpenDemoJson.Size = new Size(280, 22);
        mnuOpenDemoJson.Text = "打开 JSON 演示";
        mnuOpenDemoJson.Click += mnuOpenDemoJson_Click;
        // 
        // mnuOpenDemoXml
        // 
        mnuOpenDemoXml.Name = "mnuOpenDemoXml";
        mnuOpenDemoXml.Size = new Size(280, 22);
        mnuOpenDemoXml.Text = "打开 XML 演示";
        mnuOpenDemoXml.Click += mnuOpenDemoXml_Click;
        // 
        // mnuOpenDemoFolder
        // 
        mnuOpenDemoFolder.Name = "mnuOpenDemoFolder";
        mnuOpenDemoFolder.Size = new Size(280, 22);
        mnuOpenDemoFolder.Text = "打开演示示例目录";
        mnuOpenDemoFolder.Click += mnuOpenDemoFolder_Click;
        // 
        // mnuTools
        // 
        mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuMarkdownWiki });
        mnuTools.Name = "mnuTools";
        mnuTools.Size = new Size(44, 21);
        mnuTools.Text = "工具";
        // 
        // mnuMarkdownWiki
        // 
        mnuMarkdownWiki.Name = "mnuMarkdownWiki";
        mnuMarkdownWiki.Size = new Size(200, 22);
        mnuMarkdownWiki.Text = "MarkdownWiki…";
        mnuMarkdownWiki.Click += mnuMarkdownWiki_Click;
        // 
        // toolStrip
        // 
        toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, btnSaveCurrent, btnExportWeb });
        toolStrip.Location = new Point(0, 25);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1100, 25);
        toolStrip.TabIndex = 1;
        toolStrip.Text = "toolStrip";
        // 
        // btnOpen
        // 
        btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new Size(36, 22);
        btnOpen.Text = "打开";
        btnOpen.Click += mnuOpen_Click;
        // 
        // btnSaveCurrent
        // 
        btnSaveCurrent.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnSaveCurrent.Name = "btnSaveCurrent";
        btnSaveCurrent.Size = new Size(36, 22);
        btnSaveCurrent.Text = "保存";
        btnSaveCurrent.Click += mnuSave_Click;
        // 
        // btnExportWeb
        // 
        btnExportWeb.DisplayStyle = ToolStripItemDisplayStyle.Text;
        btnExportWeb.Name = "btnExportWeb";
        btnExportWeb.Size = new Size(72, 22);
        btnExportWeb.Text = "导出站点";
        btnExportWeb.ToolTipText = "将当前文档导出为本地预览站点";
        btnExportWeb.Click += mnuExportCurrentSite_Click;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblEngine });
        statusStrip.Location = new Point(0, 628);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
        statusStrip.TabIndex = 2;
        statusStrip.Text = "statusStrip";
        // 
        // lblStatus
        // 
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(68, 17);
        lblStatus.Spring = true;
        lblStatus.Text = "就绪";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblEngine
        // 
        lblEngine.Name = "lblEngine";
        lblEngine.Size = new Size(80, 17);
        lblEngine.Text = "引擎：WebView";
        // 
        // splitMain
        // 
        splitMain.Dock = DockStyle.Fill;
        splitMain.Location = new Point(0, 50);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(lstHistory);
        splitMain.Panel1.Controls.Add(lblHistory);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(tabsMarkdown);
        splitMain.Size = new Size(1100, 578);
        splitMain.SplitterDistance = 260;
        splitMain.TabIndex = 3;
        // 
        // lblHistory
        // 
        lblHistory.Dock = DockStyle.Top;
        lblHistory.Location = new Point(0, 0);
        lblHistory.Name = "lblHistory";
        lblHistory.Size = new Size(260, 24);
        lblHistory.TabIndex = 0;
        lblHistory.Text = "最近文件（双击打开，悬停看完整路径）";
        lblHistory.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lstHistory
        // 
        lstHistory.Dock = DockStyle.Fill;
        lstHistory.FormattingEnabled = true;
        lstHistory.HorizontalScrollbar = false;
        lstHistory.IntegralHeight = false;
        lstHistory.Location = new Point(0, 24);
        lstHistory.Name = "lstHistory";
        lstHistory.Size = new Size(260, 554);
        lstHistory.TabIndex = 1;
        // 
        // tabsMarkdown
        // 
        tabsMarkdown.Dock = DockStyle.Fill;
        tabsMarkdown.Location = new Point(0, 0);
        tabsMarkdown.Name = "tabsMarkdown";
        tabsMarkdown.SelectedIndex = 0;
        tabsMarkdown.Size = new Size(836, 578);
        tabsMarkdown.TabIndex = 0;
        // 
        // FrmMain
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 650);
        Controls.Add(splitMain);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        Name = "FrmMain";
        Text = "JHViewer";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private MenuStrip menuStrip = null!;
    private ToolStripMenuItem mnuFile = null!;
    private ToolStripMenuItem mnuOpen = null!;
    private ToolStripMenuItem mnuSave = null!;
    private ToolStripMenuItem mnuExport = null!;
    private ToolStripMenuItem mnuExportCurrentSite = null!;
    private ToolStripMenuItem mnuExportFolderSite = null!;
    private ToolStripMenuItem mnuExportSingleHtml = null!;
    private ToolStripMenuItem mnuExportChm = null!;
    private ToolStripMenuItem mnuExportDialog = null!;
    private ToolStripMenuItem mnuExit = null!;
    private ToolStripMenuItem mnuTools = null!;
    private ToolStripMenuItem mnuMarkdownWiki = null!;
    private ToolStripMenuItem mnuExplorer = null!;
    private ToolStripMenuItem mnuRegisterContext = null!;
    private ToolStripMenuItem mnuUnregisterContext = null!;
    private ToolStripMenuItem mnuEnablePreview = null!;
    private ToolStripMenuItem mnuDisablePreview = null!;
    private ToolStripMenuItem mnuKillPrevhost = null!;
    private ToolStripMenuItem mnuRestartExplorer = null!;
    private ToolStripMenuItem mnuDiagnosePreview = null!;
    private ToolStripMenuItem mnuView = null!;
    private ToolStripMenuItem mnuEngineWebView = null!;
    private ToolStripMenuItem mnuEngineWinForms = null!;
    private ToolStripMenuItem mnuPreviewWindow = null!;
    private ToolStripMenuItem mnuPreviewDemo = null!;
    private ToolStripMenuItem mnuOpenDemoJson = null!;
    private ToolStripMenuItem mnuOpenDemoXml = null!;
    private ToolStripMenuItem mnuOpenDemoFolder = null!;
    private ToolStrip toolStrip = null!;
    private ToolStripButton btnOpen = null!;
    private ToolStripButton btnSaveCurrent = null!;
    private ToolStripButton btnExportWeb = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatus = null!;
    private ToolStripStatusLabel lblEngine = null!;
    private SplitContainer splitMain = null!;
    private Label lblHistory = null!;
    private ListBox lstHistory = null!;
    private TabControl tabsMarkdown = null!;
}
