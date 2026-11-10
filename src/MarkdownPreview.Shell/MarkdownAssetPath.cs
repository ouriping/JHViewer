using System.Net;

namespace MarkdownPreview.Shell;

/// <summary>把 Markdown 里的图片地址解析成文档目录下的本地文件。</summary>
internal static class MarkdownAssetPath
{
    public static bool TryResolveLocalFile(string? src, string? baseDirectory, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(baseDirectory))
        {
            return false;
        }

        src = WebUtility.HtmlDecode(src).Trim();
        if (src.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("//", StringComparison.Ordinal)
            || src.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            if (src.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = new Uri(src).LocalPath;
            }
            else
            {
                var decoded = Uri.UnescapeDataString(src).Replace('/', Path.DirectorySeparatorChar);
                fullPath = IsWindowsAbsolute(decoded)
                    ? Path.GetFullPath(decoded)
                    : Path.GetFullPath(Path.Combine(baseDirectory, decoded.TrimStart(Path.DirectorySeparatorChar)));
            }
        }
        catch
        {
            return false;
        }

        return File.Exists(fullPath);
    }

    private static bool IsWindowsAbsolute(string path)
    {
        if (path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return true;
        }

        return path.Length >= 3
            && char.IsLetter(path[0])
            && path[1] == ':'
            && (path[2] == '\\' || path[2] == '/');
    }
}
