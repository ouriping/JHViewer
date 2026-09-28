using MarkdownWiki.Core.Models;

namespace MarkdownWiki.Core.Services;

/// <summary>
/// 将图片保存到文档同级 <c>assets/</c> 目录（相对路径引用，兼容 CommonMark/GFM）。
/// </summary>
public sealed class WikiMediaService
{
    private static readonly HashSet<string> DefaultAllowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg"
    };

    private readonly WikiPathGuard _pathGuard;
    private readonly WikiOptions _options;

    public WikiMediaService(WikiPathGuard pathGuard, WikiOptions options)
    {
        _pathGuard = pathGuard;
        _options = options;
    }

    public async Task<WikiMediaResult> SaveImageAsync(
        string documentRelativePath,
        Stream content,
        string? fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var docFull = _pathGuard.ResolveSafePath(documentRelativePath, expectDocument: true);
        var docDir = Path.GetDirectoryName(docFull) ?? _pathGuard.ContentRoot;
        var assetsDir = Path.Combine(docDir, "assets");
        Directory.CreateDirectory(assetsDir);

        var ext = ResolveExtension(fileName, contentType);
        if (!IsAllowedExtension(ext))
        {
            throw new InvalidOperationException($"不支持的图片类型：{ext}");
        }

        var maxBytes = _options.MaxUploadBytes > 0 ? _options.MaxUploadBytes : 5 * 1024 * 1024;
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length <= 0)
        {
            throw new InvalidOperationException("上传内容为空。");
        }

        if (buffer.Length > maxBytes)
        {
            throw new InvalidOperationException($"图片超过大小限制（{maxBytes} 字节）。");
        }

        var storedName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(assetsDir, storedName);
        buffer.Position = 0;
        await using (var fs = File.Create(fullPath))
        {
            await buffer.CopyToAsync(fs, cancellationToken);
        }

        var relativeFromRoot = _pathGuard.ToRelativePosix(fullPath);
        return new WikiMediaResult
        {
            RelativePath = relativeFromRoot,
            MarkdownPath = $"assets/{storedName}",
            Url = "/files/" + relativeFromRoot,
            ContentType = contentType ?? GuessContentType(ext),
            SizeBytes = buffer.Length
        };
    }

    private bool IsAllowedExtension(string ext)
    {
        var allowed = _options.AllowedImageExtensions;
        if (allowed is { Length: > 0 })
        {
            return allowed.Any(a => string.Equals(NormalizeExt(a), ext, StringComparison.OrdinalIgnoreCase));
        }

        return DefaultAllowed.Contains(ext);
    }

    private static string ResolveExtension(string? fileName, string? contentType)
    {
        var fromName = NormalizeExt(Path.GetExtension(fileName ?? string.Empty));
        if (!string.IsNullOrEmpty(fromName))
        {
            return fromName;
        }

        return (contentType ?? string.Empty).ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".png"
        };
    }

    private static string NormalizeExt(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext))
        {
            return string.Empty;
        }

        return ext.StartsWith('.') ? ext.ToLowerInvariant() : "." + ext.ToLowerInvariant();
    }

    private static string GuessContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };
}
