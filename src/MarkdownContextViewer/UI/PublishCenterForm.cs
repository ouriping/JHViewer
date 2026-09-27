using System.Diagnostics;

namespace MarkdownContextViewer;

public partial class PublishCenterForm : Form
{
    private readonly Func<(string filePath, string markdownText)?> _currentDocumentProvider = null!;
    private readonly SiteExportService _siteExportService = null!;
    private readonly DirectorySiteExportService _directorySiteExportService = null!;
    private readonly LocalWebHostService _localWebHostService = null!;

    public PublishCenterForm()
    {
        InitializeComponent();
    }

    public PublishCenterForm(
        Func<(string filePath, string markdownText)?> currentDocumentProvider,
        SiteExportService siteExportService,
        DirectorySiteExportService directorySiteExportService,
        LocalWebHostService localWebHostService)
        : this()
    {
        _currentDocumentProvider = currentDocumentProvider;
        _siteExportService = siteExportService;
        _directorySiteExportService = directorySiteExportService;
        _localWebHostService = localWebHostService;
    }

    private void btnExportCurrent_Click(object sender, EventArgs e) => ExportCurrentDocument();

    private void btnBrowse_Click(object sender, EventArgs e) => ChooseDirectory();

    private void btnExportDirectory_Click(object sender, EventArgs e) => ExportDirectorySite();

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
        var sourceRoot = txtDirectory.Text.Trim();
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
            txtDirectory.Text = dialog.SelectedPath;
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
