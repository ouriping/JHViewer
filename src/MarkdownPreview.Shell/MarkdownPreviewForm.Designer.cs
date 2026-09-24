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
        toolPanel = new Panel();
        btnRefresh = new Button();
        btnOpen = new Button();
        markdownPreviewControl = new MarkdownPreviewControl();
        toolPanel.SuspendLayout();
        SuspendLayout();
        // 
        // toolPanel
        // 
        toolPanel.Controls.Add(btnRefresh);
        toolPanel.Controls.Add(btnOpen);
        toolPanel.Dock = DockStyle.Top;
        toolPanel.Location = new Point(0, 0);
        toolPanel.Name = "toolPanel";
        toolPanel.Padding = new Padding(8, 8, 8, 4);
        toolPanel.Size = new Size(800, 48);
        toolPanel.TabIndex = 0;
        // 
        // btnOpen
        // 
        btnOpen.Location = new Point(8, 8);
        btnOpen.Name = "btnOpen";
        btnOpen.Size = new Size(110, 30);
        btnOpen.TabIndex = 0;
        btnOpen.Text = "打开 .md";
        btnOpen.UseVisualStyleBackColor = true;
        btnOpen.Click += btnOpen_Click;
        // 
        // btnRefresh
        // 
        btnRefresh.Location = new Point(128, 8);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(110, 30);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "刷新";
        btnRefresh.UseVisualStyleBackColor = true;
        btnRefresh.Click += btnRefresh_Click;
        // 
        // markdownPreviewControl
        // 
        markdownPreviewControl.Dock = DockStyle.Fill;
        markdownPreviewControl.Location = new Point(0, 48);
        markdownPreviewControl.Name = "markdownPreviewControl";
        markdownPreviewControl.Size = new Size(800, 552);
        markdownPreviewControl.TabIndex = 1;
        // 
        // MarkdownPreviewForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 600);
        Controls.Add(markdownPreviewControl);
        Controls.Add(toolPanel);
        MinimumSize = new Size(480, 360);
        Name = "MarkdownPreviewForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Markdown 渲染预览";
        toolPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Panel toolPanel = null!;
    private Button btnOpen = null!;
    private Button btnRefresh = null!;
    private MarkdownPreviewControl markdownPreviewControl = null!;
}
