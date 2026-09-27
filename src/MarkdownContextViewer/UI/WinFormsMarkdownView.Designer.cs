namespace MarkdownContextViewer;

partial class WinFormsMarkdownView
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
        richPreview = new RichTextBox();
        SuspendLayout();
        // 
        // richPreview
        // 
        richPreview.BackColor = Color.White;
        richPreview.BorderStyle = BorderStyle.None;
        richPreview.Dock = DockStyle.Fill;
        richPreview.Font = new Font("Microsoft YaHei UI", 10.5F);
        richPreview.Location = new Point(0, 0);
        richPreview.Name = "richPreview";
        richPreview.ReadOnly = true;
        richPreview.ScrollBars = RichTextBoxScrollBars.Both;
        richPreview.Size = new Size(480, 360);
        richPreview.TabIndex = 0;
        richPreview.Text = "";
        // 
        // WinFormsMarkdownView
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(richPreview);
        Name = "WinFormsMarkdownView";
        Size = new Size(480, 360);
        ResumeLayout(false);
    }

    private RichTextBox richPreview = null!;
}
