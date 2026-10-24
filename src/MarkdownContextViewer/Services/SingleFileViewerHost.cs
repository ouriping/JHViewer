using System.IO.Pipes;
using System.Text;
using MarkdownPreview.Shell;

namespace MarkdownContextViewer;

/// <summary>
/// 单文件查看进程：只显示当前文档窗口，不创建 <see cref="FrmMain"/>，也不占用主界面单实例通道。
/// </summary>
internal static class SingleFileViewerHost
{
    public const string MutexName = @"Local\JHViewer.SingleFileViewer";
    public const string PipeName = "JHViewer.SingleFileViewer";

    public static void Run(IReadOnlyList<string> files)
    {
        if (files.Count == 0)
        {
            MessageBox.Show(
                "请指定要查看的 .md、.markdown、.json 或 .xml 文件。",
                "JHViewer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Mutex? mutex = null;
        var ownsMutex = TryAcquireMutex(out mutex);
        if (!ownsMutex && TryForward(files))
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        try
        {
            var context = new ViewerApplicationContext();
            if (!context.OpenFiles(files))
            {
                context.DisposeAnchor();
                return;
            }

            if (ownsMutex)
            {
                StartServer(paths => context.OpenFiles(paths), cts.Token);
            }

            Application.Run(context);
        }
        finally
        {
            cts.Cancel();
            mutex?.Dispose();
        }
    }

    private static bool TryAcquireMutex(out Mutex? mutex)
    {
        try
        {
            mutex = new Mutex(true, MutexName, out var createdNew);
            if (createdNew)
            {
                return true;
            }

            mutex.Dispose();
            mutex = null;
            return false;
        }
        catch (AbandonedMutexException ex)
        {
            mutex = ex.Mutex;
            return mutex is not null;
        }
    }

    private static bool TryForward(IReadOnlyList<string> files)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000);
            using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
            foreach (var file in files)
            {
                writer.WriteLine(file);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void StartServer(Action<string[]> onPaths, CancellationToken cancellationToken)
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

                    if (paths.Count > 0)
                    {
                        onPaths(paths.ToArray());
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
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

    private sealed class ViewerApplicationContext : ApplicationContext
    {
        private readonly Dictionary<string, Form> _forms = new(StringComparer.OrdinalIgnoreCase);
        private readonly Form _anchor = new()
        {
            ShowInTaskbar = false,
            FormBorderStyle = FormBorderStyle.None,
            Opacity = 0,
            Size = new Size(1, 1)
        };
        private int _open;
        private int _cascade;

        public ViewerApplicationContext()
        {
            // 创建句柄，供后续命名管道回调切回 UI 线程；窗口本身不显示。
            _ = _anchor.Handle;
        }

        public void DisposeAnchor()
        {
            if (!_anchor.IsDisposed)
            {
                _anchor.Dispose();
            }
        }

        public bool OpenFiles(IReadOnlyList<string> files)
        {
            if (_anchor.InvokeRequired)
            {
                var opened = false;
                _anchor.Invoke(() => opened = OpenFilesCore(files));
                return opened || _open > 0;
            }

            return OpenFilesCore(files);
        }

        protected override void ExitThreadCore()
        {
            DisposeAnchor();
            base.ExitThreadCore();
        }

        private bool OpenFilesCore(IReadOnlyList<string> files)
        {
            var openedAny = false;
            foreach (var raw in files)
            {
                var path = raw.Trim().Trim('"');
                if (path.Length == 0)
                {
                    continue;
                }

                if (!File.Exists(path))
                {
                    MessageBox.Show($"文件不存在：{path}", "JHViewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                var fullPath = Path.GetFullPath(path);
                if (_forms.TryGetValue(fullPath, out var existing) && !existing.IsDisposed)
                {
                    Activate(existing);
                    openedAny = true;
                    continue;
                }

                if (!TryCreateForm(fullPath, out var form))
                {
                    continue;
                }

                _forms[fullPath] = form;
                form.FormClosed += (_, _) =>
                {
                    _forms.Remove(fullPath);
                    _open--;
                    if (_open <= 0)
                    {
                        ExitThread();
                    }
                };

                if (_cascade > 0)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(80 + (_cascade * 28), 60 + (_cascade * 28));
                }
                else
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                }

                _cascade++;
                _open++;
                form.Show();
                Activate(form);
                openedAny = true;
            }

            return openedAny;
        }

        private static bool TryCreateForm(string fullPath, out Form form)
        {
            var ext = Path.GetExtension(fullPath);
            if (ext.Equals(".json", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var structured = new DocumentPreviewDemoForm();
                structured.UseSingleFileChrome();
                structured.LoadFile(fullPath);
                form = structured;
                return true;
            }

            if (LaunchRequest.IsViewerExtension(fullPath))
            {
                form = new MarkdownPreviewForm(fullPath)
                {
                    Text = $"JHViewer - {Path.GetFileName(fullPath)}"
                };
                return true;
            }

            MessageBox.Show(
                "单文件查看仅支持 .md、.markdown、.json、.xml。\n" + fullPath,
                "JHViewer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            form = null!;
            return false;
        }

        private static void Activate(Form form)
        {
            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            form.Show();
            form.Activate();
            form.BringToFront();
        }
    }
}
