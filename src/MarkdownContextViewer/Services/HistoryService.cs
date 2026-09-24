using System.Text.Json;

namespace MarkdownContextViewer;

internal sealed class HistoryService
{
    private readonly string _historyFilePath;

    public HistoryService(string historyFilePath)
    {
        _historyFilePath = historyFilePath;
    }

    public List<string> LoadExistingPaths()
    {
        try
        {
            if (!File.Exists(_historyFilePath))
            {
                return [];
            }

            var json = File.ReadAllText(_historyFilePath);
            var list = JsonSerializer.Deserialize<List<string>>(json) ?? [];
            return list.Where(File.Exists).ToList();
        }
        catch
        {
            return [];
        }
    }

    public void Save(List<string> history)
    {
        var dir = Path.GetDirectoryName(_historyFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_historyFilePath, json);
    }

    public void AddRecent(List<string> history, string filePath, int maxCount = 200)
    {
        history.RemoveAll(p => string.Equals(p, filePath, StringComparison.OrdinalIgnoreCase));
        history.Insert(0, filePath);

        if (history.Count > maxCount)
        {
            history.RemoveRange(maxCount, history.Count - maxCount);
        }
    }
}

