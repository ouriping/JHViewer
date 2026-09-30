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

    private void btnStartWiki_Click(object sender, EventArgs e) => StartMarkdownWiki();

    private void btnOpenWiki_Click(object sender, EventArgs e) => MarkdownWikiLauncher.OpenBrowser();

    private void btnWikiBrowse_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择 MarkdownWiki 文档根目录"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtWikiDirectory.Text = dialog.SelectedPath;
        }
    }

    private void btnChmBrowse_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择要打包为 CHM 的 Markdown 文档根目录"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtChmDirectory.Text = dialog.SelectedPath;
        }
    }

    private void btnExportChm_Click(object sender, EventArgs e)
    {
        var root = txtChmDirectory.Text.Trim();
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            MessageBox.Show("请先选择有效的文档根目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            var result = ChmDesktopExporter.Export(root, compile: chkChmCompile.Checked);
            var detail = result.Message + Environment.NewLine +
                         "构建目录：" + result.BuildDirectory + Environment.NewLine +
                         (result.Compiled ? "CHM：" + result.ChmPath : "未生成 CHM（可打开 Project.hhp 手工编译）");
            MessageBox.Show(detail, result.Ok ? "导出完成" : "导出失败", MessageBoxButtons.OK,
                result.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (result.Ok && Directory.Exists(result.BuildDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = result.BuildDirectory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void StartMarkdownWiki()
    {
        try
        {
            var content = txtWikiDirectory.Text.Trim();
            MarkdownWikiLauncher.Start(string.IsNullOrWhiteSpace(content) ? null : content);
            Thread.Sleep(1200);
            MarkdownWikiLauncher.OpenBrowser();
            MessageBox.Show("已尝试启动 MarkdownWiki。\n访问地址：http://127.0.0.1:5099/\n若首次启动请稍等编译完成后再刷新。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
