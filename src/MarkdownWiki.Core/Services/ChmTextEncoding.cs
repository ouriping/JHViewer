using System.Text;

namespace MarkdownWiki.Core.Services;

/// <summary>
/// HTML Help Workshop（hhc.exe）按系统 ANSI 读取 .hhp/.hhc/.hhk。
/// 中文 Windows 下为 GBK/GB18030；用 UTF-8 写入会导致目录与标题乱码。
/// </summary>
internal static class ChmTextEncoding
{
    private static readonly Encoding AnsiChinese;

    static ChmTextEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // 54936 = GB18030（兼容 GBK，覆盖更全）；若不可用则回退 936
        try
        {
            AnsiChinese = Encoding.GetEncoding(54936);
        }
        catch
        {
            AnsiChinese = Encoding.GetEncoding(936);
        }
    }

    /// <summary>.hhp / .hhc / .hhk 使用的编码。</summary>
    public static Encoding ProjectFiles => AnsiChinese;

    /// <summary>
    /// 主题 HTML：带 BOM 的 UTF-8，配合 charset=utf-8，
    /// 在现代 hh.exe 中正文显示正常，且不与工程文件编码混用。
    /// </summary>
    public static Encoding TopicHtml { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    public static void WriteAllText(string path, string contents, Encoding encoding)
    {
        File.WriteAllText(path, contents, encoding);
    }
}
