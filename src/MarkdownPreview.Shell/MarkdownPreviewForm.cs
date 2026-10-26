namespace MarkdownPreview.Shell;

/// <summary>
/// 可在 VS2022 设计器中打开的 Markdown 预览窗口，便于调试渲染效果。
/// </summary>
public partial class MarkdownPreviewForm : Form
{
    public MarkdownPreviewForm()
    {
        InitializeComponent();
    }

    public MarkdownPreviewForm(string filePath) : this()
    {
        LoadFile(filePath);
    }

    public void LoadFile(string filePath)
    {
        Text = $"JHViewer - {Path.GetFileName(filePath)}";
        markdownPreviewControl.LoadMarkdownFile(filePath);
    }

    public void LoadMarkdown(string markdown, string? title = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Text = title;
        }

        markdownPreviewControl.LoadMarkdown(markdown);
    }

    private void btnOpen_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Markdown 文件 (*.md)|*.md|所有文件 (*.*)|*.*",
            Title = "选择要预览的 Markdown 文件",
            //打开历史记录
            RestoreDirectory = true,
            //显示最近打开的文件
            CheckFileExists = true,
            //显示最近打开的文件
            CheckPathExists = true,
            //显示最近打开的文件
            Multiselect = false,
            //显示最近打开的文件
            ReadOnlyChecked = false,
            //显示最近打开的文件
            ShowReadOnly = false,
            //显示最近打开的文件
            ShowHelp = false,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        var path = markdownPreviewControl.CurrentFilePath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            LoadFile(path);
        }
    }
}
