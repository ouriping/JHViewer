using System.Text;
using MarkdownWiki.Core.Models;

namespace MarkdownWiki.Core.Services;

public sealed class ChmExportService
{
    private static readonly string ChmCss = """
        body { font-family: "Microsoft YaHei", "Segoe UI", sans-serif; margin: 16px; line-height: 1.6; color: #1f2933; }
        h1, h2, h3, h4 { line-height: 1.3; }
        pre { background: #f4f6f8; padding: 10px; border: 1px solid #d9e2ec; overflow: auto; }
        code { font-family: Consolas, "Courier New", monospace; background: #eef2f6; padding: 1px 4px; }
        pre code { background: transparent; padding: 0; }
        table { border-collapse: collapse; }
        th, td { border: 1px solid #d9e2ec; padding: 6px 10px; }
        a { color: #0b6e4f; }
        img { max-width: 100%; }
        blockquote { margin: 0; padding: 4px 12px; border-left: 4px solid #0b6e4f; color: #62748a; background: #e3f5ee; }
        """;

    private readonly WikiPathGuard _pathGuard;
    private readonly WikiOptions _wikiOptions;
    private readonly WikiTreeService _treeService;
    private readonly WikiDocumentService _docs;
    private readonly ChmHtmlBuilder _htmlBuilder;
    private readonly ChmProjectWriter _projectWriter;
    private readonly ChmCompiler _compiler;

    public ChmExportService(
        WikiPathGuard pathGuard,
        WikiOptions wikiOptions,
        WikiTreeService treeService,
        WikiDocumentService docs,
        ChmHtmlBuilder htmlBuilder,
        ChmProjectWriter projectWriter,
        ChmCompiler compiler)
    {
        _pathGuard = pathGuard;
        _wikiOptions = wikiOptions;
        _treeService = treeService;
        _docs = docs;
        _htmlBuilder = htmlBuilder;
        _projectWriter = projectWriter;
        _compiler = compiler;
    }

    public ChmCompiler.DetectedCompiler? DetectCompiler(ChmExportOptions? options = null)
    {
        options ??= CreateDefaultOptions();
        return _compiler.Detect(options.CompilerPath, options.PreferredCompiler);
    }

    public ChmExportOptions CreateDefaultOptions()
    {
        var chm = _wikiOptions.Chm ?? new WikiChmOptions();
        var output = Path.IsPathRooted(chm.OutputDirectory)
            ? chm.OutputDirectory
            : Path.Combine(_pathGuard.ContentRoot, "..", chm.OutputDirectory);

        return new ChmExportOptions
        {
            Title = _wikiOptions.SiteTitle,
            HomeDocument = _wikiOptions.HomeDocument,
            OutputDirectory = Path.GetFullPath(output),
            FileName = string.IsNullOrWhiteSpace(chm.FileName) ? "JinghuaHelp.chm" : chm.FileName,
            CompilerPath = string.IsNullOrWhiteSpace(chm.CompilerPath) ? null : chm.CompilerPath,
            PreferredCompiler = string.IsNullOrWhiteSpace(chm.PreferredCompiler) ? "Auto" : chm.PreferredCompiler,
            IncludeIndex = chm.IncludeIndex,
            FullTextSearch = chm.FullTextSearch,
            Compile = true
        };
    }

    public ChmExportResult Export(ChmExportOptions? options = null)
    {
        options ??= CreateDefaultOptions();
        var logs = new List<string>();
        var buildRoot = Path.Combine(options.OutputDirectory, "build");
        var htmlRoot = Path.Combine(buildRoot, "html");
        var cssDir = Path.Combine(buildRoot, "css");

        try
        {
            // 资源管理器若正打开旧 build，Directory.Delete 会失败；先安全清理，失败则换时间戳目录
            buildRoot = PrepareBuildRoot(options.OutputDirectory, logs);
            htmlRoot = Path.Combine(buildRoot, "html");
            cssDir = Path.Combine(buildRoot, "css");

            Directory.CreateDirectory(htmlRoot);
            Directory.CreateDirectory(cssDir);
            ChmTextEncoding.WriteAllText(Path.Combine(cssDir, "chm.css"), ChmCss, ChmTextEncoding.TopicHtml);

            var tree = _treeService.GetTree();
            var files = new List<string> { @"css\chm.css" };
            var indexEntries = new List<(string name, string local)>();

            foreach (var mdFull in Directory.EnumerateFiles(_pathGuard.ContentRoot, "*.md", SearchOption.AllDirectories))
            {
                var relative = _pathGuard.ToRelativePosix(mdFull);
                if (relative.StartsWith("_meta/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var doc = _docs.Get(relative);
                var htmRelative = ChmHtmlBuilder.ToHtmRelativeFromRoot(relative); // html/xxx.htm
                var htmFull = Path.Combine(buildRoot, htmRelative.Replace('/', Path.DirectorySeparatorChar));
                var htmDir = Path.GetDirectoryName(htmFull);
                if (!string.IsNullOrEmpty(htmDir))
                {
                    Directory.CreateDirectory(htmDir);
                }

                var page = _htmlBuilder.BuildPage(doc.Markdown, relative, doc.Title, options.Title);
                // 主题页用带 BOM 的 UTF-8 + charset=utf-8，正文中文可正常显示
                ChmTextEncoding.WriteAllText(htmFull, page, ChmTextEncoding.TopicHtml);
                files.Add(htmRelative.Replace('/', '\\'));
                indexEntries.Add((doc.Title, htmRelative.Replace('/', '\\')));

                if (options.IncludeIndex)
                {
                    foreach (var (text, level) in ChmHtmlBuilder.ExtractHeadings(doc.Markdown))
                    {
                        if (level >= 2)
                        {
                            indexEntries.Add(($"{doc.Title} / {text}", htmRelative.Replace('/', '\\')));
                        }
                    }
                }

                // 复制同目录 assets
                var srcDir = Path.GetDirectoryName(mdFull);
                if (!string.IsNullOrEmpty(srcDir))
                {
                    var assetsSrc = Path.Combine(srcDir, "assets");
                    if (Directory.Exists(assetsSrc))
                    {
                        var assetsDest = Path.Combine(Path.GetDirectoryName(htmFull)!, "assets");
                        CopyDirectory(assetsSrc, assetsDest);
                        foreach (var asset in Directory.EnumerateFiles(assetsDest, "*", SearchOption.AllDirectories))
                        {
                            var rel = Path.GetRelativePath(buildRoot, asset).Replace('/', '\\');
                            files.Add(rel);
                        }
                    }
                }

                logs.Add($"已生成：{htmRelative}");
            }

            var defaultTopic = ChmHtmlBuilder.ToHtmRelativeFromRoot(options.HomeDocument).Replace('/', '\\');
            if (!files.Any(f => f.Equals(defaultTopic, StringComparison.OrdinalIgnoreCase)))
            {
                // 首页缺失时取第一篇
                var first = files.FirstOrDefault(f => f.StartsWith("html\\", StringComparison.OrdinalIgnoreCase));
                if (first is not null)
                {
                    defaultTopic = first;
                    logs.Add($"首页不存在，改用：{defaultTopic}");
                }
            }

            var hhcPath = Path.Combine(buildRoot, "TableOfContents.hhc");
            _projectWriter.WriteContents(hhcPath, tree, p => ChmHtmlBuilder.ToHtmRelativeFromRoot(p));

            var hhkPath = Path.Combine(buildRoot, "Index.hhk");
            if (options.IncludeIndex)
            {
                _projectWriter.WriteIndex(hhkPath, indexEntries);
            }

            var hhpPath = Path.Combine(buildRoot, "Project.hhp");
            _projectWriter.WriteProject(hhpPath, options, defaultTopic, files, options.IncludeIndex);
            logs.Add($"已生成工程：{hhpPath}");

            Directory.CreateDirectory(options.OutputDirectory);
            var chmName = options.FileName.EndsWith(".chm", StringComparison.OrdinalIgnoreCase)
                ? options.FileName
                : options.FileName + ".chm";
            var chmInBuild = Path.Combine(buildRoot, chmName);
            var chmFinal = Path.Combine(options.OutputDirectory, chmName);

            string? compilerUsed = null;
            var compiled = false;
            string message;

            if (options.Compile)
            {
                var detected = _compiler.Detect(options.CompilerPath, options.PreferredCompiler);
                if (detected is null)
                {
                    message = "中间工程已生成，但未检测到 hhc.exe / chmcmd。可用 HTML Help Workshop 打开 Project.hhp 手工编译。";
                    logs.Add(message);
                }
                else
                {
                    compilerUsed = $"{detected.Kind}: {detected.Path}";
                    var result = _compiler.Compile(detected, hhpPath, chmInBuild);
                    logs.AddRange(result.logs);
                    compiled = result.compiled;
                    message = result.message;
                    if (compiled && File.Exists(chmInBuild))
                    {
                        File.Copy(chmInBuild, chmFinal, overwrite: true);
                        logs.Add($"已复制到：{chmFinal}");
                    }
                }
            }
            else
            {
                message = "已生成中间工程（未请求编译）。";
            }

            return new ChmExportResult
            {
                Ok = true,
                ChmPath = compiled && File.Exists(chmFinal) ? chmFinal : null,
                BuildDirectory = buildRoot,
                ProjectPath = hhpPath,
                CompilerUsed = compilerUsed,
                Compiled = compiled,
                Message = message,
                Logs = logs
            };
        }
        catch (Exception ex)
        {
            logs.Add(ex.ToString());
            return new ChmExportResult
            {
                Ok = false,
                BuildDirectory = buildRoot,
                Message = "导出失败：" + ex.Message,
                Logs = logs
            };
        }
    }

    /// <summary>
    /// 准备空的 build 目录。若目录被资源管理器等占用，先改名再删；仍失败则使用 build-时间戳。
    /// </summary>
    private static string PrepareBuildRoot(string outputDirectory, List<string> logs)
    {
        Directory.CreateDirectory(outputDirectory);
        var preferred = Path.Combine(outputDirectory, "build");

        if (TryClearDirectory(preferred, logs))
        {
            Directory.CreateDirectory(preferred);
            return preferred;
        }

        var fallback = Path.Combine(outputDirectory, "build-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        logs.Add($"原 build 目录被占用，改用：{fallback}");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private static bool TryClearDirectory(string path, List<string> logs)
    {
        if (!Directory.Exists(path))
        {
            return true;
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return true;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(200);
            }
            catch (UnauthorizedAccessException) when (attempt < 2)
            {
                Thread.Sleep(200);
            }
            catch (Exception)
            {
                break;
            }
        }

        // 改名可绕过「当前目录被占用」；旧目录稍后再删或留待手动清理
        try
        {
            var trash = path + ".old-" + Guid.NewGuid().ToString("N")[..8];
            Directory.Move(path, trash);
            logs.Add($"已将占用中的目录移走：{trash}");
            try
            {
                Directory.Delete(trash, recursive: true);
            }
            catch
            {
                logs.Add($"残留目录可稍后手动删除：{trash}");
            }

            return true;
        }
        catch (Exception ex)
        {
            logs.Add($"无法清理 build 目录：{ex.Message}");
            return false;
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(source, file);
            var dest = Path.Combine(destination, rel);
            var destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(file, dest, overwrite: true);
        }
    }
}
