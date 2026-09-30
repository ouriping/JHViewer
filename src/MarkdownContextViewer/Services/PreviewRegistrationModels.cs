namespace MarkdownContextViewer;

/// <summary>预览窗格注册过程中的进度与日志行。</summary>
public sealed class PreviewRegistrationProgress
{
    public PreviewRegistrationProgress(int percent, string message)
    {
        // percent < 0 表示仅追加日志，不更新进度条
        Percent = percent < 0 ? -1 : Math.Clamp(percent, 0, 100);
        Message = message ?? string.Empty;
    }

    public int Percent { get; }

    public string Message { get; }
}

/// <summary>用户勾选的预览关联扩展名。</summary>
public sealed class PreviewRegistrationOptions
{
    public List<string> Extensions { get; init; } = [];

    public static PreviewRegistrationOptions CreateDefault() => new()
    {
        Extensions = [".md", ".markdown", ".json", ".xml"]
    };
}

/// <summary>已知可注册的预览格式分组（界面勾选用）。</summary>
public static class PreviewFormatCatalog
{
    public static IReadOnlyList<PreviewFormatGroup> Groups { get; } =
    [
        new PreviewFormatGroup(
            "text",
            "Markdown / 文本预览",
            "按 Markdown 渲染（Web 还原度高，WinForms 更快更省）",
            [".md", ".markdown"]),
        new PreviewFormatGroup(
            "json",
            "JSON 结构化预览",
            "树形结构 + 原文（推荐 WinForms 引擎）",
            [".json"]),
        new PreviewFormatGroup(
            "xml",
            "XML 结构化预览",
            "树形结构 + 原文（推荐 WinForms 引擎）",
            [".xml"])
    ];

    public static IReadOnlyList<string> AllExtensions { get; } =
        Groups.SelectMany(g => g.Extensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}

public sealed class PreviewFormatGroup
{
    public PreviewFormatGroup(string id, string title, string description, IReadOnlyList<string> extensions)
    {
        Id = id;
        Title = title;
        Description = description;
        Extensions = extensions;
    }

    public string Id { get; }

    public string Title { get; }

    public string Description { get; }

    public IReadOnlyList<string> Extensions { get; }
}
