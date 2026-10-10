using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MarkdownPreview.Handler
{
    partial class StructuredPreviewControl
    {
        private IContainer components = null;

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
            this.toolStripBar = new MarkdownPreview.Handler.PreviewToolStrip();
            this.btnLayout = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuTreeOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBoth = new System.Windows.Forms.ToolStripMenuItem();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.rawView = new System.Windows.Forms.RichTextBox();
            this.toolStripBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripBar
            // 
            this.toolStripBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnLayout});
            this.toolStripBar.Location = new System.Drawing.Point(0, 0);
            this.toolStripBar.Name = "toolStripBar";
            this.toolStripBar.Size = new System.Drawing.Size(640, 27);
            this.toolStripBar.TabIndex = 0;
            this.toolStripBar.Text = "toolStripBar";
            // 
            // btnLayout
            // 
            this.btnLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTreeOnly,
            this.mnuFileOnly,
            this.mnuBoth});
            this.btnLayout.Name = "btnLayout";
            this.btnLayout.Size = new System.Drawing.Size(110, 24);
            this.btnLayout.Text = "显示：树和文件";
            this.btnLayout.ToolTipText = "切换只看树、只看文件，或两边都显示";
            // 
            // mnuTreeOnly
            // 
            this.mnuTreeOnly.Name = "mnuTreeOnly";
            this.mnuTreeOnly.Size = new System.Drawing.Size(180, 26);
            this.mnuTreeOnly.Text = "只显示树结构";
            this.mnuTreeOnly.Click += new System.EventHandler(this.mnuTreeOnly_Click);
            // 
            // mnuFileOnly
            // 
            this.mnuFileOnly.Name = "mnuFileOnly";
            this.mnuFileOnly.Size = new System.Drawing.Size(180, 26);
            this.mnuFileOnly.Text = "只显示文件";
            this.mnuFileOnly.Click += new System.EventHandler(this.mnuFileOnly_Click);
            // 
            // mnuBoth
            // 
            this.mnuBoth.Name = "mnuBoth";
            this.mnuBoth.Size = new System.Drawing.Size(180, 26);
            this.mnuBoth.Text = "都显示";
            this.mnuBoth.Click += new System.EventHandler(this.mnuBoth_Click);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 27);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.treeView);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.rawView);
            this.splitMain.Size = new System.Drawing.Size(640, 413);
            this.splitMain.SplitterDistance = 280;
            this.splitMain.TabIndex = 1;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(280, 413);
            this.treeView.TabIndex = 0;
            // 
            // rawView
            // 
            this.rawView.BackColor = System.Drawing.Color.White;
            this.rawView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rawView.DetectUrls = false;
            this.rawView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rawView.Font = new System.Drawing.Font("Consolas", 10F);
            this.rawView.Location = new System.Drawing.Point(0, 0);
            this.rawView.Name = "rawView";
            this.rawView.ReadOnly = true;
            this.rawView.Size = new System.Drawing.Size(356, 413);
            this.rawView.TabIndex = 0;
            this.rawView.Text = "";
            this.rawView.WordWrap = false;
            // 
            // StructuredPreviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.toolStripBar);
            this.Name = "StructuredPreviewControl";
            this.Size = new System.Drawing.Size(640, 440);
            this.Load += new System.EventHandler(this.StructuredPreviewControl_Load);
            this.toolStripBar.ResumeLayout(false);
            this.toolStripBar.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private MarkdownPreview.Handler.PreviewToolStrip toolStripBar;
        private System.Windows.Forms.ToolStripDropDownButton btnLayout;
        private System.Windows.Forms.ToolStripMenuItem mnuTreeOnly;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOnly;
        private System.Windows.Forms.ToolStripMenuItem mnuBoth;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.RichTextBox rawView;
    }
}
