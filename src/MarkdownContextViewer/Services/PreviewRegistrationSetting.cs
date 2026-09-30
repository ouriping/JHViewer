namespace MarkdownContextViewer;

/// <summary>持久化用户勾选的预览扩展名。</summary>
public static class PreviewRegistrationSetting
{
    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MarkdownContextViewer",
        "preview-extensions.txt");

    public static PreviewRegistrationOptions Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return PreviewRegistrationOptions.CreateDefault();
            }

            var lines = File.ReadAllLines(FilePath)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.StartsWith('.') && PreviewFormatCatalog.AllExtensions.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (lines.Count == 0)
            {
                return PreviewRegistrationOptions.CreateDefault();
            }

            return new PreviewRegistrationOptions { Extensions = lines };
        }
        catch
        {
            return PreviewRegistrationOptions.CreateDefault();
        }
    }

    public static void Save(PreviewRegistrationOptions options)
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var lines = (options.Extensions ?? [])
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.StartsWith('.'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        File.WriteAllLines(FilePath, lines);
    }
}
