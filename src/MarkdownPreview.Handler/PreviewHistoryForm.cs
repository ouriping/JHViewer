using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

/// <summary>
/// 浏览记录管理：搜索、查看、用编辑器打开、移除。预览窗格太窄，不把整份列表塞进窗格。
/// </summary>
internal sealed class PreviewHistoryForm : Form
{
    private readonly PreviewHistoryStore _store;
    private readonly Action<string> _onView;
    private readonly TextBox _filter = new TextBox();
    private readonly ListBox _list = new ListBox();
    private List<string> _all = new List<string>();

    public PreviewHistoryForm(PreviewHistoryStore store, Action<string> onView)
    {
        _store = store;
        _onView = onView;

        Text = "浏览记录";
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft YaHei UI", 9F);
        ClientSize = new Size(720, 460);
        MinimizeBox = false;
        ShowInTaskbar = false;

        var filterLabel = new Label
        {
            Text = "筛选",
            AutoSize = true,
            Location = new Point(12, 16)
        };
        _filter.Location = new Point(52, 12);
        _filter.Width = 640;
        _filter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _filter.TextChanged += (_, __) => ApplyFilter();

        _list.Location = new Point(12, 44);
        _list.Size = new Size(696, 360);
        _list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _list.HorizontalScrollbar = true;
        _list.IntegralHeight = false;
        _list.DoubleClick += (_, __) => ViewSelected();

        var view = MakeButton("查看", 12, ViewSelected);
        var editor = MakeButton("编辑器", 108, OpenSelectedInEditor);
        var remove = MakeButton("移除", 204, RemoveSelected);
        var missing = MakeButton("清除无效", 300, RemoveMissing);
        var clear = MakeButton("清空", 412, ClearAll);
        var close = MakeButton("关闭", 612, (_, __) => Close());
        close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        Controls.Add(filterLabel);
        Controls.Add(_filter);
        Controls.Add(_list);
        Controls.Add(view);
        Controls.Add(editor);
        Controls.Add(remove);
        Controls.Add(missing);
        Controls.Add(clear);
        Controls.Add(close);

        Load += (_, __) => Reload();
    }

    private Button MakeButton(string text, int left, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(left, 416),
            Size = new Size(88, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        button.Click += onClick;
        return button;
    }

    private void Reload()
    {
        _all = _store.Load();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var keyword = _filter.Text.Trim();
        var shown = string.IsNullOrEmpty(keyword)
            ? _all
            : _all.Where(p => p.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var path in shown)
        {
            _list.Items.Add(path);
        }

        _list.EndUpdate();
    }

    private string SelectedPath => _list.SelectedItem as string;

    private void ViewSelected(object sender, EventArgs e) => ViewSelected();

    private void ViewSelected()
    {
        var path = SelectedPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show(this, "文件已不存在。", "浏览记录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _onView(path);
    }

    private void OpenSelectedInEditor(object sender, EventArgs e)
    {
        var path = SelectedPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!EditorLauncher.TryLaunch(path, out var error))
        {
            MessageBox.Show(this, error, "浏览记录", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void RemoveSelected(object sender, EventArgs e)
    {
        var path = SelectedPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        _store.Remove(path);
        Reload();
    }

    private void RemoveMissing(object sender, EventArgs e)
    {
        var removed = _store.RemoveMissing();
        Reload();
        MessageBox.Show(this, $"已清除 {removed} 条无效记录。", "浏览记录", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearAll(object sender, EventArgs e)
    {
        if (MessageBox.Show(this, "清空全部浏览记录？", "浏览记录", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        _store.Clear();
        Reload();
    }
}
