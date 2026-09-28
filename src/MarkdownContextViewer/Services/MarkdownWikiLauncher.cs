using System.Diagnostics;

namespace MarkdownContextViewer;

/// <summary>启动独立的 MarkdownWiki Web 帮助站点。</summary>
public static class MarkdownWikiLauncher
{
    public static string? FindProjectPath()
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MarkdownWiki", "MarkdownWiki.csproj")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "MarkdownWiki", "MarkdownWiki.csproj")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "src", "MarkdownWiki", "MarkdownWiki.csproj")),
            Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "MarkdownWiki", "MarkdownWiki.csproj")),
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    public static Process Start(string? contentRoot = null, int port = 5099)
    {
        var project = FindProjectPath()
            ?? throw new FileNotFoundException("未找到 MarkdownWiki 项目。请先在解决方案中构建 src/MarkdownWiki。");

        var args = $"run --project \"{project}\" -- --port {port}";
        if (!string.IsNullOrWhiteSpace(contentRoot))
        {
            args += $" --content \"{contentRoot}\"";
        }

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = args,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(project) ?? Environment.CurrentDirectory
        };

        return Process.Start(psi) ?? throw new InvalidOperationException("无法启动 MarkdownWiki。");
    }

    public static void OpenBrowser(int port = 5099)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = $"http://127.0.0.1:{port}/",
            UseShellExecute = true
        });
    }
}
