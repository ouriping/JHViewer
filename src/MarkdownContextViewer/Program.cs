namespace MarkdownContextViewer;

internal static class Program
{
    private static Mutex? _mutex;
    private static CancellationTokenSource? _ipcCts;

    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (!SingleInstanceService.TryAcquireMutex(out _mutex))
        {
            SingleInstanceService.ForwardArgsAndExit(args);
            return;
        }

        try
        {
            EditorPathStore.Save(Application.ExecutablePath);

            _ipcCts = new CancellationTokenSource();
            var form = new FrmMain(args);
            SingleInstanceService.StartServer(
                paths =>
                {
                    if (form.IsDisposed)
                    {
                        return;
                    }

                    form.BeginInvoke(new Action(() => form.OpenExternalFiles(paths)));
                },
                _ipcCts.Token);

            Application.ApplicationExit += (_, _) => ShutdownIpc();
            Application.Run(form);
        }
        finally
        {
            ShutdownIpc();
            _mutex?.Dispose();
            _mutex = null;
        }
    }

    private static void ShutdownIpc()
    {
        try
        {
            _ipcCts?.Cancel();
            _ipcCts?.Dispose();
        }
        catch
        {
        }

        _ipcCts = null;
    }
}
