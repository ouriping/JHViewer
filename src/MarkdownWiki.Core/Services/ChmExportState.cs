namespace MarkdownWiki.Core.Services;

/// <summary>记住最近一次 CHM 导出路径，供下载接口使用。</summary>
public sealed class ChmExportState
{
    private readonly object _lock = new();
    private string? _chmPath;
    private string? _buildDirectory;

    public void Set(string? chmPath, string? buildDirectory)
    {
        lock (_lock)
        {
            _chmPath = chmPath;
            _buildDirectory = buildDirectory;
        }
    }

    public (string? chmPath, string? buildDirectory) Get()
    {
        lock (_lock)
        {
            return (_chmPath, _buildDirectory);
        }
    }
}
