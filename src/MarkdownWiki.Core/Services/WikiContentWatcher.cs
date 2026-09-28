namespace MarkdownWiki.Core.Services;

/// <summary>监听文档根变更并失效树缓存。</summary>
public sealed class WikiContentWatcher : IDisposable
{
    private readonly WikiTreeService _treeService;
    private readonly FileSystemWatcher _watcher;
    private readonly object _lock = new();
    private CancellationTokenSource? _debounceCts;

    public WikiContentWatcher(WikiPathGuard pathGuard, WikiTreeService treeService)
    {
        _treeService = treeService;
        Directory.CreateDirectory(pathGuard.ContentRoot);

        _watcher = new FileSystemWatcher(pathGuard.ContentRoot)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Created += OnChanged;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token);
                    _treeService.Invalidate();
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }, token);
        }
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;
        }
    }
}
