namespace MarkdownWiki.Core.Models;

public sealed class ChmExportOptions
{
    public string Title { get; set; } = "帮助文档";

    /// <summary>相对文档根的首页，如 index.md。</summary>
    public string HomeDocument { get; set; } = "index.md";

    /// <summary>构建与输出根目录（绝对路径）。</summary>
    public string OutputDirectory { get; set; } = "chm-output";

    public string FileName { get; set; } = "Help.chm";

    /// <summary>显式编译器路径；空则自动探测。</summary>
    public string? CompilerPath { get; set; }

    /// <summary>Auto | Hhc | Chmcmd | None</summary>
    public string PreferredCompiler { get; set; } = "Auto";

    public bool IncludeIndex { get; set; } = true;

    public bool FullTextSearch { get; set; } = true;

    /// <summary>为 true 时若找到编译器则调用；否则只生成中间文件。</summary>
    public bool Compile { get; set; } = true;
}
