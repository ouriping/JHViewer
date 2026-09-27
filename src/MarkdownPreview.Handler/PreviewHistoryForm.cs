using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkdownPreview.Handler;

/// <summary>
/// 浏览记录管理：搜索、查看、用编辑器打开、移除。预览窗格太窄，不把整份列表塞进窗格。
/// </summary>
internal partial class PreviewHistoryForm : Form
{
    private readonly PreviewHistoryStore _store = null;
    private readonly Action<string> _onView = null;
    private List<string> _all = new List<string>();

    public PreviewHistoryForm()
    {
        InitializeComponent();
    }

    public PreviewHistoryForm(PreviewHistoryStore store, Action<string> onView)
        : this()
    {
        _store = store;
        _onView = onView;
    }

    private void PreviewHistoryForm_Load(object sender, EventArgs e) => Reload();

    private void txtFilter_TextChanged(object sender, EventArgs e) => ApplyFilter();

    private void lstHistory_DoubleClick(object sender, EventArgs e) => ViewSelected();

    private void btnView_Click(object sender, EventArgs e) => ViewSelected();

    private void btnEditor_Click(object sender, EventArgs e) => OpenSelectedInEditor();

    private void btnRemove_Click(object sender, EventArgs e) => RemoveSelected();

    private void btnRemoveMissing_Click(object sender, EventArgs e) => RemoveMissing();

    private void btnClear_Click(object sender, EventArgs e) => ClearAll();

    private void btnClose_Click(object sender, EventArgs e) => Close();

    private void Reload()
    {
        if (_store == null)
        {
            return;
        }

        _all = _store.Load();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var keyword = txtFilter.Text.Trim();
        var shown = string.IsNullOrEmpty(keyword)
            ? _all
            : _all.Where(p => p.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        lstHistory.BeginUpdate();
        lstHistory.Items.Clear();
        foreach (var path in shown)
        {
            lstHistory.Items.Add(path);
        }

        lstHistory.EndUpdate();
    }

    private string SelectedPath => lstHistory.SelectedItem as string;

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

        if (_onView != null)
        {
            _onView(path);
        }
    }

    private void OpenSelectedInEditor()
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

    private void RemoveSelected()
    {
        var path = SelectedPath;
        if (string.IsNullOrWhiteSpace(path) || _store == null)
        {
            return;
        }

        _store.Remove(path);
        Reload();
    }

    private void RemoveMissing()
    {
        if (_store == null)
        {
            return;
        }

        var removed = _store.RemoveMissing();
        Reload();
        MessageBox.Show(this, "已清除 " + removed + " 条无效记录。", "浏览记录", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearAll()
    {
        if (_store == null)
        {
            return;
        }

        if (MessageBox.Show(this, "清空全部浏览记录？", "浏览记录", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
        {
            return;
        }

        _store.Clear();
        Reload();
    }
}
