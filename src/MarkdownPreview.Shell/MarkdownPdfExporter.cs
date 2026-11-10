using System.Diagnostics;

namespace MarkdownPreview.Shell;

/// <summary>用本机 Edge 或 Chrome 的无头打印，把 Markdown 导出为 PDF。</summary>
public sealed class MarkdownPdfExporter
{
    public void Export(string markdown, string? baseDirectory, string outputPath)
    {
        var browser = FindBrowser()
            ?? throw new InvalidOperationException("未找到 Microsoft Edge 或 Chrome，无法导出 PDF。");

        var outputFull = Path.GetFullPath(outputPath);
        var outputDir = Path.GetDirectoryName(outputFull);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        if (File.Exists(outputFull))
        {
            File.Delete(outputFull);
        }

        var work = Path.Combine(Path.GetTempPath(), "JHViewerPdf-" + Guid.NewGuid().ToString("N"));
        var profile = Path.Combine(Path.GetTempPath(), "JHViewerEdge-" + Guid.NewGuid().ToString("N"));
        try
        {
            var html = new MarkdownRenderService().RenderHtml(markdown);
            var htmlPath = MarkdownHtmlPackager.Write(html, baseDirectory, work);
            Directory.CreateDirectory(profile);

            var startInfo = new ProcessStartInfo
            {
                FileName = browser,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            startInfo.ArgumentList.Add("--headless=new");
            startInfo.ArgumentList.Add("--disable-gpu");
            startInfo.ArgumentList.Add("--no-first-run");
            startInfo.ArgumentList.Add("--disable-extensions");
            startInfo.ArgumentList.Add("--no-pdf-header-footer");
            startInfo.ArgumentList.Add("--user-data-dir=" + profile);
            startInfo.ArgumentList.Add("--print-to-pdf=" + outputFull);
            startInfo.ArgumentList.Add(htmlPath);

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("无法启动浏览器导出 PDF。");
            if (!process.WaitForExit(120_000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // 超时后尽力结束进程。
                }

                throw new TimeoutException("导出 PDF 超时。");
            }

            if (!File.Exists(outputFull) || new FileInfo(outputFull).Length == 0)
            {
                throw new InvalidOperationException("浏览器没有生成 PDF 文件。");
            }
        }
        finally
        {
            TryDelete(work);
            TryDelete(profile);
        }
    }

    private static string? FindBrowser()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };
        var relative = new[]
        {
            Path.Combine("Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine("Google", "Chrome", "Application", "chrome.exe")
        };

        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            foreach (var name in relative)
            {
                var full = Path.Combine(root, name);
                if (File.Exists(full))
                {
                    return full;
                }
            }
        }

        return null;
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // 浏览器可能仍短暂占用用户数据目录。
        }
    }
}
