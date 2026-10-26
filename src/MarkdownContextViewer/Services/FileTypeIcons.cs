namespace MarkdownContextViewer;

/// <summary>
/// 文件关联图标。三种类型共用文档模板，徽章颜色和文字不同，文件放在程序旁的 icons 目录。
/// </summary>
internal static class FileTypeIcons
{
    public static string GetShellIconValue(string exePath, string extension)
    {
        var iconPath = ResolveIconPath(exePath, extension);
        return $"\"{iconPath ?? exePath}\",0";
    }

    public static string? ResolveIconPath(string exePath, string extension)
    {
        var directory = Path.GetDirectoryName(exePath);
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        var fileName = extension.ToLowerInvariant() switch
        {
            ".md" or ".markdown" or ".mdown" => "jhviewer-md.ico",
            ".json" => "jhviewer-json.ico",
            ".xml" => "jhviewer-xml.ico",
            _ => null
        };

        if (fileName is null)
        {
            return null;
        }

        var path = Path.Combine(directory, "icons", fileName);
        return File.Exists(path) ? path : null;
    }
}
