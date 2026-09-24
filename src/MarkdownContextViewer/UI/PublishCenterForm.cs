using System.Diagnostics;

namespace MarkdownContextViewer;

public sealed class PublishCenterForm : Form
{
    private readonly Func<(string filePath, string markdownText)?> _currentDocumentProvider;
    private readonly SiteExportService _siteExportService;
    private readonly DirectorySiteExportService _directorySiteExportService;
    private readonly LocalWebHostService _localWebHostService;

    private readonly TextBox _txtDirectory;

    public PublishCenterForm(
        Func<(string filePath, string markdownText)?> currentDocumentProvider,
        SiteExportService siteExportService,
        DirectorySiteExportService directorySiteExportService,
        LocalWebHostService localWebHostService)
    {
        _currentDocumentProvider = currentDocumentProvider;
        _siteExportService = siteExportService;
        _directorySiteExportService = directorySiteExportService;
        _localWebHostService = localWebHostService;

        Text = "发布中心";
        StartPosition = FormStartPosition.CenterParent;
        Width = 760;
        Height = 420;

        var tabs = new TabControl { Dock = DockStyle.Fill };

        var tabSingle = new TabPage("快速发布（当前文档）");
        var panelSingle = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var lblSingle = new Label
        {
            Dock = DockStyle.Top,
            Height = 70,
            Text = "将当前标签页的 Markdown 导出到程序目录 site-output，并启动本地网站访问。"
        };
        var btnSingle = new Button
        {
            Dock = DockStyle.Top,
            Height = 38,
            Text = "导出当前文档并访问"
        };
        btnSingle.Click += (_, _) => ExportCurrentDocument();
        panelSingle.Controls.Add(btnSingle);
        panelSingle.Controls.Add(lblSingle);
        tabSingle.Controls.Add(panelSingle);

        var tabDirectory = new TabPage("目录站点");
        var panelDir = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var lblDir = new Label
        {
            Dock = DockStyle.Top,
            Height = 50,
            Text = "选择一个包含 .md 的目录，按目录结构导出站点并访问。"
        };

        var row = new Panel { Dock = DockStyle.Top, Height = 40 };
        _txtDirectory = new TextBox { Left = 0, Top = 8, Width = 560, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
        var btnBrowse = new Button { Left = 570, Top = 6, Width = 120, Height = 28, Text = "选择目录", Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnBrowse.Click += (_, _) => ChooseDirectory();
        row.Controls.Add(_txtDirectory);
        row.Controls.Add(btnBrowse);

        var btnDirPublish = new Button
        {
            Dock = DockStyle.Top,
            Height = 38,
            Text = "导出目录站点并访问"
        };
        btnDirPublish.Click += (_, _) => ExportDirectorySite();

        panelDir.Controls.Add(btnDirPublish);
        panelDir.Controls.Add(row);
        panelDir.Controls.Add(lblDir);
        tabDirectory.Controls.Add(panelDir);

        tabs.TabPages.Add(tabSingle);
        tabs.TabPages.Add(tabDirectory);
        Controls.Add(tabs);
    }

    private void ExportCurrentDocument()
    {
        var doc = _currentDocumentProvider();
        if (doc is null)
        {
            MessageBox.Show("请先在主界面打开一个 Markdown 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var outputRoot = Path.Combine(AppContext.BaseDirectory, "site-output");
        var indexPath = _siteExportService.ExportSingleDocument(doc.Value.filePath, doc.Value.markdownText, outputRoot);
        var url = _localWebHostService.Start(outputRoot, 5088);

        OpenBrowser(url);
        MessageBox.Show($"发布成功。\n索引页：{indexPath}\n访问地址：{url}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportDirectorySite()
    {
        var sourceRoot = _txtDirectory.Text.Trim();
        if (string.IsNullOrWhiteSpace(sourceRoot) || !Directory.Exists(sourceRoot))
        {
            MessageBox.Show("请先选择有效的目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var outputRoot = Path.Combine(AppContext.BaseDirectory, "site-output-directory");
        var result = _directorySiteExportService.ExportDirectory(sourceRoot, outputRoot);
        var url = _localWebHostService.Start(outputRoot, 5088);

        OpenBrowser(url);
        MessageBox.Show($"发布成功。\n索引页：{result.indexPath}\n文档数量：{result.count}\n访问地址：{url}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ChooseDirectory()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择包含 Markdown 文档的目录"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _txtDirectory.Text = dialog.SelectedPath;
        }
    }

    private static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}

