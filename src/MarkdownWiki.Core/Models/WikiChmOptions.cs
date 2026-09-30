namespace MarkdownWiki.Core.Models;

/// <summary>嵌套在 WikiOptions 下的 CHM 导出默认配置。</summary>
public sealed class WikiChmOptions
{
    public string OutputDirectory { get; set; } = "chm-output";

    public string FileName { get; set; } = "JinghuaHelp.chm";

    public string CompilerPath { get; set; } = string.Empty;

    public string PreferredCompiler { get; set; } = "Auto";

    public bool IncludeIndex { get; set; } = true;

    public bool FullTextSearch { get; set; } = true;
}
