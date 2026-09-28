using Microsoft.Win32;

namespace MarkdownContextViewer;

/// <summary>
/// 将主程序路径写入注册表，供预览 Handler 的 EditorLauncher 定位。
/// </summary>
internal static class EditorPathStore
{
    public const string RegistryKey = @"Software\MarkdownContextViewer";
    public const string EditorExeValue = "EditorExe";

    public static void Save(string exePath)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return;
        }

        using var key = Registry.CurrentUser.CreateSubKey(RegistryKey);
        key?.SetValue(EditorExeValue, exePath, RegistryValueKind.String);
    }
}
