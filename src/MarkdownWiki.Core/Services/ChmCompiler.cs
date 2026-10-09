using System.Diagnostics;
using System.Text;

namespace MarkdownWiki.Core.Services;

public sealed class ChmCompiler
{
    public sealed record DetectedCompiler(string Path, string Kind);

    public DetectedCompiler? Detect(string? preferredPath, string preferredKind = "Auto")
    {
        if (!string.IsNullOrWhiteSpace(preferredPath) && File.Exists(preferredPath))
        {
            var kind = preferredPath.EndsWith("chmcmd.exe", StringComparison.OrdinalIgnoreCase) ? "Chmcmd" : "Hhc";
            return new DetectedCompiler(preferredPath, kind);
        }

        var candidates = new List<(string path, string kind)>();

        void AddIfExists(string? path, string kind)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                candidates.Add((path!, kind));
            }
        }

        // 优先：应用自带 tools/hhc/hhc.exe（随主程序部署）
        foreach (var bundled in EnumerateBundledHhcPaths())
        {
            AddIfExists(bundled, "Hhc");
        }

        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        AddIfExists(Path.Combine(pf86, "HTML Help Workshop", "hhc.exe"), "Hhc");
        AddIfExists(Path.Combine(pf, "HTML Help Workshop", "hhc.exe"), "Hhc");

        foreach (var name in new[] { "hhc.exe", "chmcmd.exe" })
        {
            var found = FindOnPath(name);
            if (found is not null)
            {
                candidates.Add((found, name.StartsWith("chm", StringComparison.OrdinalIgnoreCase) ? "Chmcmd" : "Hhc"));
            }
        }

        preferredKind = preferredKind?.Trim() ?? "Auto";
        if (preferredKind.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (preferredKind.Equals("Hhc", StringComparison.OrdinalIgnoreCase))
        {
            return candidates.Where(c => c.kind == "Hhc").Select(c => new DetectedCompiler(c.path, c.kind)).FirstOrDefault();
        }

        if (preferredKind.Equals("Chmcmd", StringComparison.OrdinalIgnoreCase))
        {
            return candidates.Where(c => c.kind == "Chmcmd").Select(c => new DetectedCompiler(c.path, c.kind)).FirstOrDefault();
        }

        return candidates.Select(c => new DetectedCompiler(c.path, c.kind)).FirstOrDefault();
    }

    public (bool compiled, string message, IReadOnlyList<string> logs) Compile(
        DetectedCompiler compiler,
        string hhpPath,
        string expectedChmPath)
    {
        var logs = new List<string>();
        try
        {
            if (File.Exists(expectedChmPath))
            {
                File.Delete(expectedChmPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = compiler.Path,
                Arguments = $"\"{hhpPath}\"",
                WorkingDirectory = Path.GetDirectoryName(hhpPath) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.Default,
                StandardErrorEncoding = Encoding.Default
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return (false, "无法启动编译器进程。", logs);
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit(120_000);

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                logs.AddRange(stdout.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0));
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                logs.AddRange(stderr.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0));
            }

            logs.Add($"编译器退出码：{process.ExitCode}（hhc 成功时常见为 1）");

            // 以产物为准，不依赖 exit code
            if (File.Exists(expectedChmPath) && new FileInfo(expectedChmPath).Length > 0)
            {
                return (true, $"已生成 CHM：{expectedChmPath}", logs);
            }

            return (false, "编译器已运行，但未找到有效的 .chm 产物。", logs);
        }
        catch (Exception ex)
        {
            logs.Add(ex.Message);
            return (false, "调用编译器失败：" + ex.Message, logs);
        }
    }

    /// <summary>应用目录内打包的 hhc.exe（tools/hhc/hhc.exe 或旁路 hhc.exe）。</summary>
    public static string? FindBundledHhc()
    {
        foreach (var path in EnumerateBundledHhcPaths())
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateBundledHhcPaths()
    {
        var bases = new List<string>();
        void AddBase(string? dir)
        {
            if (!string.IsNullOrWhiteSpace(dir) && !bases.Contains(dir, StringComparer.OrdinalIgnoreCase))
            {
                bases.Add(dir);
            }
        }

        AddBase(AppContext.BaseDirectory);
        try
        {
            AddBase(Path.GetDirectoryName(typeof(ChmCompiler).Assembly.Location));
        }
        catch
        {
            // ignore
        }

        foreach (var root in bases)
        {
            yield return Path.Combine(root, "tools", "hhc", "hhc.exe");
            yield return Path.Combine(root, "hhc.exe");
        }
    }

    private static string? FindOnPath(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var full = Path.Combine(dir.Trim('"'), fileName);
                if (File.Exists(full))
                {
                    return full;
                }
            }
            catch
            {
                // ignore bad PATH entries
            }
        }

        return null;
    }
}
