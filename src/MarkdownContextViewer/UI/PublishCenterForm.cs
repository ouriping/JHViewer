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

        var compile = chkChmCompile.Checked;
        if (compile)
        {
            var compiler = ChmDesktopExporter.ResolveCompilerPath();
            if (string.IsNullOrWhiteSpace(compiler))
            {
                var expected = Path.Combine(AppContext.BaseDirectory, ChmDesktopExporter.BundledHhcRelativePath);
                var choice = MessageBox.Show(
                    "未找到 CHM 编译器 hhc.exe。\n\n" +
                    $"期望自带位置：\n{expected}\n\n" +
                    "也可安装 HTML Help Workshop，或将 hhc.exe 放到上述目录后重试。\n\n" +
                    "是否仍只生成中间工程（.hhp / .hhc / HTML），稍后再手工编译？",
                    "缺少 HHC.EXE",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (choice != DialogResult.Yes)
                {
                    return;
                }

                compile = false;
            }
        }

        try
        {
            Cursor = Cursors.WaitCursor;
            var result = ChmDesktopExporter.Export(root, compile: compile);
            var detail = result.Message + Environment.NewLine +
                         "构建目录：" + result.BuildDirectory + Environment.NewLine +
                         (result.Compiled
                             ? "CHM：" + result.ChmPath
                             : "未生成 CHM（可打开 Project.hhp 手工编译）");
            if (!string.IsNullOrWhiteSpace(result.CompilerUsed))
            {
                detail += Environment.NewLine + "编译器：" + result.CompilerUsed;
            }

            MessageBox.Show(detail, result.Ok ? "导出完成" : "导出失败", MessageBoxButtons.OK,
                result.Ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            // 打开输出目录或 CHM，不要直接打开 build（会锁住目录导致下次无法删除）
            if (result.Ok)
            {
                OpenChmExportResult(result);
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

    private static void OpenChmExportResult(MarkdownWiki.Core.Models.ChmExportResult result)
    {
        try
        {
            if (result.Compiled && !string.IsNullOrWhiteSpace(result.ChmPath) && File.Exists(result.ChmPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "/select,\"" + result.ChmPath + "\"",
                    UseShellExecute = true
                });
                return;
            }

            var openDir = !string.IsNullOrWhiteSpace(result.ProjectPath)
                ? Path.GetDirectoryName(result.ProjectPath)
                : result.BuildDirectory;
            // 优先打开 chm-output 根，而不是其下的 build
            if (!string.IsNullOrWhiteSpace(openDir))
            {
                var parent = Directory.GetParent(openDir);
                if (parent is not null
                    && openDir.EndsWith("build", StringComparison.OrdinalIgnoreCase)
                    && Directory.Exists(parent.FullName))
                {
                    openDir = parent.FullName;
                }
            }

            if (!string.IsNullOrWhiteSpace(openDir) && Directory.Exists(openDir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = openDir,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // 打开资源管理器失败不影响导出结果
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
