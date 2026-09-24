namespace MarkdownPreview.Shell;

partial class MarkdownPreviewControl
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
        webBrowserPreview = new WebBrowser();
        SuspendLayout();
        // 
        // webBrowserPreview
        // 
        webBrowserPreview.AllowWebBrowserDrop = false;
        webBrowserPreview.Dock = DockStyle.Fill;
        webBrowserPreview.IsWebBrowserContextMenuEnabled = true;
        webBrowserPreview.Location = new Point(0, 0);
        webBrowserPreview.MinimumSize = new Size(20, 20);
        webBrowserPreview.Name = "webBrowserPreview";
        webBrowserPreview.ScriptErrorsSuppressed = true;
        webBrowserPreview.Size = new Size(480, 640);
        webBrowserPreview.TabIndex = 0;
        webBrowserPreview.WebBrowserShortcutsEnabled = true;
        // 
        // MarkdownPreviewControl
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(webBrowserPreview);
        Name = "MarkdownPreviewControl";
        Size = new Size(480, 640);
        ResumeLayout(false);
    }

    private WebBrowser webBrowserPreview = null!;
}
