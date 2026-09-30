using System.Net;
using System.Text;
using MarkdownWiki.Core.Models;

namespace MarkdownWiki.Core.Services;

public sealed class ChmProjectWriter
{
    public void WriteContents(string hhcPath, WikiNode tree, Func<string, string> mdPathToHtmLocal)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
        sb.AppendLine("<HTML>");
        sb.AppendLine("<HEAD>");
        sb.AppendLine("<meta name=\"GENERATOR\" content=\"MarkdownWiki ChmExport\">");
        sb.AppendLine("<!-- Sitemap 1.0 -->");
        sb.AppendLine("</HEAD><BODY>");
        sb.AppendLine("<OBJECT type=\"text/site properties\">");
        sb.AppendLine("\t<param name=\"FrameName\" value=\"right\">");
        sb.AppendLine("</OBJECT>");
        WriteNodes(sb, tree.Children, mdPathToHtmLocal, indent: 0);
        sb.AppendLine("</BODY></HTML>");
        File.WriteAllText(hhcPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public void WriteIndex(string hhkPath, IEnumerable<(string name, string local)> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
        sb.AppendLine("<HTML>");
        sb.AppendLine("<HEAD>");
        sb.AppendLine("<meta name=\"GENERATOR\" content=\"MarkdownWiki ChmExport\">");
        sb.AppendLine("<!-- Sitemap 1.0 -->");
        sb.AppendLine("</HEAD><BODY>");
        sb.AppendLine("<UL>");
        foreach (var (name, local) in entries.OrderBy(e => e.name, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine("\t<LI> <OBJECT type=\"text/sitemap\">");
            sb.AppendLine($"\t\t<param name=\"Name\" value=\"{EscapeAttr(name)}\">");
            sb.AppendLine($"\t\t<param name=\"Local\" value=\"{EscapeAttr(local)}\">");
            sb.AppendLine("\t\t</OBJECT>");
        }

        sb.AppendLine("</UL>");
        sb.AppendLine("</BODY></HTML>");
        File.WriteAllText(hhkPath, sb.ToString(), new UTF8Encoding(false));
    }

    public void WriteProject(
        string hhpPath,
        ChmExportOptions options,
        string defaultTopicLocal,
        IReadOnlyList<string> files,
        bool includeIndex)
    {
        var chmName = options.FileName;
        if (!chmName.EndsWith(".chm", StringComparison.OrdinalIgnoreCase))
        {
            chmName += ".chm";
        }

        var sb = new StringBuilder();
        sb.AppendLine("[OPTIONS]");
        sb.AppendLine("Compatibility=1.1 or later");
        sb.AppendLine($"Compiled file={chmName}");
        sb.AppendLine("Contents file=TableOfContents.hhc");
        if (includeIndex)
        {
            sb.AppendLine("Index file=Index.hhk");
        }

        sb.AppendLine($"Default topic={defaultTopicLocal}");
        sb.AppendLine("Display compile progress=Yes");
        sb.AppendLine("Language=0x804 中文(中国)");
        sb.AppendLine($"Title={options.Title}");
        sb.AppendLine("Default Font=Microsoft YaHei,9,0");
        sb.AppendLine(options.FullTextSearch ? "Full-text search=Yes" : "Full-text search=No");
        sb.AppendLine("Binary TOC=Yes");
        sb.AppendLine("Default Window=Main");
        sb.AppendLine();
        sb.AppendLine("[WINDOWS]");
        sb.AppendLine($"Main=\"{options.Title}\",\"TableOfContents.hhc\",\"{(includeIndex ? "Index.hhk" : "")}\",\"{defaultTopicLocal}\",\"{defaultTopicLocal}\",,,,,0x62520,,0x104e,,,,,,,,0");
        sb.AppendLine();
        sb.AppendLine("[FILES]");
        foreach (var file in files.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine(file.Replace('/', '\\'));
        }

        File.WriteAllText(hhpPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void WriteNodes(
        StringBuilder sb,
        IEnumerable<WikiNode> nodes,
        Func<string, string> mdPathToHtmLocal,
        int indent)
    {
        var list = nodes.ToList();
        if (list.Count == 0)
        {
            return;
        }

        var pad = new string('\t', indent);
        sb.AppendLine(pad + "<UL>");
        foreach (var node in list)
        {
            if (node.Kind == WikiNodeKind.Folder)
            {
                sb.AppendLine(pad + "\t<LI> <OBJECT type=\"text/sitemap\">");
                sb.AppendLine(pad + $"\t\t<param name=\"Name\" value=\"{EscapeAttr(node.Name)}\">");
                sb.AppendLine(pad + "\t\t</OBJECT>");
                WriteNodes(sb, node.Children, mdPathToHtmLocal, indent + 1);
            }
            else
            {
                var local = mdPathToHtmLocal(node.RelativePath).Replace('/', '\\');
                sb.AppendLine(pad + "\t<LI> <OBJECT type=\"text/sitemap\">");
                sb.AppendLine(pad + $"\t\t<param name=\"Name\" value=\"{EscapeAttr(node.Name)}\">");
                sb.AppendLine(pad + $"\t\t<param name=\"Local\" value=\"{EscapeAttr(local)}\">");
                sb.AppendLine(pad + "\t\t</OBJECT>");
            }
        }

        sb.AppendLine(pad + "</UL>");
    }

    private static string EscapeAttr(string value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);
}
