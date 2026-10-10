using System;
using System.Windows.Forms;
using MarkdownPreview.Handler.Enums;

namespace MarkdownPreview.Handler;

/// <summary>
/// 结构化预览控件：树 + 原文（JSON / XML 等）。
/// 界面布局见 <c>StructuredPreviewControl.Designer.cs</c>，可在 VS 设计器中打开。
/// </summary>
internal partial class StructuredPreviewControl : UserControl
{
    private StructuredLayout _layout = StructuredLayout.Both;

    public StructuredPreviewControl()
    {
        InitializeComponent();
    }

    private void StructuredPreviewControl_Load(object sender, EventArgs e) => ApplyLayout();

    private void mnuTreeOnly_Click(object sender, EventArgs e) => SetLayout(StructuredLayout.TreeOnly);

    private void mnuFileOnly_Click(object sender, EventArgs e) => SetLayout(StructuredLayout.FileOnly);

    private void mnuBoth_Click(object sender, EventArgs e) => SetLayout(StructuredLayout.Both);

    private void SetLayout(StructuredLayout layout)
    {
        _layout = layout;
        HandlerLog.Info("操作: 结构化显示=" + layout);
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        btnLayout.Text = _layout == StructuredLayout.TreeOnly
            ? "显示：只看树"
            : _layout == StructuredLayout.FileOnly
                ? "显示：只看文件"
                : "显示：树和文件";

        mnuTreeOnly.Checked = _layout == StructuredLayout.TreeOnly;
        mnuFileOnly.Checked = _layout == StructuredLayout.FileOnly;
        mnuBoth.Checked = _layout == StructuredLayout.Both;

        splitMain.Panel1Collapsed = _layout == StructuredLayout.FileOnly;
        splitMain.Panel2Collapsed = _layout == StructuredLayout.TreeOnly;
        if (_layout == StructuredLayout.Both && splitMain.Width > 160)
        {
            try
            {
                splitMain.SplitterDistance = Math.Max(80, splitMain.Width / 2);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public void Bind(IDocumentPreviewProvider provider, string text)
    {
        rawView.Text = text ?? string.Empty;
        treeView.Nodes.Clear();
        try
        {
            provider.FillTree(treeView, text ?? string.Empty);
        }
        catch (Exception ex)
        {
            treeView.Nodes.Add(new TreeNode(provider.DisplayName + " 解析失败: " + ex.Message));
            HandlerLog.Error(provider.DisplayName + " 树渲染失败。", ex);
        }
    }
}
