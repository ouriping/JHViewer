using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

/// <summary>
/// JSON / XML 的 WinForms 预览：树 + 原文。
/// </summary>
internal sealed class StructuredPreviewControl : UserControl
{
    private readonly SplitContainer _split;
    private readonly TreeView _tree;
    private readonly RichTextBox _raw;

    public StructuredPreviewControl()
    {
        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 280
        };

        _tree = new TreeView
        {
            Dock = DockStyle.Fill,
            HideSelection = false,
            Font = new Font("Microsoft YaHei UI", 9.5f)
        };

        _raw = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 10f),
            WordWrap = false,
            DetectUrls = false,
            BackColor = Color.White
        };

        _split.Panel1.Controls.Add(_tree);
        _split.Panel2.Controls.Add(_raw);
        Controls.Add(_split);
    }

    public void Bind(IDocumentPreviewProvider provider, string text)
    {
        _raw.Text = text ?? string.Empty;
        _tree.Nodes.Clear();
        try
        {
            provider.FillTree(_tree, text ?? string.Empty);
        }
        catch (Exception ex)
        {
            _tree.Nodes.Add(new TreeNode(provider.DisplayName + " 解析失败: " + ex.Message));
            HandlerLog.Error(provider.DisplayName + " 树渲染失败。", ex);
        }
    }
}
