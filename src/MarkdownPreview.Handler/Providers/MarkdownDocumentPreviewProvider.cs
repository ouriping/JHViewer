using System.Collections.Generic;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

internal sealed class MarkdownDocumentPreviewProvider : IDocumentPreviewProvider
{
    public string Id => "markdown";

    public string DisplayName => "Markdown";

    public IReadOnlyList<string> Extensions { get; } = new[] { ".md", ".markdown", ".mdown" };

    public bool IsMarkdown => true;

    public bool PreferWinForms => false;

    public void FillTree(TreeView tree, string text)
    {
        // Markdown 由宿主 Present 处理，不使用树视图。
    }

    public string ToHtml(string text) => MarkdownHtmlRenderer.Render(text ?? string.Empty);
}
