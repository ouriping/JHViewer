using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownPreview.Shell;

/// <summary>
/// 把渲染后的 HTML 和其中的相对图片写到同一目录，供 IE 预览和浏览器打印使用。
/// </summary>
internal static class MarkdownHtmlPackager
{
    private static readonly Regex ImgSrcRegex = new(
        @"<img\b([^>]*?)\bsrc\s*=\s*(['""])(?<src>[^'""]+)\2([^>]*)>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Write(string html, string? baseDirectory, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var assets = Path.Combine(outputDirectory, "assets");
        var index = 0;
        var rewritten = ImgSrcRegex.Replace(html ?? string.Empty, match =>
        {
            var src = WebUtility.HtmlDecode(match.Groups["src"].Value.Trim());
            if (!MarkdownAssetPath.TryResolveLocalFile(src, baseDirectory, out var fullPath))
            {
                return match.Value;
            }

            Directory.CreateDirectory(assets);
            index++;
            var destName = "img" + index + Path.GetExtension(fullPath);
            File.Copy(fullPath, Path.Combine(assets, destName), overwrite: true);
            var before = match.Groups[1].Value;
            var quote = match.Groups[2].Value;
            var after = match.Groups[3].Value;
            return $"<img{before}src={quote}assets/{destName}{quote}{after}>";
        });

        var htmlPath = Path.Combine(outputDirectory, "index.html");
        File.WriteAllText(htmlPath, rewritten, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return htmlPath;
    }
}
