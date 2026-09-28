using MarkdownWiki.Core.Models;
using MarkdownWiki.Core.Services;

namespace MarkdownWiki.Tests;

public class WikiPathGuardTests
{
    private static string TempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "markdown-wiki-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    [Fact]
    public void Rejects_path_traversal()
    {
        var root = TempRoot();
        try
        {
            var guard = new WikiPathGuard(root);
            Assert.Throws<InvalidOperationException>(() => guard.ResolveSafePath("../secret.md", expectDocument: true));
            Assert.Throws<InvalidOperationException>(() => guard.ResolveSafePath("a/../../secret.md", expectDocument: true));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Resolves_nested_document()
    {
        var root = TempRoot();
        try
        {
            var nested = Path.Combine(root, "guide");
            Directory.CreateDirectory(nested);
            File.WriteAllText(Path.Combine(nested, "faq.md"), "# FAQ");

            var guard = new WikiPathGuard(root);
            var full = guard.ResolveSafePath("guide/faq.md", expectDocument: true);
            Assert.True(File.Exists(full));
            Assert.Equal("guide/faq.md", guard.ToRelativePosix(full));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}

public class WikiTreeAndDocumentTests
{
    private static (string root, WikiPathGuard guard, WikiTreeService tree, WikiDocumentService docs) CreateServices()
    {
        var root = Path.Combine(Path.GetTempPath(), "markdown-wiki-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "getting-started"));
        File.WriteAllText(Path.Combine(root, "index.md"), "# Home\n\nHello");
        File.WriteAllText(Path.Combine(root, "getting-started", "install.md"), "# Install");

        var options = new WikiOptions { ContentRoot = root, SiteTitle = "Test" };
        var guard = new WikiPathGuard(root);
        var tree = new WikiTreeService(guard, options);
        var docs = new WikiDocumentService(guard, tree);
        return (root, guard, tree, docs);
    }

    [Fact]
    public void Builds_tree_with_folders_and_docs()
    {
        var (root, _, tree, _) = CreateServices();
        try
        {
            var node = tree.GetTree();
            Assert.Contains(node.Children, c => c.Name == "getting-started" && c.Kind == WikiNodeKind.Folder);
            Assert.Contains(node.Children, c => c.Name == "index" && c.Kind == WikiNodeKind.Document);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Save_and_search_work()
    {
        var (root, _, _, docs) = CreateServices();
        try
        {
            docs.Save("index.md", "# Home\n\nUpdated unique-token-xyz");
            var doc = docs.Get("index.md");
            Assert.Contains("unique-token-xyz", doc.Markdown);

            var hits = docs.Search("unique-token-xyz");
            Assert.Contains(hits, h => h.RelativePath == "index.md");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void Create_rename_delete_document()
    {
        var (root, _, _, docs) = CreateServices();
        try
        {
            var created = docs.CreateDocument("getting-started", "new-page");
            Assert.Equal("getting-started/new-page.md", created);
            Assert.True(File.Exists(Path.Combine(root, "getting-started", "new-page.md")));

            var renamed = docs.Rename(created, "renamed.md");
            Assert.Equal("getting-started/renamed.md", renamed);

            docs.Delete(renamed);
            Assert.False(File.Exists(Path.Combine(root, "getting-started", "renamed.md")));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
