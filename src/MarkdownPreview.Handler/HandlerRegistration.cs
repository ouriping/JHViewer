using System;
using Microsoft.Win32;
using SharpShell.ServerRegistration;

namespace MarkdownPreview.Handler;

/// <summary>
/// 供主程序 / 注册工具调用的安装入口。
/// 以 HKCU CodeBase 注册为主（稳定、无需管理员），SharpShell API 为辅。
/// </summary>
public static class HandlerRegistration
{
    public static void Install()
    {
        Install(DocumentPreviewRegistry.AllExtensions);
    }

    public static void Install(System.Collections.Generic.IEnumerable<string> extensions)
    {
        var server = new MarkdownPreviewHandler();
        TryUninstallQuiet(server);

        // 主路径：手写 .NET Framework COM CodeBase（避免 SharpShell InstallServer 半残状态）
        InstallViaRegistryCodeBase();
        EnsureUserAssociations(extensions);
        EnsureDisableLowIlUser();

        // 尽力再用 SharpShell 补全 PreviewHandlers / 关联（失败不影响）
        try
        {
            ServerRegistrationManager.RegisterServer(server, RegistrationType.OS64Bit);
            ServerRegistrationManager.SetDisableProcessIsolationValue(
                server.ServerClsid,
                RegistrationType.OS64Bit,
                1);
        }
        catch
        {
            // ignore
        }
    }

    public static void Uninstall()
    {
        var server = new MarkdownPreviewHandler();
        TryUninstallQuiet(server);
        try
        {
            ServerRegistrationManager.UnregisterServer(server, RegistrationType.OS64Bit);
        }
        catch
        {
            // ignore
        }

        DeleteUserClsidTree();
        ClearUserAssociations();
    }

    private static void TryUninstallQuiet(MarkdownPreviewHandler server)
    {
        try
        {
            ServerRegistrationManager.UninstallServer(server, RegistrationType.OS64Bit);
        }
        catch
        {
            // ignore
        }
    }

    private static void InstallViaRegistryCodeBase()
    {
        var asm = typeof(MarkdownPreviewHandler).Assembly;
        var codeBase = new Uri(asm.Location).AbsoluteUri;
        var version = asm.GetName().Version?.ToString() ?? "1.0.0.0";
        var fullName = asm.FullName;
        var className = typeof(MarkdownPreviewHandler).FullName;
        var clsid = HandlerIds.ClsidBraced;

        // 先清掉可能残留的旧 comhost / 半残键
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"Software\Classes\CLSID\{clsid}",
            throwOnMissingSubKey: false);

        using (var appId = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppID\{clsid}"))
        {
            appId?.SetValue("DllSurrogate", "Prevhost.exe");
        }

        using (var clsidKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{clsid}"))
        {
            if (clsidKey == null)
            {
                throw new InvalidOperationException("无法创建 CLSID 注册表项。");
            }

            clsidKey.SetValue("", HandlerIds.DisplayName);
            clsidKey.SetValue("DisplayName", HandlerIds.DisplayName);
            clsidKey.SetValue("AppID", clsid);
            clsidKey.SetValue("DisableLowILProcessIsolation", 1, RegistryValueKind.DWord);

            WriteInproc(clsidKey, null, className, fullName, version, codeBase);
            WriteInproc(clsidKey, version, className, fullName, version, codeBase);
        }

        using (var previewHandlers = Registry.CurrentUser.CreateSubKey(
                   @"Software\Microsoft\Windows\CurrentVersion\PreviewHandlers"))
        {
            previewHandlers?.SetValue(clsid, HandlerIds.DisplayName);
        }
    }

    private static void WriteInproc(
        RegistryKey clsidKey,
        string versionSubKey,
        string className,
        string assembly,
        string version,
        string codeBase)
    {
        var path = string.IsNullOrEmpty(versionSubKey)
            ? "InprocServer32"
            : @"InprocServer32\" + versionSubKey;

        using var inproc = clsidKey.CreateSubKey(path);
        if (inproc == null)
        {
            return;
        }

        inproc.SetValue("", "mscoree.dll");
        inproc.SetValue("ThreadingModel", "Both");
        inproc.SetValue("Class", className);
        inproc.SetValue("Assembly", assembly);
        inproc.SetValue("RuntimeVersion", "v4.0.30319");
        inproc.SetValue("CodeBase", codeBase);
    }

    private static void EnsureUserAssociations(System.Collections.Generic.IEnumerable<string> extensions)
    {
        const string shellEx = "{8895b1c6-b41f-4c1c-a562-0d564250836f}";

        // 先清掉已知扩展，再按本次勾选写入，避免残留旧关联。
        ClearUserAssociations();

        foreach (var raw in extensions ?? System.Array.Empty<string>())
        {
            var ext = NormalizeExt(raw);
            if (ext.Length == 0)
            {
                continue;
            }

            foreach (var root in new[]
                     {
                         @"Software\Classes\" + ext,
                         @"Software\Classes\SystemFileAssociations\" + ext
                     })
            {
                using var key = Registry.CurrentUser.CreateSubKey(root);
                key?.SetValue("PerceivedType", "text");
                using var preview = Registry.CurrentUser.CreateSubKey(root + @"\shellex\" + shellEx);
                preview?.SetValue("", HandlerIds.ClsidBraced);
            }
        }

        using var previewHandlers = Registry.CurrentUser.CreateSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\PreviewHandlers");
        previewHandlers?.SetValue(HandlerIds.ClsidBraced, HandlerIds.DisplayName);
    }

    private static string NormalizeExt(string value)
    {
        var ext = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (ext.Length == 0)
        {
            return string.Empty;
        }

        if (!ext.StartsWith(".", System.StringComparison.Ordinal))
        {
            ext = "." + ext;
        }

        return ext;
    }

    private static void EnsureDisableLowIlUser()
    {
        using var key = Registry.CurrentUser.CreateSubKey(
            $@"Software\Classes\CLSID\{HandlerIds.ClsidBraced}");
        key?.SetValue("DisableLowILProcessIsolation", 1, RegistryValueKind.DWord);
    }

    private static void DeleteUserClsidTree()
    {
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"Software\Classes\CLSID\{HandlerIds.ClsidBraced}",
            throwOnMissingSubKey: false);
        Registry.CurrentUser.DeleteSubKeyTree(
            $@"Software\Classes\AppID\{HandlerIds.ClsidBraced}",
            throwOnMissingSubKey: false);

        using var previewHandlers = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\PreviewHandlers", writable: true);
        if (previewHandlers?.GetValue(HandlerIds.ClsidBraced) != null)
        {
            previewHandlers.DeleteValue(HandlerIds.ClsidBraced, throwOnMissingValue: false);
        }
    }

    private static void ClearUserAssociations()
    {
        const string shellEx = "{8895b1c6-b41f-4c1c-a562-0d564250836f}";
        foreach (var ext in DocumentPreviewRegistry.AllExtensions)
        {
            foreach (var path in new[]
                     {
                         @"Software\Classes\" + ext + @"\shellex\" + shellEx,
                         @"Software\Classes\SystemFileAssociations\" + ext + @"\shellex\" + shellEx
                     })
            {
                Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            }
        }
    }
}
