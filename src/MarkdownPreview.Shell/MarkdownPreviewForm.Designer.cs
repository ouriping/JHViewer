namespace MarkdownPreview.Shell;

partial class MarkdownPreviewForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        markdownPreviewControl = new MarkdownPreviewControl();
        menuStrip1 = new MenuStrip();
        打开文件OToolStripMenuItem = new ToolStripMenuItem();
        刷新RToolStripMenuItem = new ToolStripMenuItem();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // markdownPreviewControl
        // 
        markdownPreviewControl.Dock = DockStyle.Fill;
        markdownPreviewControl.Location = new Point(0, 28);
        markdownPreviewControl.Margin = new Padding(5, 5, 5, 5);
        markdownPreviewControl.Name = "markdownPreviewControl";
        markdownPreviewControl.Size = new Size(1029, 678);
        markdownPreviewControl.TabIndex = 1;
        // 
        // menuStrip1
        // 
        menuStrip1.ImageScalingSize = new Size(20, 20);
        menuStrip1.Items.AddRange(new ToolStripItem[] { 打开文件OToolStripMenuItem, 刷新RToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(1029, 28);
        menuStrip1.TabIndex = 2;
        menuStrip1.Text = "menuStrip1";
        // 
        // 打开文件OToolStripMenuItem
        // 
        打开文件OToolStripMenuItem.Name = "打开文件OToolStripMenuItem";
        打开文件OToolStripMenuItem.Size = new Size(105, 24);
        打开文件OToolStripMenuItem.Text = "打开文件(&O)";
        打开文件OToolStripMenuItem.Click += btnOpen_Click;
        // 
        // 刷新RToolStripMenuItem
        // 
        刷新RToolStripMenuItem.Name = "刷新RToolStripMenuItem";
        刷新RToolStripMenuItem.Size = new Size(73, 24);
        刷新RToolStripMenuItem.Text = "刷新(&R)";
        刷新RToolStripMenuItem.Click += btnRefresh_Click;
        // 
        // MarkdownPreviewForm
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1029, 706);
        Controls.Add(markdownPreviewControl);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Margin = new Padding(4, 4, 4, 4);
        MinimumSize = new Size(612, 415);
        Name = "MarkdownPreviewForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Markdown 渲染预览";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
    private MarkdownPreviewControl markdownPreviewControl = null!;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem 打开文件OToolStripMenuItem;
    private ToolStripMenuItem 刷新RToolStripMenuItem;
}
