namespace MarkdownContextViewer;

partial class FrmTestView
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
        markdownEditorControl1 = new MarkdownEditorControl();
        SuspendLayout();
        // 
        // markdownEditorControl1
        // 
        markdownEditorControl1.Dock = DockStyle.Fill;
        markdownEditorControl1.Location = new Point(0, 0);
        markdownEditorControl1.Margin = new Padding(4, 4, 4, 4);
        markdownEditorControl1.MarkdownText = "";
        markdownEditorControl1.Name = "markdownEditorControl1";
        markdownEditorControl1.Size = new Size(1700, 1124);
        markdownEditorControl1.TabIndex = 0;
        // 
        // FrmTestView
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1700, 1124);
        Controls.Add(markdownEditorControl1);
        Name = "FrmTestView";
        Text = "FrmTestView";
        ResumeLayout(false);
    }

    private MarkdownEditorControl markdownEditorControl1 = null!;
}
