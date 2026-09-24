using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarkdownPreview.Handler;

/// <summary>
/// 与主程序共用 %LocalAppData%\MarkdownContextViewer\history.json。
/// </summary>
internal sealed class PreviewHistoryStore
{
    private const int MaxCount = 200;
    private readonly object _sync = new object();

    public string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MarkdownContextViewer",
        "history.json");

    public List<string> Load()
    {
        lock (_sync)
        {
            return Read();
        }
    }

    public void Add(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        lock (_sync)
        {
            var history = Read();
            history.RemoveAll(p => string.Equals(p, filePath, StringComparison.OrdinalIgnoreCase));
            history.Insert(0, filePath);
            if (history.Count > MaxCount)
            {
                history.RemoveRange(MaxCount, history.Count - MaxCount);
            }

            Write(history);
        }
    }

    public void Remove(string filePath)
    {
        lock (_sync)
        {
            var history = Read();
            history.RemoveAll(p => string.Equals(p, filePath, StringComparison.OrdinalIgnoreCase));
            Write(history);
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            Write(new List<string>());
        }
    }

    public int RemoveMissing()
    {
        lock (_sync)
        {
            var history = Read();
            var removed = history.RemoveAll(p => !File.Exists(p));
            if (removed > 0)
            {
                Write(history);
            }

            return removed;
        }
    }

    private List<string> Read()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new List<string>();
            }

            var json = File.ReadAllText(FilePath, Encoding.UTF8);
            if (!JsonStringList.TryParse(json, out var list))
            {
                HandlerLog.Error("history.json 无法解析，已忽略。");
                return new List<string>();
            }

            return list;
        }
        catch (Exception ex)
        {
            HandlerLog.Error("读取浏览记录失败。", ex);
            return new List<string>();
        }
    }

    private void Write(List<string> history)
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, JsonStringList.Format(history), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception ex)
        {
            HandlerLog.Error("保存浏览记录失败。", ex);
        }
    }
}
