using System.ComponentModel;
using System.Diagnostics;

namespace MarkdownContextViewer;

public enum ExportPreset
{
    Site,
    CurrentSite,
    FolderSite,
    SingleHtml,
    Chm,
    Wiki
}

public partial class PublishCenterForm : Form
{
    private readonly Func<(string filePath, string markdownText)?> _currentDocumentProvider = null!;
    private readonly SiteExportService _siteExportService = null!;
    private readonly DirectorySiteExportService _directorySiteExportService = null!;
    private readonly SingleHtmlExportService _singleHtmlExportService = null!;
    private readonly LocalWebHostService _localWebHostService = null!;
    private readonly ExportPreset _preset;
    private bool _updatingUi;

    public PublishCenterForm()
    {
        InitializeComponent();
        _preset = ExportPreset.Site;
    }

    public PublishCenterForm(
        Func<(string filePath, string markdownText)?> currentDocumentProvider,
        SiteExportService siteExportService,
        DirectorySiteExportService directorySiteExportService,
        SingleHtmlExportService singleHtmlExportService,
        LocalWebHostService localWebHostService,
        ExportPreset preset = ExportPreset.Site)
        : this()
    {
        _currentDocumentProvider = currentDocumentProvider;
        _siteExportService = siteExportService;
        _directorySiteExportService = directorySiteExportService;
        _singleHtmlExportService = singleHtmlExportService;
        _localWebHostService = localWebHostService;
        _preset = preset;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || _siteExportService is null)
        {
            return;
        }

        InitializeFromDocument();
        ApplyPreset(_preset);
        UpdateUi();
    }

    private void InitializeFromDocument()
    {
        var doc = _currentDocumentProvider?.Invoke();
        if (doc is null)
        {
            lblCurrentDoc.Text = "未打开文档";
            radioCurrent.Enabled = false;
            radioFolder.Checked = true;
            return;
        }

        lblCurrentDoc.Text = doc.Value.filePath;
        var dir = Path.GetDirectoryName(doc.Value.filePath);
        if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir) && string.IsNullOrWhiteSpace(txtDirectory.Text))
        {
            txtDirectory.Text = dir;
        }
    }

    public void ApplyPreset(ExportPreset preset)
    {
        switch (preset)
        {
            case ExportPreset.CurrentSite:
                if (radioCurrent.Enabled)
                {
                    radioCurrent.Checked = true;
                }
                else
                {
                    radioFolder.Checked = true;
                }

                radioSite.Checked = true;
                break;
            case ExportPreset.FolderSite:
                radioFolder.Checked = true;
                radioSite.Checked = true;
                break;
            case ExportPreset.SingleHtml:
                radioFolder.Checked = true;
                radioSingleHtml.Checked = true;
                break;
            case ExportPreset.Chm:
                radioFolder.Checked = true;
                radioChm.Checked = true;
                break;
            case ExportPreset.Wiki:
                radioFolder.Checked = true;
                radioWiki.Checked = true;
                break;
            default:
                if (radioCurrent.Enabled)
                {
                    radioCurrent.Checked = true;
                }

                radioSite.Checked = true;
                break;
        }

        UpdateUi();
    }

    private void SourceOrTargetChanged(object? sender, EventArgs e)
    {
        if (!_updatingUi)
        {
            UpdateUi();
        }
    }

    private void UpdateUi()
    {
        if (_updatingUi)
        {
            return;
        }

        _updatingUi = true;
        try
        {
            UpdateUiCore();
        }
        finally
        {
            _updatingUi = false;
        }
    }

    private void UpdateUiCore()
    {
        var wiki = radioWiki.Checked;
        var chm = radioChm.Checked;
        var singleHtml = radioSingleHtml.Checked;
        var site = radioSite.Checked;
        var useCurrent = radioCurrent.Checked && radioCurrent.Enabled;

        if ((chm || wiki) && useCurrent)
        {
            radioFolder.Checked = true;
            useCurrent = false;
        }

        var allowCurrent = (site || singleHtml)
            && !string.IsNullOrWhiteSpace(lblCurrentDoc.Text)
            && !lblCurrentDoc.Text.Equals("未打开文档", StringComparison.Ordinal);
        radioCurrent.Enabled = allowCurrent;
        panelDirRow.Enabled = !useCurrent || chm || wiki || (singleHtml && radioFolder.Checked);
        chkChmCompile.Visible = chm;
        btnOpenWiki.Visible = wiki;
        chkOpenAfter.Visible = !wiki;
        chkOpenAfter.Text = chm
            ? "完成后在资源管理器中显示"
            : singleHtml
                ? "完成后打开 docs.html"
                : "完成后在浏览器中打开";
        btnPrimary.Text = wiki ? "启动" : "导出";

        if (wiki)
        {
            lblHint.Text = "启动独立 MarkdownWiki（端口 5099）。目录可留空以使用默认示例。";
        }
        else if (chm)
        {
            lblHint.Text = "将目录中的 Markdown 打包为 Windows CHM，输出在文档根旁的 chm-output。";
        }
        else if (singleHtml)
        {
            lblHint.Text = useCurrent
                ? "把当前文档导出为单个 docs.html，本地图片自动嵌入为 Base64。"
                : "把目录下全部 .md 合并为一个 docs.html，本地图片自动嵌入为 Base64。";
        }
        else if (useCurrent)
        {
            lblHint.Text = "把当前打开的文档导出为本地 HTML 站点并可用浏览器访问。";
        }
        else
        {
            lblHint.Text = "按目录结构把 Markdown 导出为本地 HTML 站点并可用浏览器访问。";
        }
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择包含 Markdown 文档的目录",
            UseDescriptionForTitle = true
        };

        var current = txtDirectory.Text.Trim();
        if (Directory.Exists(current))
        {
            dialog.SelectedPath = current;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtDirectory.Text = dialog.SelectedPath;
            radioFolder.Checked = true;
        }
    }

    private void btnOpenWiki_Click(object sender, EventArgs e) => MarkdownWikiLauncher.OpenBrowser();

    private void btnPrimary_Click(object sender, EventArgs e)
    {
        try
        {
            if (radioWiki.Checked)
            {
                StartMarkdownWiki();
                return;
            }

            if (radioChm.Checked)
            {
                ExportChm();
                return;
            }

            if (radioSingleHtml.Checked)
            {
                ExportSingleHtml();
                return;
            }

            ExportSite();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void ExportSite()
    {
        Cursor = Cursors.WaitCursor;
        string url;
        string detail;

        if (radioCurrent.Checked && radioCurrent.Enabled)
        {
            var doc = _currentDocumentProvider();
            if (doc is null)
            {
                MessageBox.Show("请先在主界面打开一个 Markdown 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var outputRoot = Path.Combine(AppContext.BaseDirectory, "site-output");
            var indexPath = _siteExportService.ExportSingleDocument(doc.Value.filePath, doc.Value.markdownText, outputRoot);
            url = _localWebHostService.Start(outputRoot, 5088);
            detail = $"索引页：{indexPath}\n访问地址：{url}";
        }
        else
        {
            if (!TryGetDirectory(out var sourceRoot))
            {
                return;
            }

            var outputRoot = Path.Combine(AppContext.BaseDirectory, "site-output-directory");
            var result = _directorySiteExportService.ExportDirectory(sourceRoot, outputRoot);
            url = _localWebHostService.Start(outputRoot, 5088);
            detail = $"索引页：{result.indexPath}\n文档数量：{result.count}\n访问地址：{url}";
        }

        if (chkOpenAfter.Checked)
        {
            OpenBrowser(url);
        }

        MessageBox.Show(detail, "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportSingleHtml()
    {
        Cursor = Cursors.WaitCursor;
        string outputPath;
        int count;
        int images;

        if (radioCurrent.Checked && radioCurrent.Enabled)
        {
            var doc = _currentDocumentProvider();
            if (doc is null)
            {
                MessageBox.Show("请先在主界面打开一个 Markdown 文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var outDir = Path.Combine(AppContext.BaseDirectory, "site-output");
            Directory.CreateDirectory(outDir);
            (outputPath, count, images) = _singleHtmlExportService.ExportSingleDocument(
                doc.Value.filePath,
                doc.Value.markdownText,
                Path.Combine(outDir, "docs.html"));
        }
        else
        {
            if (!TryGetDirectory(out var sourceRoot))
            {
                return;
            }

            (outputPath, count, images) = _singleHtmlExportService.ExportDirectory(
                sourceRoot,
                Path.Combine(sourceRoot, "docs.html"));
        }

        var detail = $"输出文件：{outputPath}\n文档数量：{count}\n嵌入图片：{images}";
        if (chkOpenAfter.Checked)
        {
            OpenBrowser(outputPath);
        }

        MessageBox.Show(detail, "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ExportChm()
    {
        if (!TryGetDirectory(out var root, allowEmpty: false))
        {
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

        if (result.Ok && chkOpenAfter.Checked)
        {
            OpenChmExportResult(result);
        }
    }

    private void StartMarkdownWiki()
    {
        var content = txtDirectory.Text.Trim();
        if (!string.IsNullOrWhiteSpace(content) && !Directory.Exists(content))
        {
            MessageBox.Show("请先选择有效的文档目录，或留空使用默认示例。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        MarkdownWikiLauncher.Start(string.IsNullOrWhiteSpace(content) ? null : content);
        Thread.Sleep(1200);
        MarkdownWikiLauncher.OpenBrowser();

        MessageBox.Show(
            "已尝试启动 MarkdownWiki。\n访问地址：http://127.0.0.1:5099/\n若首次启动请稍等编译完成后再刷新。",
            "提示",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private bool TryGetDirectory(out string directory, bool allowEmpty = false)
    {
        directory = txtDirectory.Text.Trim();
        if (allowEmpty && string.IsNullOrWhiteSpace(directory))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            MessageBox.Show("请先选择有效的文档目录。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        return true;
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

    private static void OpenBrowser(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
