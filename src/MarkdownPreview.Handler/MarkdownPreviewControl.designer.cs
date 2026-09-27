using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MarkdownPreview.Handler
{
    partial class MarkdownPreviewControl
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseRuntime();
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.infoStrip = new MarkdownPreview.Handler.PreviewToolStrip();
            this.lblFile = new System.Windows.Forms.ToolStripLabel();
            this.lblHelp = new System.Windows.Forms.ToolStripLabel();
            this.lblUpdate = new System.Windows.Forms.ToolStripLabel();
            this.lblVersion = new System.Windows.Forms.ToolStripLabel();
            this.lblAuthor = new System.Windows.Forms.ToolStripLabel();
            this.actionStrip = new MarkdownPreview.Handler.PreviewToolStrip();
            this.btnRecent = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnHistory = new System.Windows.Forms.ToolStripButton();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnEditor = new System.Windows.Forms.ToolStripButton();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnMore = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuZoomReset = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.panelToolbar.SuspendLayout();
            this.infoStrip.SuspendLayout();
            this.actionStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelToolbar
            // 
            this.panelToolbar.AutoSize = true;
            this.panelToolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelToolbar.Controls.Add(this.infoStrip);
            this.panelToolbar.Controls.Add(this.actionStrip);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Location = new System.Drawing.Point(0, 0);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(720, 62);
            this.panelToolbar.TabIndex = 0;
            // 
            // infoStrip
            // 
            this.infoStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.infoStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.infoStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFile,
            this.lblHelp,
            this.lblUpdate,
            this.lblVersion,
            this.lblAuthor});
            this.infoStrip.Location = new System.Drawing.Point(0, 31);
            this.infoStrip.Name = "infoStrip";
            this.infoStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.infoStrip.Size = new System.Drawing.Size(720, 31);
            this.infoStrip.TabIndex = 1;
            this.infoStrip.Text = "infoStrip";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = false;
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(180, 22);
            this.lblFile.Text = "未打开文件";
            this.lblFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblHelp
            // 
            this.lblHelp.IsLink = true;
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(69, 22);
            this.lblHelp.Text = "帮助文档";
            this.lblHelp.Click += new System.EventHandler(this.lblHelp_Click);
            // 
            // lblUpdate
            // 
            this.lblUpdate.IsLink = true;
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(81, 22);
            this.lblUpdate.Text = "检查更新…";
            this.lblUpdate.Click += new System.EventHandler(this.lblUpdate_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(39, 22);
            this.lblVersion.Text = "v1.0";
            // 
            // lblAuthor
            // 
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(54, 22);
            this.lblAuthor.Text = "大白菜";
            // 
            // actionStrip
            // 
            this.actionStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.actionStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.actionStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRecent,
            this.btnHistory,
            this.btnRefresh,
            this.toolStripSeparator1,
            this.btnEditor,
            this.btnExport,
            this.toolStripSeparator2,
            this.btnMore});
            this.actionStrip.Location = new System.Drawing.Point(0, 0);
            this.actionStrip.Name = "actionStrip";
            this.actionStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.actionStrip.Size = new System.Drawing.Size(720, 31);
            this.actionStrip.TabIndex = 0;
            this.actionStrip.Text = "actionStrip";
            this.actionStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.actionStrip_ItemClicked);
            // 
            // btnRecent
            // 
            this.btnRecent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRecent.Name = "btnRecent";
            this.btnRecent.Size = new System.Drawing.Size(53, 24);
            this.btnRecent.Text = "最近";
            this.btnRecent.ToolTipText = "最近预览过的文件，选择后在窗格中查看";
            this.btnRecent.DropDownOpening += new System.EventHandler(this.btnRecent_DropDownOpening);
            // 
            // btnHistory
            // 
            this.btnHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(43, 24);
            this.btnHistory.Text = "记录";
            this.btnHistory.ToolTipText = "打开浏览记录管理器";
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(43, 24);
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.ToolTipText = "重新读取当前文件";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnEditor
            // 
            this.btnEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEditor.Name = "btnEditor";
            this.btnEditor.Size = new System.Drawing.Size(58, 24);
            this.btnEditor.Text = "编辑器";
            this.btnEditor.ToolTipText = "用 Markdown 编辑器打开当前文件";
            this.btnEditor.Click += new System.EventHandler(this.btnEditor_Click);
            // 
            // btnExport
            // 
            this.btnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(43, 24);
            this.btnExport.Text = "导出";
            this.btnExport.ToolTipText = "导出为内嵌样式和本地图片的 HTML 单文件";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnMore
            // 
            this.btnMore.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopyPath,
            this.mnuOpenFolder,
            this.mnuOpenBrowser,
            this.mnuZoomIn,
            this.mnuZoomOut,
            this.mnuZoomReset,
            this.mnuPrint});
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(53, 24);
            this.btnMore.Text = "更多";
            // 
            // mnuCopyPath
            // 
            this.mnuCopyPath.Name = "mnuCopyPath";
            this.mnuCopyPath.Size = new System.Drawing.Size(197, 26);
            this.mnuCopyPath.Text = "复制路径";
            this.mnuCopyPath.Click += new System.EventHandler(this.mnuCopyPath_Click);
            // 
            // mnuOpenFolder
            // 
            this.mnuOpenFolder.Name = "mnuOpenFolder";
            this.mnuOpenFolder.Size = new System.Drawing.Size(197, 26);
            this.mnuOpenFolder.Text = "打开所在文件夹";
            this.mnuOpenFolder.Click += new System.EventHandler(this.mnuOpenFolder_Click);
            // 
            // mnuOpenBrowser
            // 
            this.mnuOpenBrowser.Name = "mnuOpenBrowser";
            this.mnuOpenBrowser.Size = new System.Drawing.Size(197, 26);
            this.mnuOpenBrowser.Text = "在浏览器中打开";
            this.mnuOpenBrowser.Click += new System.EventHandler(this.mnuOpenBrowser_Click);
            // 
            // mnuZoomIn
            // 
            this.mnuZoomIn.Name = "mnuZoomIn";
            this.mnuZoomIn.Size = new System.Drawing.Size(197, 26);
            this.mnuZoomIn.Text = "放大";
            this.mnuZoomIn.Click += new System.EventHandler(this.mnuZoomIn_Click);
            // 
            // mnuZoomOut
            // 
            this.mnuZoomOut.Name = "mnuZoomOut";
            this.mnuZoomOut.Size = new System.Drawing.Size(197, 26);
            this.mnuZoomOut.Text = "缩小";
            this.mnuZoomOut.Click += new System.EventHandler(this.mnuZoomOut_Click);
            // 
            // mnuZoomReset
            // 
            this.mnuZoomReset.Name = "mnuZoomReset";
            this.mnuZoomReset.Size = new System.Drawing.Size(197, 26);
            this.mnuZoomReset.Text = "重置缩放";
            this.mnuZoomReset.Click += new System.EventHandler(this.mnuZoomReset_Click);
            // 
            // mnuPrint
            // 
            this.mnuPrint.Name = "mnuPrint";
            this.mnuPrint.Size = new System.Drawing.Size(197, 26);
            this.mnuPrint.Text = "打印";
            this.mnuPrint.Click += new System.EventHandler(this.mnuPrint_Click);
            // 
            // panelPreview
            // 
            this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPreview.Location = new System.Drawing.Point(0, 62);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(720, 418);
            this.panelPreview.TabIndex = 1;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // MarkdownPreviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panelPreview);
            this.Controls.Add(this.panelToolbar);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Name = "MarkdownPreviewControl";
            this.Size = new System.Drawing.Size(720, 480);
            this.panelToolbar.ResumeLayout(false);
            this.panelToolbar.PerformLayout();
            this.infoStrip.ResumeLayout(false);
            this.infoStrip.PerformLayout();
            this.actionStrip.ResumeLayout(false);
            this.actionStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Panel panelToolbar;
        private PreviewToolStrip infoStrip;
        private ToolStripLabel lblFile;
        private ToolStripLabel lblHelp;
        private ToolStripLabel lblUpdate;
        private ToolStripLabel lblVersion;
        private ToolStripLabel lblAuthor;
        private PreviewToolStrip actionStrip;
        private ToolStripDropDownButton btnRecent;
        private ToolStripButton btnHistory;
        private ToolStripButton btnRefresh;
        private ToolStripButton btnEditor;
        private ToolStripButton btnExport;
        private ToolStripDropDownButton btnMore;
        private ToolStripMenuItem mnuCopyPath;
        private ToolStripMenuItem mnuOpenFolder;
        private ToolStripMenuItem mnuOpenBrowser;
        private ToolStripSeparator sepMore1 = new ToolStripSeparator();
        private ToolStripMenuItem mnuZoomIn;
        private ToolStripMenuItem mnuZoomOut;
        private ToolStripMenuItem mnuZoomReset;
        private ToolStripSeparator sepMore2 = new ToolStripSeparator();
        private ToolStripMenuItem mnuPrint;
        private Panel panelPreview;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
    }
}
