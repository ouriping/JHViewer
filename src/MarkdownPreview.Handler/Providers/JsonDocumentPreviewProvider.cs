using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

internal sealed class JsonDocumentPreviewProvider : IDocumentPreviewProvider
{
    private const int MaxNodes = 2000;
    private const int MaxDepth = 12;

    public string Id => "json";

    public string DisplayName => "JSON";

    public IReadOnlyList<string> Extensions { get; } = new[] { ".json" };

    public bool IsMarkdown => false;

    public bool PreferWinForms => true;

    public void FillTree(TreeView tree, string text)
    {
        tree.BeginUpdate();
        try
        {
            tree.Nodes.Clear();
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var root = serializer.DeserializeObject(text ?? string.Empty);
            var counter = new NodeCounter();
            var node = BuildNode("JSON", root, 0, counter);
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
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var root = serializer.DeserializeObject(text ?? string.Empty);
            var sb = new StringBuilder();
            sb.Append("<ul class=\"tree\">");
            AppendHtml(sb, "JSON", root, 0, new NodeCounter());
            sb.Append("</ul>");
            return WrapHtml("JSON 预览", sb.ToString(), text);
        }
        catch (Exception ex)
        {
            return WrapHtml("JSON 预览", $"<p class=\"err\">解析失败：{HtmlEncode(ex.Message)}</p>", text);
        }
    }

    private static TreeNode BuildNode(string name, object value, int depth, NodeCounter counter)
    {
        counter.Count++;
        if (counter.Count > MaxNodes || depth > MaxDepth)
        {
            counter.Truncated = true;
            return new TreeNode(name + " = …");
        }

        if (value == null)
        {
            return new TreeNode(name + " = null");
        }

        if (value is string || value is bool || value is int || value is long || value is double || value is decimal)
        {
            return new TreeNode(name + " = " + FormatScalar(value));
        }

        if (value is Dictionary<string, object> obj)
        {
            var node = new TreeNode(name + " { }");
            foreach (var kv in obj)
            {
                if (counter.Count >= MaxNodes)
                {
                    counter.Truncated = true;
                    node.Nodes.Add("…");
                    break;
                }

                node.Nodes.Add(BuildNode(kv.Key, kv.Value, depth + 1, counter));
            }

            return node;
        }

        if (value is ArrayList list)
        {
            var node = new TreeNode(name + $" [ {list.Count} ]");
            for (var i = 0; i < list.Count; i++)
            {
                if (counter.Count >= MaxNodes)
                {
                    counter.Truncated = true;
                    node.Nodes.Add("…");
                    break;
                }

                node.Nodes.Add(BuildNode("[" + i + "]", list[i], depth + 1, counter));
            }

            return node;
        }

        return new TreeNode(name + " = " + value);
    }

    private static void AppendHtml(StringBuilder sb, string name, object value, int depth, NodeCounter counter)
    {
        counter.Count++;
        if (counter.Count > MaxNodes || depth > MaxDepth)
        {
            counter.Truncated = true;
            sb.Append("<li>").Append(HtmlEncode(name)).Append(" = …</li>");
            return;
        }

        if (value == null)
        {
            sb.Append("<li><b>").Append(HtmlEncode(name)).Append("</b> = <i>null</i></li>");
            return;
        }

        if (value is string || value is bool || value is int || value is long || value is double || value is decimal)
        {
            sb.Append("<li><b>").Append(HtmlEncode(name)).Append("</b> = ")
                .Append(HtmlEncode(FormatScalar(value))).Append("</li>");
            return;
        }

        if (value is Dictionary<string, object> obj)
        {
            sb.Append("<li><b>").Append(HtmlEncode(name)).Append("</b> { }<ul>");
            foreach (var kv in obj)
            {
                if (counter.Count >= MaxNodes)
                {
                    counter.Truncated = true;
                    sb.Append("<li>…</li>");
                    break;
                }

                AppendHtml(sb, kv.Key, kv.Value, depth + 1, counter);
            }

            sb.Append("</ul></li>");
            return;
        }

        if (value is ArrayList list)
        {
            sb.Append("<li><b>").Append(HtmlEncode(name)).Append("</b> [ ").Append(list.Count).Append(" ]<ul>");
            for (var i = 0; i < list.Count; i++)
            {
                if (counter.Count >= MaxNodes)
                {
                    counter.Truncated = true;
                    sb.Append("<li>…</li>");
                    break;
                }

                AppendHtml(sb, "[" + i + "]", list[i], depth + 1, counter);
            }

            sb.Append("</ul></li>");
            return;
        }

        sb.Append("<li><b>").Append(HtmlEncode(name)).Append("</b> = ")
            .Append(HtmlEncode(Convert.ToString(value))).Append("</li>");
    }

    private static string FormatScalar(object value)
    {
        if (value is string s)
        {
            return "\"" + s + "\"";
        }

        if (value is bool b)
        {
            return b ? "true" : "false";
        }

        return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    internal static string WrapHtml(string title, string body, string raw)
    {
        return $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8"" />
<title>{HtmlEncode(title)}</title>
<style>
body {{ font-family: Consolas, ""Microsoft YaHei UI"", sans-serif; margin: 12px; background:#fff; color:#24292f; }}
.err {{ color:#cf222e; }}
.tree {{ line-height: 1.45; }}
pre {{ background:#f6f8fa; padding:12px; border-radius:6px; overflow:auto; white-space:pre-wrap; }}
h2 {{ font-size:14px; margin:12px 0 6px; color:#57606a; }}
</style></head><body>
{body}
<h2>原文</h2>
<pre>{HtmlEncode(raw ?? string.Empty)}</pre>
</body></html>";
    }

    internal static string HtmlEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private sealed class NodeCounter
    {
        public int Count;
        public bool Truncated;
    }
}
