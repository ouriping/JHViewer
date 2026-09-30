using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MarkdownPreview.Handler;

internal sealed class XmlDocumentPreviewProvider : IDocumentPreviewProvider
{
    private const int MaxNodes = 2000;
    private const int MaxDepth = 12;

    public string Id => "xml";

    public string DisplayName => "XML";

    public IReadOnlyList<string> Extensions { get; } = new[] { ".xml" };

    public bool IsMarkdown => false;

    public bool PreferWinForms => true;

    public void FillTree(TreeView tree, string text)
    {
        tree.BeginUpdate();
        try
        {
            tree.Nodes.Clear();
            var doc = XDocument.Parse(text ?? string.Empty, LoadOptions.PreserveWhitespace);
            var counter = new NodeCounter();
            if (doc.Root == null)
            {
                tree.Nodes.Add("(空文档)");
                return;
            }

            var node = BuildElement(doc.Root, 0, counter);
            tree.Nodes.Add(node);
            node.Expand();
            if (counter.Truncated)
            {
                tree.Nodes.Add(new TreeNode($"… 已截断（上限 {MaxNodes} 节点 / 深度 {MaxDepth}）"));
            }
        }
        finally
        {
            tree.EndUpdate();
        }
    }

    public string ToHtml(string text)
    {
        try
        {
            var doc = XDocument.Parse(text ?? string.Empty, LoadOptions.PreserveWhitespace);
            var sb = new StringBuilder();
            sb.Append("<ul class=\"tree\">");
            if (doc.Root != null)
            {
                AppendElementHtml(sb, doc.Root, 0, new NodeCounter());
            }

            sb.Append("</ul>");
            return JsonDocumentPreviewProvider.WrapHtml("XML 预览", sb.ToString(), text);
        }
        catch (Exception ex)
        {
            return JsonDocumentPreviewProvider.WrapHtml(
                "XML 预览",
                $"<p class=\"err\">解析失败：{JsonDocumentPreviewProvider.HtmlEncode(ex.Message)}</p>",
                text);
        }
    }

    private static TreeNode BuildElement(XElement element, int depth, NodeCounter counter)
    {
        counter.Count++;
        if (counter.Count > MaxNodes || depth > MaxDepth)
        {
            counter.Truncated = true;
            return new TreeNode(element.Name.LocalName + " …");
        }

        var label = element.Name.LocalName;
        if (element.HasAttributes)
        {
            label += " " + string.Join(" ", AttrSummary(element));
        }

        var node = new TreeNode(label);
        var text = GetDirectText(element);
        if (!string.IsNullOrWhiteSpace(text) && !element.HasElements)
        {
            node.Text = label + " = " + TrimValue(text);
            return node;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            node.Nodes.Add("#text = " + TrimValue(text));
        }

        foreach (var child in element.Elements())
        {
            if (counter.Count >= MaxNodes)
            {
                counter.Truncated = true;
                node.Nodes.Add("…");
                break;
            }

            node.Nodes.Add(BuildElement(child, depth + 1, counter));
        }

        return node;
    }

    private static void AppendElementHtml(StringBuilder sb, XElement element, int depth, NodeCounter counter)
    {
        counter.Count++;
        if (counter.Count > MaxNodes || depth > MaxDepth)
        {
            counter.Truncated = true;
            sb.Append("<li>").Append(JsonDocumentPreviewProvider.HtmlEncode(element.Name.LocalName)).Append(" …</li>");
            return;
        }

        var label = element.Name.LocalName;
        sb.Append("<li><b>").Append(JsonDocumentPreviewProvider.HtmlEncode(label)).Append("</b>");
        if (element.HasAttributes)
        {
            sb.Append(" <span style=\"color:#57606a\">")
                .Append(JsonDocumentPreviewProvider.HtmlEncode(string.Join(" ", AttrSummary(element))))
                .Append("</span>");
        }

        var text = GetDirectText(element);
        if (!string.IsNullOrWhiteSpace(text) && !element.HasElements)
        {
            sb.Append(" = ").Append(JsonDocumentPreviewProvider.HtmlEncode(TrimValue(text))).Append("</li>");
            return;
        }

        sb.Append("<ul>");
        if (!string.IsNullOrWhiteSpace(text))
        {
            sb.Append("<li>#text = ").Append(JsonDocumentPreviewProvider.HtmlEncode(TrimValue(text))).Append("</li>");
        }

        foreach (var child in element.Elements())
        {
            if (counter.Count >= MaxNodes)
            {
                counter.Truncated = true;
                sb.Append("<li>…</li>");
                break;
            }

            AppendElementHtml(sb, child, depth + 1, counter);
        }

        sb.Append("</ul></li>");
    }

    private static IEnumerable<string> AttrSummary(XElement element)
    {
        foreach (var attr in element.Attributes())
        {
            yield return attr.Name.LocalName + "=\"" + TrimValue(attr.Value) + "\"";
        }
    }

    private static string GetDirectText(XElement element)
    {
        var sb = new StringBuilder();
        foreach (var node in element.Nodes())
        {
            if (node is XText text)
            {
                sb.Append(text.Value);
            }
        }

        return sb.ToString().Trim();
    }

    private static string TrimValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var oneLine = value.Replace("\r", " ").Replace("\n", " ").Trim();
        return oneLine.Length > 80 ? oneLine.Substring(0, 77) + "…" : oneLine;
    }

    private sealed class NodeCounter
    {
        public int Count;
        public bool Truncated;
    }
}
