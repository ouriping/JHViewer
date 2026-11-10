namespace MarkdownPreview.Shell;

/// <summary>
/// Markdown 预览窗口。每个窗口可打开多个文档标签；将一个窗口的标题栏拖到另一个预览窗口上并松开时合并标签。
/// </summary>
public partial class MarkdownPreviewForm : Form
{
    private const int WmNcLButtonDown = 0x00A1;
    private const int WmEnterSizeMove = 0x0231;
    private const int WmExitSizeMove = 0x0232;
    private const int HtCaption = 2;
    private const int CloseButtonSize = 16;
    private const int TabDragThreshold = 5;
    private const int MinimumMergeWidth = 120;
    private const int MinimumMergeHeight = 80;
    private const double MinimumOverlapRatio = 0.30;

    private readonly TabControl _documentTabs = new();
    private readonly Dictionary<TabPage, Rectangle> _closeButtonBounds = new();
    private TabPage? _dragTab;
    private Point _dragStart;
    private bool _captionDragCandidate;
    private bool _windowMoveInProgress;
    private bool _merging;

    public MarkdownPreviewForm()
    {
        InitializeComponent();

        Controls.Remove(markdownPreviewControl);
        _documentTabs.Dock = DockStyle.Fill;
        _documentTabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        _documentTabs.ItemSize = new Size(190, 28);
        _documentTabs.SizeMode = TabSizeMode.Fixed;
        _documentTabs.DrawItem += DocumentTabs_DrawItem;
        _documentTabs.MouseDown += DocumentTabs_MouseDown;
        _documentTabs.MouseMove += DocumentTabs_MouseMove;
        _documentTabs.MouseUp += DocumentTabs_MouseUp;
        _documentTabs.MouseClick += DocumentTabs_MouseClick;
        _documentTabs.SelectedIndexChanged += (_, _) => UpdateWindowTitle();
        Controls.Add(_documentTabs);
        menuStrip1.BringToFront();

        AddPreviewTab(markdownPreviewControl, "新建文档", null);
        UpdateWindowTitle();
    }

    public MarkdownPreviewForm(string filePath) : this()
    {
        LoadFile(filePath);
    }

    /// <summary>打开文件；已在当前窗口或其它预览窗口打开时，切换到已有标签。</summary>
    public void LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            MessageBox.Show($"无法打开文件：{filePath}", "Markdown 预览", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var fullPath = Path.GetFullPath(filePath);
        var existing = FindOpenDocument(fullPath);
        if (existing is not null)
        {
            existing.Form._documentTabs.SelectedTab = existing.Page;
            existing.Form.RestoreAndActivate();
            return;
        }

        var view = new MarkdownPreviewControl();
        view.LoadMarkdownFile(fullPath);
        AddPreviewTab(view, Path.GetFileName(fullPath), fullPath);
    }

    public void LoadMarkdown(string markdown, string? title = null)
    {
        var view = new MarkdownPreviewControl();
        view.LoadMarkdown(markdown);
        AddPreviewTab(view, string.IsNullOrWhiteSpace(title) ? "新建文档" : title, null);
    }

    /// <summary>从文件管理器或其它入口调用，使用非模态预览窗体打开 Markdown 文件。</summary>
    public static MarkdownPreviewForm OpenFileWindow(string filePath, IWin32Window? owner = null)
    {
        var existing = FindOpenDocument(Path.GetFullPath(filePath));
        if (existing is not null)
        {
            existing.Form._documentTabs.SelectedTab = existing.Page;
            existing.Form.RestoreAndActivate();
            return existing.Form;
        }

        var form = new MarkdownPreviewForm();
        form.LoadFile(filePath);
        if (owner is null)
        {
            form.Show();
        }
        else
        {
            form.Show(owner);
        }

        return form;
    }

    private void RestoreAndActivate()
    {
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }

        if (!Visible)
        {
            Show();
        }

        Activate();
        BringToFront();
    }

    private void AddPreviewTab(MarkdownPreviewControl view, string title, string? filePath)
    {
        var page = new TabPage(title)
        {
            Tag = filePath,
            ToolTipText = filePath ?? title
        };
        view.Dock = DockStyle.Fill;
        page.Controls.Add(view);
        _documentTabs.TabPages.Add(page);
        _documentTabs.SelectedTab = page;
        UpdateWindowTitle();
    }

    private static (MarkdownPreviewForm Form, TabPage Page)? FindOpenDocument(string fullPath)
    {
        foreach (Form openForm in Application.OpenForms)
        {
            if (openForm is not MarkdownPreviewForm previewForm || previewForm.IsDisposed)
            {
                continue;
            }

            foreach (TabPage page in previewForm._documentTabs.TabPages)
            {
                if (page.Tag is string path
                    && string.Equals(Path.GetFullPath(path), fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return (previewForm, page);
                }
            }
        }

        return null;
    }

    private void UpdateWindowTitle()
    {
        var page = _documentTabs.SelectedTab;
        Text = page is null || page.Tag is not string path
            ? "Markdown 渲染预览"
            : $"Markdown 预览 - {Path.GetFileName(path)}";
    }

    private void DocumentTabs_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _documentTabs.TabCount)
        {
            return;
        }

        var page = _documentTabs.TabPages[e.Index];
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var background = new SolidBrush(selected ? SystemColors.ControlLightLight : SystemColors.Control);
        e.Graphics.FillRectangle(background, e.Bounds);

        var closeRect = GetCloseButtonBounds(e.Bounds);
        _closeButtonBounds[page] = closeRect;
        var textRect = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top + 2,
            closeRect.Left - e.Bounds.Left - 10, e.Bounds.Height - 4);
        TextRenderer.DrawText(e.Graphics, page.Text, e.Font, textRect,
            selected ? SystemColors.ControlText : SystemColors.GrayText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(e.Graphics, "×", e.Font, closeRect,
            selected ? SystemColors.ControlText : SystemColors.GrayText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static Rectangle GetCloseButtonBounds(Rectangle tabBounds) => new(
        tabBounds.Right - CloseButtonSize - 5,
        tabBounds.Top + (tabBounds.Height - CloseButtonSize) / 2,
        CloseButtonSize,
        CloseButtonSize);

    private void DocumentTabs_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var index = HitTab(e.Location);
        _dragTab = index >= 0 ? _documentTabs.TabPages[index] : null;
        _dragStart = e.Location;
        if (_dragTab is not null && _closeButtonBounds.TryGetValue(_dragTab, out var closeBounds)
            && closeBounds.Contains(e.Location))
        {
            _dragTab = null;
        }
    }

    private void DocumentTabs_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragTab is null || e.Button != MouseButtons.Left
            || (Math.Abs(e.X - _dragStart.X) < TabDragThreshold && Math.Abs(e.Y - _dragStart.Y) < TabDragThreshold))
        {
            return;
        }

        var targetIndex = HitTab(e.Location);
        var oldIndex = _documentTabs.TabPages.IndexOf(_dragTab);
        if (targetIndex < 0 || oldIndex < 0 || targetIndex == oldIndex)
        {
            return;
        }

        _documentTabs.TabPages.Remove(_dragTab);
        _documentTabs.TabPages.Insert(targetIndex, _dragTab);
        _documentTabs.SelectedTab = _dragTab;
        _dragStart = e.Location;
    }

    private void DocumentTabs_MouseUp(object? sender, MouseEventArgs e) => _dragTab = null;

    private void DocumentTabs_MouseClick(object? sender, MouseEventArgs e)
    {
        var index = HitTab(e.Location);
        if (index < 0)
        {
            return;
        }

        var page = _documentTabs.TabPages[index];
        if (e.Button == MouseButtons.Left)
        {
            if (_closeButtonBounds.TryGetValue(page, out var closeBounds) && closeBounds.Contains(e.Location))
            {
                CloseTab(page);
            }

            return;
        }

        if (e.Button == MouseButtons.Right)
        {
            _documentTabs.SelectedTab = page;
            var menu = new ContextMenuStrip();
            menu.Items.Add("关闭标签", null, (_, _) => CloseTab(page));
            menu.Items.Add("关闭其他标签", null, (_, _) => CloseOtherTabs(page));
            menu.Show(_documentTabs, e.Location);
        }
    }

    private int HitTab(Point point)
    {
        for (var i = 0; i < _documentTabs.TabCount; i++)
        {
            if (_documentTabs.GetTabRect(i).Contains(point))
            {
                return i;
            }
        }

        return -1;
    }

    private void CloseTab(TabPage page)
    {
        if (!_documentTabs.TabPages.Contains(page))
        {
            return;
        }

        _closeButtonBounds.Remove(page);
        _documentTabs.TabPages.Remove(page);
        page.Dispose();
        if (_documentTabs.TabCount == 0)
        {
            Close();
        }
        else
        {
            UpdateWindowTitle();
        }
    }

    private void CloseOtherTabs(TabPage keep)
    {
        foreach (TabPage page in _documentTabs.TabPages.Cast<TabPage>().Where(page => page != keep).ToArray())
        {
            _closeButtonBounds.Remove(page);
            _documentTabs.TabPages.Remove(page);
            page.Dispose();
        }

        _documentTabs.SelectedTab = keep;
        UpdateWindowTitle();
    }

    protected override void WndProc(ref Message m)
    {
        // 仅由标题栏拖动触发合并；调整窗口大小不会触发合并。
        if (m.Msg == WmNcLButtonDown && m.WParam.ToInt32() == HtCaption)
        {
            _captionDragCandidate = true;
        }

        if (m.Msg == WmEnterSizeMove)
        {
            _windowMoveInProgress = _captionDragCandidate;
        }

        base.WndProc(ref m);

        if (m.Msg == WmExitSizeMove)
        {
            var shouldCheckMerge = _windowMoveInProgress && _captionDragCandidate;
            _windowMoveInProgress = false;
            _captionDragCandidate = false;
            if (shouldCheckMerge && !_merging && WindowState == FormWindowState.Normal)
            {
                MergeWithOverlappedPreviewWindow();
            }
        }
    }

    private void MergeWithOverlappedPreviewWindow()
    {
        var target = Application.OpenForms.OfType<MarkdownPreviewForm>()
            .Where(form => form != this && !form.IsDisposed && form.Visible
                && form.WindowState == FormWindowState.Normal)
            .Select(form => new { Form = form, Overlap = Rectangle.Intersect(Bounds, form.Bounds) })
            .Where(candidate => IsIntentionalOverlap(candidate.Overlap, Bounds, candidate.Form.Bounds))
            .OrderByDescending(candidate => (long)candidate.Overlap.Width * candidate.Overlap.Height)
            .Select(candidate => candidate.Form)
            .FirstOrDefault();

        target?.AbsorbTabs(this);
    }

    private static bool IsIntentionalOverlap(Rectangle overlap, Rectangle first, Rectangle second)
    {
        if (overlap.Width < MinimumMergeWidth || overlap.Height < MinimumMergeHeight)
        {
            return false;
        }

        var overlapArea = (long)overlap.Width * overlap.Height;
        var smallerWindowArea = Math.Min((long)first.Width * first.Height, (long)second.Width * second.Height);
        return smallerWindowArea > 0 && overlapArea >= smallerWindowArea * MinimumOverlapRatio;
    }

    private void AbsorbTabs(MarkdownPreviewForm source)
    {
        if (source._merging || IsDisposed || source.IsDisposed)
        {
            return;
        }

        source._merging = true;
        foreach (TabPage page in source._documentTabs.TabPages.Cast<TabPage>().ToArray())
        {
            var duplicate = page.Tag is string path
                ? _documentTabs.TabPages.Cast<TabPage>().FirstOrDefault(existing => existing.Tag is string existingPath
                    && string.Equals(Path.GetFullPath(existingPath), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                : null;

            source._documentTabs.TabPages.Remove(page);
            if (duplicate is null)
            {
                source._closeButtonBounds.Remove(page);
                _documentTabs.TabPages.Add(page);
                _documentTabs.SelectedTab = page;
            }
            else
            {
                page.Dispose();
            }
        }

        UpdateWindowTitle();
        source.Close();
        RestoreAndActivate();
    }

    private void btnOpen_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Markdown 文件 (*.md)|*.md|所有文件 (*.*)|*.*",
            Title = "选择要预览的 Markdown 文件",
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
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        if (_documentTabs.SelectedTab?.Tag is string path && File.Exists(path)
            && _documentTabs.SelectedTab.Controls.OfType<MarkdownPreviewControl>().FirstOrDefault() is { } view)
        {
            view.LoadMarkdownFile(path);
        }
    }
}
