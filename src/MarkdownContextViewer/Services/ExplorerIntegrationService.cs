using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MarkdownContextViewer;

/// <summary>
/// 资源管理器集成：默认打开关联 + 右键菜单 + SharpShell 预览处理程序部署/注册。
/// </summary>
internal sealed class ExplorerIntegrationService
{
    private const string ProgIdPrefix = "MarkdownContextViewer";
    private const string AppRegistryRoot = @"Software\MarkdownContextViewer";
    private const string PreviousAssociationsKey = AppRegistryRoot + @"\PreviousAssociations";
    private const string CapabilitiesKey = AppRegistryRoot + @"\Capabilities";
    private const string RegisteredApplicationsKey = @"Software\RegisteredApplications";
    private const string ContextMenuVerb = "MarkdownContextViewer";
    private const string PreviewHandlersKey = @"Software\Microsoft\Windows\CurrentVersion\PreviewHandlers";

    /// <summary>双击 /「打开」/ 右键菜单用单文件查看器处理的扩展名。</summary>
    private static readonly string[] OpenFileExtensions =
    {
        ".md", ".markdown", ".json", ".xml"
    };

    private const string ViewerMenuLabel = "用 JHViewer 打开";

    private static readonly string[] PreviewFileExtensions =
    {
        ".md", ".markdown", ".json", ".xml"
    };
    private const string IeFeatureEmulationKey =
        @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

    /// <summary>与 MarkdownPreview.Handler.HandlerIds.Clsid 保持一致。</summary>
    private const string PreviewClsid = "{9F2E4B6A-1C8D-4F3E-A7B9-0D5E6F7A8B9C}";
    private const string PreviewHandlerShellEx = "{8895b1c6-b41f-4c1c-a562-0d564250836f}";
    private const string PreviewDisplayName = "JHViewer Preview Handler";

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

    public static string GetBundledPreviewHandlerDirectory() =>
        Path.Combine(AppContext.BaseDirectory, "preview-handler");

    public static string GetRegisterExePath()
    {
        // 优先已部署目录，其次发布包里的 preview-handler，最后才是主程序旁边的旧布局
        var deployed = Path.Combine(GetDeployDirectory(), "MarkdownPreview.Register.exe");
        if (File.Exists(deployed))
        {
            return deployed;
        }

        var bundled = Path.Combine(GetBundledPreviewHandlerDirectory(), "MarkdownPreview.Register.exe");
        if (File.Exists(bundled))
        {
            return bundled;
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

    public int KillPrevhost() => KillPrevhost(progress: null, CancellationToken.None);

    public int KillPrevhost(IProgress<PreviewRegistrationProgress>? progress, CancellationToken cancellationToken)
    {
        Report(progress, 15, "查找 prevhost.exe…");
        var processes = Process.GetProcessesByName("prevhost");
        if (processes.Length == 0)
        {
            Report(progress, 100, "没有运行中的 prevhost。");
            return 0;
        }

        var killed = 0;
        for (var i = 0; i < processes.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var process = processes[i];
            Report(progress, 20 + (60 * i / processes.Length), $"结束 prevhost PID {process.Id}…");
            try
            {
                process.Kill(entireProcessTree: true);
                if (!process.WaitForExit(1500))
                {
                    Report(progress, -1, $"prevhost PID {process.Id} 未在 1.5s 内退出，跳过等待。");
                }
                else
                {
                    killed++;
                }
            }
            catch (Exception ex)
            {
                Report(progress, -1, $"结束 prevhost PID {process.Id} 失败：{ex.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }

        Report(progress, 100, $"已结束 {killed}/{processes.Length} 个 prevhost。");
        return killed;
    }

    public void RestartExplorer() =>
        RestartExplorer(progress: null, CancellationToken.None, timeoutMs: 8000);

    /// <summary>
    /// 重启资源管理器。不提权；带总超时，避免 Kill/WaitForExit 卡住 UI。
    /// </summary>
    public void RestartExplorer(
        IProgress<PreviewRegistrationProgress>? progress,
        CancellationToken cancellationToken,
        int timeoutMs = 8000)
    {
        if (timeoutMs < 2000)
        {
            timeoutMs = 2000;
        }

        var sw = Stopwatch.StartNew();
        Report(progress, 5, "结束预览宿主 prevhost…");
        KillPrevhost(progress, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        Report(progress, 35, "结束资源管理器 explorer.exe…");
        var explorers = Process.GetProcessesByName("explorer");
        foreach (var process in explorers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remain = timeoutMs - (int)sw.ElapsedMilliseconds;
            if (remain <= 0)
            {
                break;
            }

            try
            {
                Report(progress, 40, $"结束 explorer PID {process.Id}…");
                process.Kill(entireProcessTree: true);
                var wait = Math.Min(1500, remain);
                if (!process.WaitForExit(wait))
                {
                    Report(progress, -1, $"explorer PID {process.Id} 未在 {wait}ms 内退出。");
                }
            }
            catch (Exception ex)
            {
                Report(progress, -1, $"结束 explorer PID {process.Id} 失败：{ex.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }

        // 若仍有残留，用 taskkill 再扫一遍（仍不提权）
        if (Process.GetProcessesByName("explorer").Length > 0
            && sw.ElapsedMilliseconds < timeoutMs
            && !cancellationToken.IsCancellationRequested)
        {
            Report(progress, 55, "尝试 taskkill /IM explorer.exe…");
            TryTaskKill("explorer.exe", Math.Min(3000, timeoutMs - (int)sw.ElapsedMilliseconds), progress);
        }

        Report(progress, 70, "等待 explorer 退出…");
        while (sw.ElapsedMilliseconds < timeoutMs
               && !cancellationToken.IsCancellationRequested
               && Process.GetProcessesByName("explorer").Length > 0)
        {
            Thread.Sleep(200);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (Process.GetProcessesByName("explorer").Length > 0)
        {
            Report(progress, 85, "部分 explorer 仍在运行（可能由系统自动拉起）。");
        }
        else
        {
            Report(progress, 85, "启动 explorer.exe…");
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                UseShellExecute = true
            });
        }

        // 最多再等 2 秒确认任务栏回来，不再长时间阻塞
        var confirmUntil = Math.Min(timeoutMs, (int)sw.ElapsedMilliseconds + 2000);
        while (sw.ElapsedMilliseconds < confirmUntil
               && !cancellationToken.IsCancellationRequested
               && Process.GetProcessesByName("explorer").Length == 0)
        {
            Thread.Sleep(200);
        }

        if (Process.GetProcessesByName("explorer").Length == 0)
        {
            throw new TimeoutException(
                $"重启资源管理器超时（{timeoutMs}ms）。请按 Ctrl+Shift+Esc 打开任务管理器，运行新任务 explorer.exe。");
        }

        Report(progress, 100, "资源管理器已恢复。");
    }

    private static void TryTaskKill(string imageName, int timeoutMs, IProgress<PreviewRegistrationProgress>? progress)
    {
        if (timeoutMs < 500)
        {
            return;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/F /IM {imageName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (process is null)
            {
                return;
            }

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(); } catch { /* ignore */ }
                Report(progress, -1, $"taskkill {imageName} 超时已中止。");
                return;
            }

            var output = (process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd()).Trim();
            if (!string.IsNullOrWhiteSpace(output))
            {
                Report(progress, -1, output.Split('\n')[0].Trim());
            }
        }
        catch (Exception ex)
        {
            Report(progress, -1, "taskkill 失败：" + ex.Message);
        }
    }

    /// <summary>
    /// 已注册过文件关联时，把打开命令升级为 <c>--view</c>，并在 .md 仍由本程序接管时补齐 .markdown。
    /// 不会在用户从未注册时擅自占用默认应用。
    /// </summary>
    public void UpgradeViewerAssociations(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return;
        }

        var openCommand = BuildViewerOpenCommand(exePath);
        var known = OpenFileExtensions.Where(ProgIdCommandExists).ToArray();
        if (known.Length == 0 && !IsDefaultOurs(".md"))
        {
            return;
        }

        var changed = false;
        foreach (var ext in known)
        {
            changed |= UpdateProgIdCommandIfNeeded(ext, exePath, openCommand);
            changed |= UpdateContextMenuCommandIfNeeded(ext, exePath, openCommand);
        }

        var exeName = Path.GetFileName(exePath);
        using (var commandKey = Registry.CurrentUser.OpenSubKey(
                   $@"Software\Classes\Applications\{exeName}\shell\open\command",
                   writable: true))
        {
            if (commandKey?.GetValue("") is string current
                && !string.Equals(current, openCommand, StringComparison.OrdinalIgnoreCase))
            {
                commandKey.SetValue("", openCommand);
                changed = true;
            }
        }

        if (IsDefaultOurs(".md"))
        {
            foreach (var ext in OpenFileExtensions)
            {
                if (!IsDefaultOurs(ext) || !ProgIdCommandExists(ext))
                {
                    var progId = GetProgId(ext);
                    RegisterProgId(progId, GetFriendlyTypeName(ext), exePath, openCommand);
                    AssociateExtension(ext, progId);
                    EnsureOpenWithProgId(ext, progId);
                    changed = true;
                }

                if (!ContextMenuCommandMatches(ext, exePath, openCommand))
                {
                    RegisterContextMenuVerb(ext, exePath, openCommand);
                    changed = true;
                }
            }
        }

        if (!changed)
        {
            return;
        }

        RegisterApplicationOpenWith(exeName, openCommand);
        RegisterApplicationCapabilities(exePath);
        using var appKey = Registry.CurrentUser.CreateSubKey(AppRegistryRoot);
        appKey?.SetValue("EditorExe", exePath);
        NotifyShellAssociationsChanged();
    }

    /// <summary>
    /// 注册 .md / .markdown / .json / .xml 的默认打开（ProgId + shell\open）与右键「用 JHViewer 打开」。
    /// 打开命令带 <c>--view</c>，只启动单文件查看窗口。
    /// </summary>
    public void RegisterContextMenu(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            throw new FileNotFoundException("找不到主程序路径。", exePath);
        }

        var openCommand = BuildViewerOpenCommand(exePath);
        var exeName = Path.GetFileName(exePath);

        foreach (var ext in OpenFileExtensions)
        {
            var progId = GetProgId(ext);
            RegisterProgId(progId, GetFriendlyTypeName(ext), exePath, openCommand);
            AssociateExtension(ext, progId);
            RegisterContextMenuVerb(ext, exePath, openCommand);
            EnsureOpenWithProgId(ext, progId);
        }

        RegisterApplicationOpenWith(exeName, openCommand);
        RegisterApplicationCapabilities(exePath);

        using var appKey = Registry.CurrentUser.CreateSubKey(AppRegistryRoot);
        appKey?.SetValue("EditorExe", exePath);

        NotifyShellAssociationsChanged();
    }

    private static string BuildViewerOpenCommand(string exePath) =>
        $"\"{exePath}\" --view \"%1\"";

    private static bool ProgIdCommandExists(string extension)
    {
        using var command = Registry.CurrentUser.OpenSubKey(
            $@"Software\Classes\{GetProgId(extension)}\shell\open\command");
        return command?.GetValue("") is string cmd && !string.IsNullOrWhiteSpace(cmd);
    }

    private static bool IsDefaultOurs(string extension)
    {
        using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{extension}");
        var current = key?.GetValue("") as string;
        return string.Equals(current, GetProgId(extension), StringComparison.OrdinalIgnoreCase);
    }

    private static bool UpdateProgIdCommandIfNeeded(string extension, string exePath, string openCommand)
    {
        if (!ProgIdCommandExists(extension))
        {
            return false;
        }

        using var commandKey = Registry.CurrentUser.OpenSubKey(
            $@"Software\Classes\{GetProgId(extension)}\shell\open\command");
        if (string.Equals(commandKey?.GetValue("") as string, openCommand, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        RegisterProgId(GetProgId(extension), GetFriendlyTypeName(extension), exePath, openCommand);
        return true;
    }

    private static bool ContextMenuCommandMatches(string extension, string exePath, string openCommand)
    {
        var shellKeyPath = $@"Software\Classes\SystemFileAssociations\{extension}\shell\{ContextMenuVerb}";
        using var shellKey = Registry.CurrentUser.OpenSubKey(shellKeyPath);
        if (shellKey is null)
        {
            return false;
        }

        if (!string.Equals(shellKey.GetValue("") as string, ViewerMenuLabel, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.Equals(shellKey.GetValue("Icon") as string, exePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        using var commandKey = Registry.CurrentUser.OpenSubKey(shellKeyPath + @"\command");
        return string.Equals(commandKey?.GetValue("") as string, openCommand, StringComparison.OrdinalIgnoreCase);
    }

    private static bool UpdateContextMenuCommandIfNeeded(string extension, string exePath, string openCommand)
    {
        var shellKeyPath = $@"Software\Classes\SystemFileAssociations\{extension}\shell\{ContextMenuVerb}";
        using var shellKey = Registry.CurrentUser.OpenSubKey(shellKeyPath);
        if (shellKey is null)
        {
            return false;
        }

        if (ContextMenuCommandMatches(extension, exePath, openCommand))
        {
            return false;
        }

        RegisterContextMenuVerb(extension, exePath, openCommand);
        return true;
    }

    /// <summary>取消本程序写入的默认打开关联与右键菜单（并尽量恢复原先 ProgId）。</summary>
    public void UnregisterContextMenu()
    {
        foreach (var ext in OpenFileExtensions)
        {
            var progId = GetProgId(ext);
            UnassociateExtension(ext, progId);
            RemoveOpenWithProgId(ext, progId);
            Registry.CurrentUser.DeleteSubKeyTree(
                $@"Software\Classes\{progId}",
                throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKeyTree(
                $@"Software\Classes\SystemFileAssociations\{ext}\shell\{ContextMenuVerb}",
                throwOnMissingSubKey: false);
        }

        // 兼容旧版仅注册 .md 右键菜单的路径
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"Software\Classes\SystemFileAssociations\.md\shell\{ContextMenuVerb}",
            throwOnMissingSubKey: false);

        UnregisterApplicationCapabilities();
        Registry.CurrentUser.DeleteSubKeyTree(PreviousAssociationsKey, throwOnMissingSubKey: false);
        NotifyShellAssociationsChanged();
    }

    public bool IsFileAssociationRegistered()
    {
        foreach (var ext in OpenFileExtensions)
        {
            var progId = GetProgId(ext);
            using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ext}");
            var current = key?.GetValue("") as string;
            if (!string.Equals(current, progId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            using var command = Registry.CurrentUser.OpenSubKey(
                $@"Software\Classes\{progId}\shell\open\command");
            if (command?.GetValue("") is not string cmd
                || cmd.IndexOf("--view", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }
        }

        return true;
    }

    private static string GetProgId(string extension) =>
        ProgIdPrefix + extension.ToLowerInvariant();

    private static string GetFriendlyTypeName(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".md" or ".markdown" => "Markdown 文档",
            ".json" => "JSON 文档",
            ".xml" => "XML 文档",
            _ => "文档"
        };

    private static void RegisterProgId(string progId, string friendlyName, string exePath, string openCommand)
    {
        using var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}");
        progIdKey?.SetValue("", friendlyName);
        progIdKey?.SetValue("FriendlyTypeName", friendlyName);

        using var iconKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\DefaultIcon");
        iconKey?.SetValue("", $"\"{exePath}\",0");

        using var openKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\shell\open");
        openKey?.SetValue("", "打开(&O)");

        using var commandKey = Registry.CurrentUser.CreateSubKey(
            $@"Software\Classes\{progId}\shell\open\command");
        commandKey?.SetValue("", openCommand);
    }

    private static void AssociateExtension(string extension, string progId)
    {
        var extKeyPath = $@"Software\Classes\{extension}";
        using var extKey = Registry.CurrentUser.CreateSubKey(extKeyPath);
        if (extKey is null)
        {
            return;
        }

        var previous = extKey.GetValue("") as string;
        if (!string.IsNullOrWhiteSpace(previous)
            && !string.Equals(previous, progId, StringComparison.OrdinalIgnoreCase))
        {
            using var backup = Registry.CurrentUser.CreateSubKey(PreviousAssociationsKey);
            backup?.SetValue(extension, previous);
        }

        extKey.SetValue("", progId);

        using var openWith = Registry.CurrentUser.CreateSubKey($@"{extKeyPath}\OpenWithProgids");
        openWith?.SetValue(progId, Array.Empty<byte>(), RegistryValueKind.None);
    }

    private static void UnassociateExtension(string extension, string progId)
    {
        var extKeyPath = $@"Software\Classes\{extension}";
        using var extKey = Registry.CurrentUser.OpenSubKey(extKeyPath, writable: true);
        if (extKey is null)
        {
            return;
        }

        var current = extKey.GetValue("") as string;
        if (string.Equals(current, progId, StringComparison.OrdinalIgnoreCase))
        {
            using var backup = Registry.CurrentUser.OpenSubKey(PreviousAssociationsKey);
            var previous = backup?.GetValue(extension) as string;
            if (!string.IsNullOrWhiteSpace(previous)
                && !string.Equals(previous, progId, StringComparison.OrdinalIgnoreCase))
            {
                extKey.SetValue("", previous);
            }
            else
            {
                extKey.DeleteValue("", throwOnMissingValue: false);
            }
        }

        using var openWith = Registry.CurrentUser.OpenSubKey($@"{extKeyPath}\OpenWithProgids", writable: true);
        openWith?.DeleteValue(progId, throwOnMissingValue: false);
    }

    private static void EnsureOpenWithProgId(string extension, string progId)
    {
        using var key = Registry.CurrentUser.CreateSubKey(
            $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithProgids");
        key?.SetValue(progId, Array.Empty<byte>(), RegistryValueKind.None);
    }

    private static void RemoveOpenWithProgId(string extension, string progId)
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithProgids",
            writable: true);
        key?.DeleteValue(progId, throwOnMissingValue: false);
    }

    private static void RegisterContextMenuVerb(string extension, string exePath, string openCommand)
    {
        var shellKeyPath =
            $@"Software\Classes\SystemFileAssociations\{extension}\shell\{ContextMenuVerb}";
        using var shellKey = Registry.CurrentUser.CreateSubKey(shellKeyPath);
        shellKey?.SetValue("", ViewerMenuLabel);
        shellKey?.SetValue("Icon", exePath);

        using var commandKey = Registry.CurrentUser.CreateSubKey($"{shellKeyPath}\\command");
        commandKey?.SetValue("", openCommand);
    }

    private static void RegisterApplicationOpenWith(string exeName, string openCommand)
    {
        using var commandKey = Registry.CurrentUser.CreateSubKey(
            $@"Software\Classes\Applications\{exeName}\shell\open\command");
        commandKey?.SetValue("", openCommand);

        using var supported = Registry.CurrentUser.CreateSubKey(
            $@"Software\Classes\Applications\{exeName}\SupportedTypes");
        foreach (var ext in OpenFileExtensions)
        {
            supported?.SetValue(ext, string.Empty);
        }
    }

    private static void RegisterApplicationCapabilities(string exePath)
    {
        using var caps = Registry.CurrentUser.CreateSubKey(CapabilitiesKey);
        caps?.SetValue("ApplicationName", "JHViewer");
        caps?.SetValue("ApplicationDescription", "用 JHViewer 单独打开 Markdown / JSON / XML（不进入主界面）");
        caps?.SetValue("ApplicationIcon", $"\"{exePath}\",0");

        using var fileAssoc = Registry.CurrentUser.CreateSubKey(CapabilitiesKey + @"\FileAssociations");
        foreach (var ext in OpenFileExtensions)
        {
            fileAssoc?.SetValue(ext, GetProgId(ext));
        }

        using var registered = Registry.CurrentUser.CreateSubKey(RegisteredApplicationsKey);
        registered?.SetValue("MarkdownContextViewer", CapabilitiesKey);
    }

    private static void UnregisterApplicationCapabilities()
    {
        Registry.CurrentUser.DeleteSubKeyTree(CapabilitiesKey, throwOnMissingSubKey: false);
        using var registered = Registry.CurrentUser.OpenSubKey(RegisteredApplicationsKey, writable: true);
        registered?.DeleteValue("MarkdownContextViewer", throwOnMissingValue: false);
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
        // 开发时用工程的 net48 输出（门面程序集版本正确）。
        var fromProject = FindSiblingProjectOutput("MarkdownPreview.Handler", "MarkdownPreview.Handler.dll");
        if (fromProject is not null)
        {
            return fromProject;
        }

        // 发布包把 net48 依赖放在子目录，避免覆盖自包含运行时的 System.Memory 8.0.0.0。
        var bundled = GetBundledPreviewHandlerDirectory();
        if (File.Exists(Path.Combine(bundled, "MarkdownPreview.Handler.dll")))
        {
            return bundled;
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

        var bundled = GetBundledPreviewHandlerDirectory();
        if (File.Exists(Path.Combine(bundled, "MarkdownPreview.Register.exe")))
        {
            return bundled;
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

        if (!process.WaitForExit(elevate ? 30000 : 20000))
        {
            try { process.Kill(); } catch { /* ignore */ }
            throw new TimeoutException(elevate
                ? "提权注册超时（30 秒）。若 UAC 窗口仍开着请先关闭，或同意后重试。"
                : "注册工具超时（20 秒）。");
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
            process?.WaitForExit(12000);
            if (process is { HasExited: false })
            {
                try { process.Kill(); } catch { /* ignore */ }
            }
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
                process?.WaitForExit(12000);
            if (process is { HasExited: false })
            {
                try { process.Kill(); } catch { /* ignore */ }
            }
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
