namespace MarkdownContextViewer;

partial class WebMarkdownView
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CleanupTempFile();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        webBrowser = new WebBrowser();
        SuspendLayout();
        // 
        // webBrowser
        // 
        webBrowser.AllowWebBrowserDrop = false;
        webBrowser.Dock = DockStyle.Fill;
        webBrowser.IsWebBrowserContextMenuEnabled = true;
        webBrowser.Location = new Point(0, 0);
        webBrowser.MinimumSize = new Size(20, 20);
        webBrowser.Name = "webBrowser";
        webBrowser.ScriptErrorsSuppressed = true;
        webBrowser.Size = new Size(480, 640);
        webBrowser.TabIndex = 0;
        webBrowser.WebBrowserShortcutsEnabled = true;
        // 
        // WebMarkdownView
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(webBrowser);
        Name = "WebMarkdownView";
        Size = new Size(480, 640);
        ResumeLayout(false);
    }

    private WebBrowser webBrowser = null!;
}
