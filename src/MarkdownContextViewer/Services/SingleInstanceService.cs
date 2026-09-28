using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace MarkdownContextViewer;

/// <summary>
/// 单实例：Mutex 互斥 + Named Pipe 把后续启动参数转发给已有窗口。
/// </summary>
internal static class SingleInstanceService
{
    public const string MutexName = @"Local\MarkdownContextViewer.SingleInstance";
    public const string PipeName = "MarkdownContextViewer.OpenFile";
    public const string ActivateToken = "__ACTIVATE__";

    private const int AsfwAny = -1;

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int processId);

    public static bool TryAcquireMutex(out Mutex mutex)
    {
        mutex = new Mutex(true, MutexName, out var createdNew);
        if (createdNew)
        {
            return true;
        }

        mutex.Dispose();
        mutex = null!;
        return false;
    }

    /// <summary>
    /// 第二实例：把参数发给已有实例后退出。
    /// </summary>
    public static void ForwardArgsAndExit(string[] args)
    {
        try
        {
            AllowSetForegroundWindow(AsfwAny);
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000);
            using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
            // 无文件参数时也通知主实例置前
            writer.WriteLine(ActivateToken);
            foreach (var arg in args)
            {
                var path = arg.Trim().Trim('"');
                if (path.Length > 0)
                {
                    writer.WriteLine(path);
                }
            }
        }
        catch
        {
            // 已有实例尚未就绪或已退出：忽略，由用户再次点击即可。
        }
    }

    /// <summary>
    /// 主实例：后台监听打开请求。
    /// </summary>
    public static void StartServer(Action<string[]> onPaths, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                    using var reader = new StreamReader(server, Encoding.UTF8);
                    var paths = new List<string>();
                    while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
                    {
                        var path = line.Trim().Trim('"');
                        if (path.Length > 0)
                        {
                            paths.Add(path);
                        }
                    }

                    onPaths(paths.ToArray());
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // 单次连接失败后继续听下一次。
                    try
                    {
                        await Task.Delay(200, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }, cancellationToken);
    }
}
