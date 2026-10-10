using MarkdownPreview.Handler.Enums;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

 

internal sealed class StructuredPreviewControl : UserControl
{
    private readonly SplitContainer _split;
    private readonly TreeView _tree;
    private readonly RichTextBox _raw;
    private readonly ToolStripDropDownButton _layoutButton;
    private StructuredLayout _layout = StructuredLayout.Both;

    public StructuredPreviewControl()
    {
        var bar = new PreviewToolStrip { Dock = DockStyle.Top };
        _layoutButton = new ToolStripDropDownButton("显示：树和文件")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            ToolTipText = "切换只看树、只看文件，或两边都显示"
        };
        var treeOnly = new ToolStripMenuItem("只显示树结构");
        var fileOnly = new ToolStripMenuItem("只显示文件");
        var both = new ToolStripMenuItem("都显示");
        treeOnly.Click += (_, __) => SetLayout(StructuredLayout.TreeOnly);
        fileOnly.Click += (_, __) => SetLayout(StructuredLayout.FileOnly);
        both.Click += (_, __) => SetLayout(StructuredLayout.Both);
        _layoutButton.DropDownItems.Add(treeOnly);
        _layoutButton.DropDownItems.Add(fileOnly);
        _layoutButton.DropDownItems.Add(both);
        bar.Items.Add(_layoutButton);

        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical
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
        Controls.Add(bar);
        Load += (_, __) => ApplyLayout();
    }

    private void SetLayout(StructuredLayout layout)
    {
        _layout = layout;
        HandlerLog.Info("操作: 结构化显示=" + layout);
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        _layoutButton.Text = _layout == StructuredLayout.TreeOnly
            ? "显示：只看树"
            : _layout == StructuredLayout.FileOnly
                ? "显示：只看文件"
                : "显示：树和文件";

        foreach (ToolStripMenuItem item in _layoutButton.DropDownItems)
        {
            item.Checked = (item.Text == "只显示树结构" && _layout == StructuredLayout.TreeOnly)
                || (item.Text == "只显示文件" && _layout == StructuredLayout.FileOnly)
                || (item.Text == "都显示" && _layout == StructuredLayout.Both);
        }

        _split.Panel1Collapsed = _layout == StructuredLayout.FileOnly;
        _split.Panel2Collapsed = _layout == StructuredLayout.TreeOnly;
        if (_layout == StructuredLayout.Both && _split.Width > 160)
        {
            try
            {
                _split.SplitterDistance = Math.Max(80, _split.Width / 2);
            }
            catch (InvalidOperationException)
            {
            }
        }
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
