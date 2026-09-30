using System.Collections.Generic;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

/// <summary>
/// 进程内文档预览提供者：按扩展名分发不同渲染实现。
/// </summary>
internal interface IDocumentPreviewProvider
{
    string Id { get; }

    string DisplayName { get; }

    IReadOnlyList<string> Extensions { get; }

    /// <summary>是否走宿主内置 Markdown（WebView2 / RichTextBox）路径。</summary>
    bool IsMarkdown { get; }

    /// <summary>结构化文本更推荐 WinForms（更快更省）。</summary>
    bool PreferWinForms { get; }

    /// <summary>填充树视图；解析失败应抛出异常。</summary>
    void FillTree(TreeView tree, string text);

    /// <summary>生成 Web 引擎 / 导出用的 HTML。</summary>
    string ToHtml(string text);
}
