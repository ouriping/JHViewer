using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkdownPreview.Handler;

internal static class DocumentPreviewRegistry
{
    private static readonly IDocumentPreviewProvider[] Providers =
    {
        new MarkdownDocumentPreviewProvider(),
        new JsonDocumentPreviewProvider(),
        new XmlDocumentPreviewProvider()
    };

    public static IDocumentPreviewProvider Markdown { get; } = Providers[0];

    public static IReadOnlyList<string> AllExtensions { get; } =
        Providers.SelectMany(p => p.Extensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    public static IDocumentPreviewProvider Resolve(string pathOrExtension)
    {
        var input = pathOrExtension ?? string.Empty;
        var ext = Path.GetExtension(input);
        if (string.IsNullOrEmpty(ext) && input.StartsWith(".", StringComparison.Ordinal))
        {
            ext = input;
        }

        ext = ext.Trim().ToLowerInvariant();
        foreach (var provider in Providers)
        {
            if (provider.Extensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase)))
            {
                return provider;
            }
        }

        return Markdown;
    }
}
