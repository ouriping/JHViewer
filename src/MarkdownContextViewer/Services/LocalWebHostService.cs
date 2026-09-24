using System.Net;

namespace MarkdownContextViewer;

public sealed class LocalWebHostService : IDisposable
{
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private string? _rootPath;
    private int _port;

    public string Start(string rootPath, int port = 5088)
    {
        Directory.CreateDirectory(rootPath);

        if (_listener is not null && _listener.IsListening && _port == port && string.Equals(_rootPath, rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return $"http://127.0.0.1:{port}/";
        }

        Stop();

        _rootPath = rootPath;
        _port = port;
        _cts = new CancellationTokenSource();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        _listener.Start();

        _listenTask = Task.Run(() => ListenLoop(_cts.Token));

        return $"http://127.0.0.1:{port}/";
    }

    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            _listenTask?.Wait(1000);
        }
        catch
        {
            // ignore
        }
        finally
        {
            _listener = null;
            _cts?.Dispose();
            _cts = null;
            _listenTask = null;
            _rootPath = null;
            _port = 0;
        }
    }

    private async Task ListenLoop(CancellationToken cancellationToken)
    {
        if (_listener is null || _rootPath is null)
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext? context = null;
            try
            {
                context = await _listener.GetContextAsync();
                await HandleRequest(context, _rootPath);
            }
            catch when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                if (context is not null)
                {
                    try
                    {
                        context.Response.StatusCode = 500;
                        context.Response.Close();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }
    }

    private static async Task HandleRequest(HttpListenerContext context, string rootPath)
    {
        var raw = context.Request.Url?.AbsolutePath ?? "/";
        var relativePath = Uri.UnescapeDataString(raw.TrimStart('/'));
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = "index.html";
        }

        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
        var safeRoot = Path.GetFullPath(rootPath);

        if (!fullPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            context.Response.StatusCode = 404;
            context.Response.Close();
            return;
        }

        var bytes = await File.ReadAllBytesAsync(fullPath);
        context.Response.ContentType = GetContentType(fullPath);
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.OutputStream.Close();
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    public void Dispose()
    {
        Stop();
    }
}

