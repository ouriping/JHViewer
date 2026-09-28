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

    /// <summary>管理后台登录密码（固定口令，默认 123456）。</summary>
    public string AdminPassword { get; set; } = "123456";

    /// <summary>树扫描时忽略的目录名（不区分大小写）。</summary>
    public string[] IgnoredDirectories { get; set; } = ["_meta", ".git", "node_modules", "assets"];

    /// <summary>单张图片上传大小上限（字节），默认 5MB。</summary>
    public long MaxUploadBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>允许的图片扩展名（含点或不含点均可）。</summary>
    public string[] AllowedImageExtensions { get; set; } = [".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg"];
}
