using System;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

/// <summary>
/// 预览窗格里的工具栏。资源管理器对预览窗口返回“不激活”，
/// ToolStrip 收不到有效点击；这里把 WM_MOUSEACTIVATE 改成激活。
/// </summary>
public class PreviewToolStrip : ToolStrip
{
    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_ACTIVATE = 1;

    public PreviewToolStrip()
    {
        GripStyle = ToolStripGripStyle.Hidden;
        RenderMode = ToolStripRenderMode.System;
        CanOverflow = true;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_MOUSEACTIVATE)
        {
            HandlerLog.Info("操作: 工具栏收到鼠标激活 " + Name);
            m.Result = (IntPtr)MA_ACTIVATE;
            return;
        }

        base.WndProc(ref m);
    }
}
