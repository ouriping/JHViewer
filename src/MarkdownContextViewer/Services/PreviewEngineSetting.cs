namespace MarkdownContextViewer;

public enum PreviewEngineKind
{
    WebView,
    WinForms
}

/// <summary>
/// 主程序与资源管理器预览宿主共用的渲染引擎选择。
/// </summary>
public static class PreviewEngineSetting
{
    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MarkdownContextViewer",
        "preview-engine.txt");

    public static PreviewEngineKind Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return PreviewEngineKind.WebView;
            }

            var text = File.ReadAllText(FilePath).Trim();
            return text.Equals("WinForms", StringComparison.OrdinalIgnoreCase)
                ? PreviewEngineKind.WinForms
                : PreviewEngineKind.WebView;
        }
        catch
        {
            return PreviewEngineKind.WebView;
        }
    }

    public static void Save(PreviewEngineKind kind)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(FilePath, kind == PreviewEngineKind.WinForms ? "WinForms" : "WebView");
    }
}
