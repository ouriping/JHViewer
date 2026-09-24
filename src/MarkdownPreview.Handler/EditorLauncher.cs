using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace MarkdownPreview.Handler;

/// <summary>
/// 从右键菜单注册项找到主程序并打开当前 Markdown。
/// </summary>
internal static class EditorLauncher
{
    private const string CommandKey =
        @"Software\Classes\SystemFileAssociations\.md\shell\MarkdownContextViewer\command";

    private const string IconKey =
        @"Software\Classes\SystemFileAssociations\.md\shell\MarkdownContextViewer";

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
            error = "未找到编辑器。请先在主程序中点击「注册右键菜单」。";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = "\"" + markdownPath + "\"",
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
        var fromCommand = ParseExecutable(ReadRegistry(CommandKey, ""));
        if (File.Exists(fromCommand))
        {
            return fromCommand;
        }

        var fromIcon = ReadRegistry(IconKey, "Icon");
        return File.Exists(fromIcon) ? fromIcon : null;
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
