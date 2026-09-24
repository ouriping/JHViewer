using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPreviewHandler;

namespace MarkdownPreview.Handler;

/// <summary>
/// 资源管理器预览窗格：SharpShell + Markdig + WebView2。
/// </summary>
[ComVisible(true)]
[Guid(HandlerIds.Clsid)]
[DisplayName(HandlerIds.DisplayName)]
[COMServerAssociation(AssociationType.ClassOfExtension, ".md")]
[PreviewHandler(
    DisableLowILProcessIsolation = true,
    SurrogateHostType = SurrogateHostType.DedicatedPrevhost)]
public sealed class MarkdownPreviewHandler : SharpPreviewHandler
{
    protected override PreviewHandlerControl DoPreview()
    {
        var control = new MarkdownPreviewControl();
        var path = SelectedFilePath;
        HandlerLog.Info($"DoPreview: {path}");

        try
        {
            if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                control.ShowFile(path);
            }
            else
            {
                control.ShowMarkdown("**无法读取 Markdown 文件。**");
            }
        }
        catch (System.Exception ex)
        {
            HandlerLog.Error("DoPreview failed.", ex);
            control.ShowMarkdown($"**预览失败：** {ex.Message}");
        }

        return control;
    }
}
