using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownPreview.Handler;

/// <summary>
/// 把预览 HTML 写成单文件：样式已内联，本地图片转为 data URI。
/// </summary>
internal static class HtmlSingleFileExporter
{
    private const int MaxImageBytes = 2 * 1024 * 1024;

    private static readonly Regex ImgSrc = new Regex(
        "(<img\\b[^>]*?\\bsrc\\s*=\\s*\")([^\"]+)(\")",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static void Export(string html, string markdownPath, string outputPath)
    {
        var directory = Path.GetDirectoryName(markdownPath) ?? "";
        var portable = ImgSrc.Replace(html ?? "", match => EmbedImage(match, directory));
        File.WriteAllText(outputPath, portable, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string EmbedImage(Match match, string markdownDirectory)
    {
        var src = match.Groups[2].Value;
        if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            src.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            src.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return match.Value;
        }

        var local = ResolveLocalPath(src, markdownDirectory);
        if (local == null || !File.Exists(local))
        {
            return match.Value;
        }

        var info = new FileInfo(local);
        if (info.Length <= 0 || info.Length > MaxImageBytes)
        {
            return match.Groups[1].Value + new Uri(local).AbsoluteUri + match.Groups[3].Value;
        }

        var mime = MimeFromExtension(Path.GetExtension(local));
        var base64 = Convert.ToBase64String(File.ReadAllBytes(local));
        return match.Groups[1].Value + "data:" + mime + ";base64," + base64 + match.Groups[3].Value;
    }

    private static string ResolveLocalPath(string src, string markdownDirectory)
    {
        try
        {
            var decoded = Uri.UnescapeDataString(src.Replace('\\', '/'));
            if (decoded.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(decoded).LocalPath;
            }

            if (Path.IsPathRooted(decoded))
            {
                return decoded;
            }

            return Path.GetFullPath(Path.Combine(markdownDirectory, decoded.Replace('/', Path.DirectorySeparatorChar)));
        }
        catch
        {
            return null;
        }
    }

    private static string MimeFromExtension(string extension)
    {
        switch ((extension ?? "").ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            case ".webp":
                return "image/webp";
            case ".svg":
                return "image/svg+xml";
            case ".bmp":
                return "image/bmp";
            case ".ico":
                return "image/x-icon";
            default:
                return "image/png";
        }
    }
}
