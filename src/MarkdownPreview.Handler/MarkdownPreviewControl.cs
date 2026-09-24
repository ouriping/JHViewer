using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SharpShell.SharpPreviewHandler;

namespace MarkdownPreview.Handler;

/// <summary>
/// SharpShell 预览宿主控件：用 WebView2 渲染 HTML。
/// </summary>
public sealed class MarkdownPreviewControl : PreviewHandlerControl
{
    private readonly WebView2 _webView;
    private string _pendingHtml;
    private bool _initStarted;

    public MarkdownPreviewControl()
    {
        _webView = new WebView2
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_webView);
        HandleCreated += (_, __) => BeginInitWebView();
    }

    public void SetHtmlContent(string html)
    {
        _pendingHtml = html ?? string.Empty;
        if (IsHandleCreated)
        {
            BeginInitWebView();
        }

        TryNavigate();
    }

    private void BeginInitWebView()
    {
        if (_initStarted)
        {
            return;
        }

        _initStarted = true;
        _ = InitWebViewAsync();
    }

    private async Task InitWebViewAsync()
    {
        try
        {
            // prevhost 低完整性只能写 LocalLow；UserDataFolder 必须可写。
            var userData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
                "MarkdownContextViewer",
                "WebView2");
            Directory.CreateDirectory(userData);

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: userData)
                .ConfigureAwait(true);
            await _webView.EnsureCoreWebView2Async(env).ConfigureAwait(true);

            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

            HandlerLog.Info("WebView2 initialized.");
            TryNavigate();
        }
        catch (Exception ex)
        {
            HandlerLog.Error("WebView2 init failed.", ex);
            Controls.Clear();
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "WebView2 初始化失败：\r\n" + ex.Message +
                       "\r\n请确认已安装 WebView2 Runtime。"
            });
        }
    }

    private void TryNavigate()
    {
        if (_pendingHtml is null || _webView.CoreWebView2 is null)
        {
            return;
        }

        try
        {
            _webView.CoreWebView2.NavigateToString(_pendingHtml);
            HandlerLog.Info($"NavigateToString length={_pendingHtml.Length}");
        }
        catch (Exception ex)
        {
            HandlerLog.Error("NavigateToString failed.", ex);
        }
    }
}
