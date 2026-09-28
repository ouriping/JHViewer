using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;

namespace MarkdownWiki.Tests;

public class WikiMediaServiceTests
{
    [Fact]
    public async Task SaveImage_writes_under_assets_and_returns_relative_markdown_path()
    {
        var root = Path.Combine(Path.GetTempPath(), "markdown-wiki-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "guide"));
        File.WriteAllText(Path.Combine(root, "guide", "faq.md"), "# FAQ");

        try
        {
            var options = new WikiOptions { ContentRoot = root };
            var guard = new WikiPathGuard(root);
            var media = new WikiMediaService(guard, options);

            await using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
            var result = await media.SaveImageAsync("guide/faq.md", stream, "clip.png", "image/png");

            Assert.StartsWith("guide/assets/", result.RelativePath);
            Assert.StartsWith("assets/", result.MarkdownPath);
            Assert.EndsWith(".png", result.MarkdownPath);
            Assert.True(File.Exists(Path.Combine(root, result.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task SaveImage_rejects_disallowed_extension()
    {
        var root = Path.Combine(Path.GetTempPath(), "markdown-wiki-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "index.md"), "# Home");

        try
        {
            var options = new WikiOptions { ContentRoot = root, AllowedImageExtensions = [".png"] };
            var media = new WikiMediaService(new WikiPathGuard(root), options);
            await using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                media.SaveImageAsync("index.md", stream, "x.exe", "application/octet-stream"));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
