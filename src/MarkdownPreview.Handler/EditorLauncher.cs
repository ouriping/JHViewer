using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace MarkdownPreview.Handler;

/// <summary>
/// 定位 MarkdownContextViewer.exe 并用路径打开当前 Markdown。
/// 查找顺序：注册表右键菜单 → EditorExe → 旁路/部署目录 → 向上搜索。
/// 传入 --edit，让主程序打开编辑主界面；不带该参数的文件关联只会打开单文件查看窗口。
/// </summary>
internal static class EditorLauncher
{
    private const string ContextCommandKey =
        @"Software\Classes\SystemFileAssociations\.md\shell\MarkdownContextViewer\command";

    private const string ContextShellKey =
        @"Software\Classes\SystemFileAssociations\.md\shell\MarkdownContextViewer";

    private const string ProgIdOpenCommandKey =
        @"Software\Classes\MarkdownContextViewer.md\shell\open\command";

    private const string AppRegistryKey = @"Software\MarkdownContextViewer";
    private const string EditorExeValue = "EditorExe";
    private const string ExeName = "MarkdownContextViewer.exe";

    public static bool TryLaunch(string markdownPath, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(markdownPath) || !File.Exists(markdownPath))
        {
            error = "当前没有可打开的 Markdown 文件。";
            return false;
        }

        var exe = FindExecutable();
        if (exe == null)
        {
            error = "未找到编辑器（MarkdownContextViewer.exe）。\n请先运行主程序，或在主程序中点击「注册默认打开与右键菜单」。";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = "--edit \"" + markdownPath + "\"",
                WorkingDirectory = Path.GetDirectoryName(exe) ?? "",
                UseShellExecute = false
            });
            return true;
        }
        catch (Exception ex)
        {
            HandlerLog.Error("打开编辑器失败。", ex);
            error = "打开编辑器失败：" + ex.Message;
            return false;
        }
    }

    private static string FindExecutable()
    {
        foreach (var candidate in EnumerateCandidates())
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                HandlerLog.Info("定位编辑器: " + candidate);
                return candidate;
            }
        }

        return null;
    }

    private static System.Collections.Generic.IEnumerable<string> EnumerateCandidates()
    {
        yield return ParseExecutable(ReadRegistry(ContextCommandKey, ""));
        yield return ReadRegistry(ContextShellKey, "Icon");
        yield return ParseExecutable(ReadRegistry(ProgIdOpenCommandKey, ""));
        yield return ReadRegistry(AppRegistryKey, EditorExeValue);

        string assemblyDir = null;
        try
        {
            assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取 Handler 目录失败。", ex);
        }

        if (!string.IsNullOrEmpty(assemblyDir))
        {
            yield return Path.Combine(assemblyDir, ExeName);

            // 部署目录为 %LocalAppData%\MarkdownContextViewer\PreviewHandler
            var parent = Directory.GetParent(assemblyDir)?.FullName;
            if (!string.IsNullOrEmpty(parent))
            {
                yield return Path.Combine(parent, ExeName);
            }

            // 开发布局：Handler\bin\...\net48 → ContextViewer\bin\...\net8.0-windows
            yield return Path.GetFullPath(Path.Combine(
                assemblyDir,
                "..", "..", "..", "..",
                "MarkdownContextViewer", "bin", "Debug", "net8.0-windows", ExeName));
            yield return Path.GetFullPath(Path.Combine(
                assemblyDir,
                "..", "..", "..", "..",
                "MarkdownContextViewer", "bin", "Release", "net8.0-windows", ExeName));

            foreach (var found in SearchUpward(assemblyDir, ExeName, maxLevels: 6))
            {
                yield return found;
            }
        }

        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        yield return Path.Combine(localApp, "MarkdownContextViewer", ExeName);
    }

    private static System.Collections.Generic.IEnumerable<string> SearchUpward(
        string startDir, string fileName, int maxLevels)
    {
        var dir = startDir;
        for (var i = 0; i < maxLevels && !string.IsNullOrEmpty(dir); i++)
        {
            var candidate = Path.Combine(dir, fileName);
            if (File.Exists(candidate))
            {
                yield return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }
    }

    private static string ReadRegistry(string subKey, string name)
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(subKey))
            {
                return key?.GetValue(name) as string;
            }
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取编辑器路径失败。", ex);
            return null;
        }
    }

    private static string ParseExecutable(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        command = command.Trim();
        if (command.StartsWith("\"", StringComparison.Ordinal))
        {
            var end = command.IndexOf('"', 1);
            return end > 1 ? command.Substring(1, end - 1) : null;
        }

        var space = command.IndexOf(' ');
        return space < 0 ? command : command.Substring(0, space);
    }
}
