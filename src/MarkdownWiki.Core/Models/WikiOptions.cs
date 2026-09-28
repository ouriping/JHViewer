namespace MarkdownWiki.Core.Models;

public sealed class WikiOptions
{
    public const string SectionName = "Wiki";

    /// <summary>文档根目录（绝对或相对路径）。</summary>
    public string ContentRoot { get; set; } = "wiki-content";

    public string SiteTitle { get; set; } = "帮助文档";

    /// <summary>相对文档根的首页文档，如 index.md。</summary>
    public string HomeDocument { get; set; } = "index.md";

    public bool AllowLan { get; set; }

    public int Port { get; set; } = 5099;

    public bool EnableEdit { get; set; } = true;

    /// <summary>树扫描时忽略的目录名（不区分大小写）。</summary>
    public string[] IgnoredDirectories { get; set; } = ["_meta", ".git", "node_modules"];
}
