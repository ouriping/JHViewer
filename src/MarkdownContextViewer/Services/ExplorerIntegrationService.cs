using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MarkdownContextViewer;

/// <summary>
/// 资源管理器集成：右键菜单 + SharpShell 预览处理程序部署/注册。
/// </summary>
internal sealed class ExplorerIntegrationService
{
    private const string ContextMenuKey = @"Software\Classes\SystemFileAssociations\.md\shell\MarkdownContextViewer";
    private const string MdExtensionKey = @"Software\Classes\.md";
    private const string MdSystemAssociationKey = @"Software\Classes\SystemFileAssociations\.md";
    private const string PreviewHandlersKey = @"Software\Microsoft\Windows\CurrentVersion\PreviewHandlers";

    private static readonly string[] PreviewFileExtensions =
    {
        ".md", ".markdown", ".json", ".xml"
    };
    private const string IeFeatureEmulationKey =
        @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

    /// <summary>与 MarkdownPreview.Handler.HandlerIds.Clsid 保持一致。</summary>
    private const string PreviewClsid = "{9F2E4B6A-1C8D-4F3E-A7B9-0D5E6F7A8B9C}";
    private const string PreviewHandlerShellEx = "{8895b1c6-b41f-4c1c-a562-0d564250836f}";
    private const string PreviewDisplayName = "Markdown Context Viewer Preview Handler";

    private const int SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);

    public static string GetDeployDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarkdownContextViewer",
            "PreviewHandler");
    }

    public static string GetDeployedHandlerDllPath() =>
        Path.Combine(GetDeployDirectory(), "MarkdownPreview.Handler.dll");

    public static string GetRegisterExePath()
    {
        // 优先部署目录，其次主程序旁边的 Register 输出
        var deployed = Path.Combine(GetDeployDirectory(), "MarkdownPreview.Register.exe");
        if (File.Exists(deployed))
        {
            return deployed;
        }

        return Path.Combine(AppContext.BaseDirectory, "MarkdownPreview.Register.exe");
    }

    public bool IsPreviewEnabled()
    {
        using var previewKey = Registry.ClassesRoot.OpenSubKey(
            $@".md\shellex\{PreviewHandlerShellEx}");
        var value = previewKey?.GetValue("") as string;
        if (!string.Equals(value, PreviewClsid, StringComparison.OrdinalIgnoreCase))
        {
            // HKCR 合并视图可能来自 HKCU
            using var hkcu = Registry.CurrentUser.OpenSubKey(
                $@"Software\Classes\.md\shellex\{PreviewHandlerShellEx}");
            value = hkcu?.GetValue("") as string;
            if (!string.Equals(value, PreviewClsid, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return File.Exists(GetDeployedHandlerDllPath());
    }

    public bool IsMediumIntegrityOptOutEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey($@"Software\Classes\CLSID\{PreviewClsid}");
            return Equals(key?.GetValue("DisableLowILProcessIsolation"), 1);
        }
        catch
        {
            return false;
        }
    }

    public string DiagnosePreview()
    {
        var lines = new List<string>();
        lines.Add("方案: SharpShell + Markdig + WebView2（.NET Framework 4.8）");
        lines.Add("说明: 资源管理器通过 COM/prevhost 加载预览 DLL，不启动主程序。");
        lines.Add("");
        lines.Add($"部署目录: {GetDeployDirectory()}");
        lines.Add($"部署 Handler DLL: {GetDeployedHandlerDllPath()}");
        lines.Add($"部署存在: {File.Exists(GetDeployedHandlerDllPath())}");
        lines.Add($"Register 工具: {GetRegisterExePath()}");
        lines.Add($"Register 存在: {File.Exists(GetRegisterExePath())}");
        lines.Add($"预览已关联: {IsPreviewEnabled()}");
        lines.Add($"HKCU DisableLowIL: {ReadDisableLowIl(Registry.CurrentUser)}");
        lines.Add($"HKLM DisableLowIL: {ReadDisableLowIl(Registry.LocalMachine)}");

        using (var inproc = Registry.CurrentUser.OpenSubKey(
                   $@"Software\Classes\CLSID\{PreviewClsid}\InprocServer32"))
        {
            lines.Add($"InprocServer32: {inproc?.GetValue("") ?? "(无)"}");
            lines.Add($"CodeBase: {inproc?.GetValue("CodeBase") ?? "(无)"}");
            lines.Add($"Assembly: {inproc?.GetValue("Assembly") ?? "(无)"}");
            lines.Add($"Class: {inproc?.GetValue("Class") ?? "(无)"}");
            if (inproc?.GetValue("CodeBase") is null)
            {
                lines.Add("错误: 缺少 CodeBase —— COM 无法加载预览 DLL。请重新勾选「资源管理器预览」。");
            }
        }

        using (var appId = Registry.CurrentUser.OpenSubKey($@"Software\Classes\AppID\{PreviewClsid}"))
        {
            lines.Add($"DllSurrogate: {appId?.GetValue("DllSurrogate") ?? "(无)"}");
        }

        var prevHosts = Process.GetProcessesByName("prevhost");
        lines.Add($"prevhost 进程数: {prevHosts.Length}");
        foreach (var p in prevHosts)
        {
            lines.Add($"  - PID {p.Id} 完整性={TryGetIntegrityLabel(p.Id)} 已加载组件={TryHasMarkdownModule(p)}");
            p.Dispose();
        }

        foreach (var logPath in GetLogCandidatePaths())
        {
            lines.Add($"诊断日志: {logPath}");
            if (!File.Exists(logPath))
            {
                lines.Add("  (文件不存在)");
                continue;
            }

            try
            {
                lines.Add("最近日志:");
                lines.AddRange(File.ReadLines(logPath).TakeLast(8).Select(x => "  " + x));
            }
            catch
            {
                lines.Add("  (读取失败)");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    public void EnablePreview() =>
        EnablePreview(PreviewFileExtensions, progress: null);

    public void EnablePreview(
        IReadOnlyList<string> extensions,
        IProgress<PreviewRegistrationProgress>? progress = null)
    {
        var selected = NormalizeExtensions(extensions);
        if (selected.Count == 0)
        {
            throw new InvalidOperationException("请至少选择一种要注册的文件格式。");
        }

        Report(progress, 5, "清理旧版 COM 注册…");
        UnregisterLegacyNetComHost();

        Report(progress, 18, "部署预览组件文件到本地目录…");
        DeployPreviewHandlerFiles();
        Report(progress, 28, $"组件已部署：{GetDeployDirectory()}");

        var installArgs = "install " + string.Join(' ', selected);
        Report(progress, 35, $"运行注册脚本（无提权）：MarkdownPreview.Register.exe {installArgs}");
        try
        {
            var output = RunRegisterTool(installArgs, elevate: false);
            AppendScriptOutput(progress, output);
            Report(progress, 55, "COM 注册完成（HKCU）。");
        }
        catch (Exception ex)
        {
            Report(progress, 40, "无提权注册失败，尝试 UAC 提权安装…");
            try
            {
                var output = RunRegisterTool(installArgs, elevate: true);
                AppendScriptOutput(progress, output);
                Report(progress, 55, "COM 注册完成（已提权）。");
            }
            catch
            {
                throw new InvalidOperationException(
                    "SharpShell 注册失败。请同意 UAC 或检查 MarkdownPreview.Register.exe。\n" + ex.Message);
            }
        }

        Report(progress, 62, "配置 HKLM DisableLowIL（中等完整性 prevhost）…");
        EnsureHklmDisableLowIl();

        Report(progress, 70, "配置浏览器模拟（FEATURE_BROWSER_EMULATION）…");
        ConfigureBrowserEmulation();

        Report(progress, 78, "按勾选写入扩展名预览关联：" + string.Join(" ", selected));
        foreach (var ext in PreviewFileExtensions)
        {
            ClearExtensionPreview($@"Software\Classes\{ext}");
            ClearExtensionPreview($@"Software\Classes\SystemFileAssociations\{ext}");
        }

        foreach (var ext in selected)
        {
            EnsureExtensionPreview($@"Software\Classes\{ext}");
            EnsureExtensionPreview($@"Software\Classes\SystemFileAssociations\{ext}");
            Report(progress, 85, $"已关联 {ext}");
        }

        Report(progress, 92, "通知 Shell 关联已变更…");
        NotifyShellAssociationsChanged();

        Report(progress, 96, "结束 prevhost 进程…");
        KillPrevhost();

        Report(progress, 100, "预览窗格启用完成。");
    }

    public void DisablePreview() =>
        DisablePreview(progress: null);

    public void DisablePreview(IProgress<PreviewRegistrationProgress>? progress = null)
    {
        Report(progress, 10, "运行卸载脚本：MarkdownPreview.Register.exe uninstall");
        Exception? last = null;
        foreach (var elevate in new[] { false, true })
        {
            try
            {
                if (File.Exists(GetRegisterExePath()))
                {
                    var output = RunRegisterTool("uninstall", elevate);
                    AppendScriptOutput(progress, output);
                    last = null;
                    Report(progress, 45, elevate ? "卸载脚本完成（已提权）。" : "卸载脚本完成。");
                    break;
                }
            }
            catch (Exception ex)
            {
                last = ex;
                Report(progress, 30, (elevate ? "提权" : "无提权") + "卸载失败：" + ex.Message);
            }
        }

        _ = last; // 即使 uninstall 失败也继续清注册表
        Report(progress, 55, "清除扩展名预览关联…");
        foreach (var ext in PreviewFileExtensions)
        {
            ClearExtensionPreview($@"Software\Classes\{ext}");
            ClearExtensionPreview($@"Software\Classes\SystemFileAssociations\{ext}");
            Report(progress, 70, $"已清除 {ext}");
        }

        Report(progress, 80, "清理旧版 COM / DisableLowIL / 浏览器模拟…");
        UnregisterLegacyNetComHost();
        ClearHklmDisableLowIl();
        ClearBrowserEmulation();

        Report(progress, 90, "通知 Shell 并结束 prevhost…");
        NotifyShellAssociationsChanged();
        KillPrevhost();

        Report(progress, 100, "预览窗格已取消。");
    }

    private static List<string> NormalizeExtensions(IEnumerable<string>? extensions)
    {
        var allowed = new HashSet<string>(PreviewFileExtensions, StringComparer.OrdinalIgnoreCase);
        return (extensions ?? [])
            .Select(x => (x ?? string.Empty).Trim().ToLowerInvariant())
            .Select(x => x.StartsWith('.') ? x : "." + x)
            .Where(x => allowed.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void Report(IProgress<PreviewRegistrationProgress>? progress, int percent, string message)
    {
        progress?.Report(new PreviewRegistrationProgress(percent, message));
    }

    private static void AppendScriptOutput(IProgress<PreviewRegistrationProgress>? progress, string? output)
    {
        if (string.IsNullOrWhiteSpace(output) || progress is null)
        {
            return;
        }

        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            progress.Report(new PreviewRegistrationProgress(-1, "  » " + line.Trim()));
        }
    }

    public int KillPrevhost()
    {
        var killed = 0;
        foreach (var process in Process.GetProcessesByName("prevhost"))
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(3000);
                killed++;
            }
            catch
            {
                // ignore
            }
            finally
            {
                process.Dispose();
            }
        }

        return killed;
    }

    public void RestartExplorer()
    {
        KillPrevhost();
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            catch
            {
                // ignore
            }
            finally
            {
                process.Dispose();
            }
        }

        Thread.Sleep(800);
        if (Process.GetProcessesByName("explorer").Length == 0)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                UseShellExecute = true
            });
        }
    }

    public void RegisterContextMenu(string exePath)
    {
        using var shellKey = Registry.CurrentUser.CreateSubKey(ContextMenuKey);
        shellKey?.SetValue("", "用 Markdown 浏览器打开");
        shellKey?.SetValue("Icon", exePath);

        using var commandKey = Registry.CurrentUser.CreateSubKey($"{ContextMenuKey}\\command");
        commandKey?.SetValue("", $"\"{exePath}\" \"%1\"");

        // 供预览 Handler 在未装右键菜单时也能定位主程序
        using var appKey = Registry.CurrentUser.CreateSubKey(@"Software\MarkdownContextViewer");
        appKey?.SetValue("EditorExe", exePath);
    }

    public void UnregisterContextMenu()
    {
        Registry.CurrentUser.DeleteSubKeyTree(ContextMenuKey, throwOnMissingSubKey: false);
    }

    public string DeployPreviewHandlerFiles()
    {
        var sourceDir = FindHandlerBuildOutput();
        if (sourceDir is null)
        {
            throw new FileNotFoundException(
                "未找到 MarkdownPreview.Handler 编译输出，请先生成解决方案。");
        }

        var deployDir = GetDeployDirectory();
        Directory.CreateDirectory(deployDir);

        // 清理旧残留（net8 comhost / 误拷的主程序）
        foreach (var pattern in new[]
                 {
                     "MarkdownPreview.Shell*",
                     "MarkdownContextViewer*",
                     "*.comhost.dll"
                 })
        {
            foreach (var stale in Directory.GetFiles(deployDir, pattern))
            {
                try { File.Delete(stale); } catch { /* locked */ }
            }
        }

        // 只拷贝预览处理程序依赖，避免把主程序 bin 整目录塞进来
        string[] required =
        [
            "MarkdownPreview.Handler.dll",
            "SharpShell.dll",
            "Markdig.dll",
            "Microsoft.Web.WebView2.Core.dll",
            "Microsoft.Web.WebView2.WinForms.dll",
            "System.Buffers.dll",
            "System.Memory.dll",
            "System.Numerics.Vectors.dll",
            "System.Runtime.CompilerServices.Unsafe.dll"
        ];

        foreach (var name in required)
        {
            var src = Path.Combine(sourceDir, name);
            if (!File.Exists(src))
            {
                // 主程序输出目录可能缺部分依赖，回退到 Handler 工程输出
                continue;
            }

            File.Copy(src, Path.Combine(deployDir, name), overwrite: true);
        }

        // 若主目录缺文件，从 Handler\bin\net48 再补一遍
        var handlerProjectOut = FindHandlerProjectOutput();
        if (handlerProjectOut is not null)
        {
            foreach (var name in required)
            {
                var dst = Path.Combine(deployDir, name);
                if (File.Exists(dst))
                {
                    continue;
                }

                var src = Path.Combine(handlerProjectOut, name);
                if (File.Exists(src))
                {
                    File.Copy(src, dst, overwrite: true);
                }
            }

            var runtimes = Path.Combine(handlerProjectOut, "runtimes");
            if (Directory.Exists(runtimes))
            {
                CopyDirectory(runtimes, Path.Combine(deployDir, "runtimes"));
            }
        }

        var sourceRuntimes = Path.Combine(sourceDir, "runtimes");
        if (Directory.Exists(sourceRuntimes))
        {
            CopyDirectory(sourceRuntimes, Path.Combine(deployDir, "runtimes"));
        }

        foreach (var loader in Directory.GetFiles(deployDir, "WebView2Loader.dll", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(deployDir, "WebView2Loader.dll");
            if (!string.Equals(loader, dest, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(loader, dest, overwrite: true);
            }
        }

        var registerSource = FindRegisterBuildOutput();
        if (registerSource is not null)
        {
            foreach (var file in Directory.GetFiles(registerSource, "MarkdownPreview.Register.*"))
            {
                File.Copy(file, Path.Combine(deployDir, Path.GetFileName(file)), overwrite: true);
            }
        }

        foreach (var name in required)
        {
            if (!File.Exists(Path.Combine(deployDir, name)))
            {
                throw new FileNotFoundException($"部署缺少依赖：{name}", Path.Combine(deployDir, name));
            }
        }

        return GetDeployedHandlerDllPath();
    }

    private static string? FindHandlerBuildOutput()
    {
        // 构建输出优先；其次主程序旁已复制的 DLL
        var fromProject = FindSiblingProjectOutput("MarkdownPreview.Handler", "MarkdownPreview.Handler.dll");
        if (fromProject is not null)
        {
            return fromProject;
        }

        var besideApp = Path.Combine(AppContext.BaseDirectory, "MarkdownPreview.Handler.dll");
        return File.Exists(besideApp) ? AppContext.BaseDirectory : null;
    }

    private static string? FindHandlerProjectOutput() =>
        FindSiblingProjectOutput("MarkdownPreview.Handler", "MarkdownPreview.Handler.dll");

    private static string? FindRegisterBuildOutput()
    {
        var fromProject = FindSiblingProjectOutput("MarkdownPreview.Register", "MarkdownPreview.Register.exe");
        if (fromProject is not null)
        {
            return fromProject;
        }

        var besideApp = Path.Combine(AppContext.BaseDirectory, "MarkdownPreview.Register.exe");
        return File.Exists(besideApp) ? AppContext.BaseDirectory : null;
    }

    /// <summary>
    /// 从主程序输出目录向上查找 src/{projectName}/bin/{Debug|Release}/net48/{fileName}。
    /// </summary>
    private static string? FindSiblingProjectOutput(string projectName, string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            foreach (var configuration in new[] { "Debug", "Release" })
            {
                var candidate = Path.Combine(
                    dir.FullName,
                    projectName,
                    "bin",
                    configuration,
                    "net48",
                    fileName);
                if (File.Exists(candidate))
                {
                    return Path.GetDirectoryName(candidate);
                }

                // 兼容已位于 src/ 下的布局
                candidate = Path.Combine(
                    dir.FullName,
                    "src",
                    projectName,
                    "bin",
                    configuration,
                    "net48",
                    fileName);
                if (File.Exists(candidate))
                {
                    return Path.GetDirectoryName(candidate);
                }
            }
        }

        return null;
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var sub in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(sub, Path.Combine(destDir, Path.GetFileName(sub)));
        }
    }

    /// <returns>捕获到的脚本标准输出（提权模式可能为空）。</returns>
    private string RunRegisterTool(string command, bool elevate)
    {
        var exe = GetRegisterExePath();
        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("未找到 MarkdownPreview.Register.exe，请先编译 Register 项目。", exe);
        }

        // 必须从部署目录启动，使 CodeBase/相对依赖指向部署文件
        var workDir = GetDeployDirectory();
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = command,
            WorkingDirectory = workDir,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        if (elevate)
        {
            // runas 必须 UseShellExecute，无法重定向输出
            psi.UseShellExecute = true;
            psi.Verb = "runas";
        }
        else
        {
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
        }

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("无法启动注册工具（可能取消了 UAC）。");

        var stdout = string.Empty;
        var stderr = string.Empty;
        if (!elevate)
        {
            stdout = process.StandardOutput.ReadToEnd();
            stderr = process.StandardError.ReadToEnd();
        }

        if (!process.WaitForExit(60000))
        {
            try { process.Kill(); } catch { /* ignore */ }
            throw new TimeoutException("注册工具超时。");
        }

        if (process.ExitCode != 0)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            throw new InvalidOperationException(
                $"注册工具失败，退出码={process.ExitCode}。" +
                (string.IsNullOrWhiteSpace(detail) ? "请以管理员同意 UAC 后重试。" : "\n" + detail.Trim()));
        }

        return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
    }

    /// <summary>清理旧版 .NET 8 comhost 注册，避免与 SharpShell 冲突。</summary>
    private static void UnregisterLegacyNetComHost()
    {
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{PreviewClsid}", throwOnMissingSubKey: false);
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\AppID\{PreviewClsid}", throwOnMissingSubKey: false);
        using var previewHandlers = Registry.CurrentUser.OpenSubKey(PreviewHandlersKey, writable: true);
        if (previewHandlers?.GetValue(PreviewClsid) is not null)
        {
            previewHandlers.DeleteValue(PreviewClsid, throwOnMissingValue: false);
        }
    }

    private static void EnsureExtensionPreview(string extensionKeyPath)
    {
        using var extensionKey = Registry.CurrentUser.CreateSubKey(extensionKeyPath);
        extensionKey?.SetValue("PerceivedType", "text");

        using var previewKey = Registry.CurrentUser.CreateSubKey(
            $@"{extensionKeyPath}\shellex\{PreviewHandlerShellEx}");
        previewKey?.SetValue("", PreviewClsid);
    }

    private static void ClearExtensionPreview(string extensionKeyPath)
    {
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"{extensionKeyPath}\shellex\{PreviewHandlerShellEx}",
            throwOnMissingSubKey: false);
    }

    private static void ConfigureBrowserEmulation()
    {
        using var key = Registry.CurrentUser.CreateSubKey(IeFeatureEmulationKey);
        key?.SetValue("prevhost.exe", 11001, RegistryValueKind.DWord);
    }

    private static void ClearBrowserEmulation()
    {
        using var key = Registry.CurrentUser.OpenSubKey(IeFeatureEmulationKey, writable: true);
        if (key?.GetValue("prevhost.exe") is not null)
        {
            key.DeleteValue("prevhost.exe", throwOnMissingValue: false);
        }
    }

    private static string ReadDisableLowIl(RegistryKey root)
    {
        try
        {
            using var key = root.OpenSubKey($@"Software\Classes\CLSID\{PreviewClsid}");
            var value = key?.GetValue("DisableLowILProcessIsolation");
            return value is null ? "(未设置)" : Convert.ToString(value) ?? "(未设置)";
        }
        catch
        {
            return "(无法读取)";
        }
    }

    private static void EnsureHklmDisableLowIl()
    {
        if (TryWriteHklmDisableLowIl())
        {
            return;
        }

        try
        {
            var args =
                $"add \"HKLM\\Software\\Classes\\CLSID\\{PreviewClsid}\" " +
                "/v DisableLowILProcessIsolation /t REG_DWORD /d 1 /f";
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = args,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            });
            process?.WaitForExit(15000);
        }
        catch
        {
            // user cancelled
        }
    }

    private static bool TryWriteHklmDisableLowIl()
    {
        try
        {
            using var key = Registry.LocalMachine.CreateSubKey($@"Software\Classes\CLSID\{PreviewClsid}");
            if (key is null)
            {
                return false;
            }

            key.SetValue("DisableLowILProcessIsolation", 1, RegistryValueKind.DWord);
            key.SetValue("", PreviewDisplayName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void ClearHklmDisableLowIl()
    {
        try
        {
            Registry.LocalMachine.DeleteSubKeyTree(
                $@"Software\Classes\CLSID\{PreviewClsid}",
                throwOnMissingSubKey: false);
        }
        catch
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"delete \"HKLM\\Software\\Classes\\CLSID\\{PreviewClsid}\" /f",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                process?.WaitForExit(15000);
            }
            catch
            {
                // ignore
            }
        }
    }

    private static IEnumerable<string> GetLogCandidatePaths()
    {
        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
            "MarkdownContextViewer",
            "preview-handler.log");
        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarkdownContextViewer",
            "preview-handler.log");
    }

    private static string TryGetIntegrityLabel(int pid)
    {
        try { return ProcessIntegrity.GetLabel(pid); }
        catch { return "未知"; }
    }

    private static string TryHasMarkdownModule(Process process)
    {
        try
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Contains("MarkdownPreview", StringComparison.OrdinalIgnoreCase)
                    || module.ModuleName.Contains("WebView2", StringComparison.OrdinalIgnoreCase))
                {
                    return "是";
                }
            }

            return "否";
        }
        catch
        {
            return "无法枚举";
        }
    }

    private static void NotifyShellAssociationsChanged()
    {
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
    }
}
