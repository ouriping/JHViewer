using System.Text.Json;
using System.Xml.Linq;

namespace MarkdownContextViewer;

/// <summary>
/// 主程序内 JSON / XML 结构化预览演示（树 + 原文），不依赖资源管理器 prevhost。
/// </summary>
public sealed class DocumentPreviewDemoForm : Form
{
    private const int MaxNodes = 2000;
    private const int MaxDepth = 12;

    private readonly SplitContainer _split = new()
    {
        Dock = DockStyle.Fill,
        Orientation = Orientation.Vertical,
        SplitterDistance = 320
    };

    private readonly TreeView _tree = new()
    {
        Dock = DockStyle.Fill,
        HideSelection = false,
        Font = new Font("Microsoft YaHei UI", 9.5f)
    };

    private readonly RichTextBox _raw = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 10f),
        WordWrap = false,
        DetectUrls = false,
        BackColor = Color.White
    };

    private readonly ToolStripLabel _lblPath = new()
    {
        Text = "未打开文件",
        AutoSize = false,
        Width = 420
    };

    private readonly ToolStripDropDownButton _layoutButton = new("显示：树和文件")
    {
        DisplayStyle = ToolStripItemDisplayStyle.Text,
        ToolTipText = "切换只看树、只看文件，或两边都显示"
    };

    private enum LayoutMode
    {
        Both,
        TreeOnly,
        FileOnly
    }

    private LayoutMode _layout = LayoutMode.Both;
    private string? _currentPath;

    public DocumentPreviewDemoForm()
    {
        Text = "结构化预览演示（JSON / XML）";
        Width = 960;
        Height = 640;
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Microsoft YaHei UI", 9f);

        var strip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System
        };
        var btnOpen = new ToolStripButton("打开…") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnRefresh = new ToolStripButton("刷新") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnDemoJson = new ToolStripButton("JSON 演示") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnDemoXml = new ToolStripButton("XML 演示") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        var btnFolder = new ToolStripButton("示例目录") { DisplayStyle = ToolStripItemDisplayStyle.Text };
        btnOpen.Click += (_, _) => OpenByDialog();
        btnRefresh.Click += (_, _) => Reload();
        btnDemoJson.Click += (_, _) => OpenDemo("book-catalog.json");
        btnDemoXml.Click += (_, _) => OpenDemo("library.xml");
        btnFolder.Click += (_, _) => OpenDemoFolder();
        var treeOnly = new ToolStripMenuItem("只显示树结构");
        var fileOnly = new ToolStripMenuItem("只显示文件");
        var both = new ToolStripMenuItem("都显示") { Checked = true };
        treeOnly.Click += (_, _) => SetLayout(LayoutMode.TreeOnly);
        fileOnly.Click += (_, _) => SetLayout(LayoutMode.FileOnly);
        both.Click += (_, _) => SetLayout(LayoutMode.Both);
        _layoutButton.DropDownItems.AddRange([treeOnly, fileOnly, both]);
        strip.Items.AddRange([btnOpen, btnRefresh, new ToolStripSeparator(), btnDemoJson, btnDemoXml, btnFolder, new ToolStripSeparator(), _layoutButton, _lblPath]);

        _split.Panel1.Controls.Add(_tree);
        _split.Panel2.Controls.Add(_raw);
        Controls.Add(_split);
        Controls.Add(strip);
    }

    public void LoadFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show(this, "文件不存在。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _currentPath = path;
        var text = File.ReadAllText(path);
        _raw.Text = text;
        _lblPath.Text = path;
        Text = $"结构化预览 - {Path.GetFileName(path)}";

        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear();
            var ext = Path.GetExtension(path);
            if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                FillJson(text);
            }
            else if (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                FillXml(text);
            }
            else
            {
                _tree.Nodes.Add("仅支持 .json / .xml");
            }
        }
        catch (Exception ex)
        {
            _tree.Nodes.Clear();
            _tree.Nodes.Add("解析失败: " + ex.Message);
        }
        finally
        {
            _tree.EndUpdate();
        }
    }

    private void OpenByDialog()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "结构化文本|*.json;*.xml|JSON (*.json)|*.json|XML (*.xml)|*.xml|所有文件|*.*",
            Title = "选择 JSON / XML 文件"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void Reload()
    {
        if (!string.IsNullOrWhiteSpace(_currentPath))
        {
            LoadFile(_currentPath);
        }
    }

    private void OpenDemo(string fileName)
    {
        var dir = PreviewDemoPaths.FindDirectory();
        if (dir is null)
        {
            MessageBox.Show(this, "未找到 samples/preview-demo 目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        LoadFile(Path.Combine(dir, fileName));
    }

    private void OpenDemoFolder()
    {
        var dir = PreviewDemoPaths.FindDirectory();
        if (dir is null)
        {
            MessageBox.Show(this, "未找到 samples/preview-demo 目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = dir,
            UseShellExecute = true
        });
    }

    private void SetLayout(LayoutMode layout)
    {
        _layout = layout;
        _layoutButton.Text = layout switch
        {
            LayoutMode.TreeOnly => "显示：只看树",
            LayoutMode.FileOnly => "显示：只看文件",
            _ => "显示：树和文件"
        };
        foreach (ToolStripMenuItem item in _layoutButton.DropDownItems)
        {
            item.Checked = (item.Text == "只显示树结构" && layout == LayoutMode.TreeOnly)
                || (item.Text == "只显示文件" && layout == LayoutMode.FileOnly)
                || (item.Text == "都显示" && layout == LayoutMode.Both);
        }

        _split.Panel1Collapsed = layout == LayoutMode.FileOnly;
        _split.Panel2Collapsed = layout == LayoutMode.TreeOnly;
        if (layout == LayoutMode.Both && _split.Width > 160)
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

    private void FillJson(string text)
    {
        using var doc = JsonDocument.Parse(text);
        var counter = new NodeCounter();
        var root = BuildJsonNode("JSON", doc.RootElement, 0, counter);
        _tree.Nodes.Add(root);
        root.Expand();
        if (counter.Truncated)
        {
            _tree.Nodes.Add($"… 已截断（上限 {MaxNodes} 节点 / 深度 {MaxDepth}）");
        }
    }

    private void FillXml(string text)
    {
        var doc = XDocument.Parse(text, LoadOptions.PreserveWhitespace);
        if (doc.Root is null)
        {
            _tree.Nodes.Add("(空文档)");
            return;
        }

        var counter = new NodeCounter();
        var root = BuildXmlNode(doc.Root, 0, counter);
        _tree.Nodes.Add(root);
        root.Expand();
        if (counter.Truncated)
        {
            _tree.Nodes.Add($"… 已截断（上限 {MaxNodes} 节点 / 深度 {MaxDepth}）");
        }
    }

    private static TreeNode BuildJsonNode(string name, JsonElement element, int depth, NodeCounter counter)
    {
        counter.Count++;
        if (counter.Count > MaxNodes || depth > MaxDepth)
        {
            counter.Truncated = true;
            return new TreeNode(name + " = …");
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var node = new TreeNode(name + " { }");
                foreach (var prop in element.EnumerateObject())
                {
                    if (counter.Count >= MaxNodes)
                    {
                        counter.Truncated = true;
                        node.Nodes.Add("…");
                        break;
                    }

                    node.Nodes.Add(BuildJsonNode(prop.Name, prop.Value, depth + 1, counter));
                }

                return node;
            }
            case JsonValueKind.Array:
            {
                var node = new TreeNode($"{name} [ {element.GetArrayLength()} ]");
                var i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    if (counter.Count >= MaxNodes)
                    {
                        counter.Truncated = true;
                        node.Nodes.Add("…");
                        break;
                    }

                    node.Nodes.Add(BuildJsonNode($"[{i++}]", item, depth + 1, counter));
                }

                return node;
            }
            case JsonValueKind.String:
                return new TreeNode($"{name} = \"{element.GetString()}\"");
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new TreeNode($"{name} = {element}");
            case JsonValueKind.Null:
                return new TreeNode($"{name} = null");
            default:
                return new TreeNode($"{name} = {element}");
        }
    }

    private static TreeNode BuildXmlNode(XElement element, int depth, NodeCounter counter)
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
            label += " " + string.Join(" ", element.Attributes().Select(a => $"{a.Name.LocalName}=\"{Trim(a.Value)}\""));
        }

        var text = string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
        if (!string.IsNullOrWhiteSpace(text) && !element.HasElements)
        {
            return new TreeNode($"{label} = {Trim(text)}");
        }

        var node = new TreeNode(label);
        if (!string.IsNullOrWhiteSpace(text))
        {
            node.Nodes.Add("#text = " + Trim(text));
        }

        foreach (var child in element.Elements())
        {
            if (counter.Count >= MaxNodes)
            {
                counter.Truncated = true;
                node.Nodes.Add("…");
                break;
            }

            node.Nodes.Add(BuildXmlNode(child, depth + 1, counter));
        }

        return node;
    }

    private static string Trim(string value)
    {
        var one = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return one.Length > 80 ? one[..77] + "…" : one;
    }

    private sealed class NodeCounter
    {
        public int Count;
        public bool Truncated;
    }
}

internal static class PreviewDemoPaths
{
    public static string? FindDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "samples", "preview-demo"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "samples", "preview-demo")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "samples", "preview-demo"))
        };

        return candidates.FirstOrDefault(Directory.Exists);
    }
}
