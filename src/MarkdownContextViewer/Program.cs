namespace MarkdownContextViewer;

internal static class Program
{
    private static Mutex? _mutex;
    private static CancellationTokenSource? _ipcCts;

    /// <summary>
    /// 
    /// args参数：
    /// --view C:\path\to\file.md C:\path\to\file2.md
    /// --edit C:\path\to\file.md C:\path\to\file2.md
    /// --register
    /// --unregister
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        //args 格式化,转在字符串方便查看 
        //var argsStr = string.Join(", ", args);
        //MessageBox.Show(argsStr, "JHViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);

        var request = LaunchRequest.Parse(args);
        if (request.UnregisterAssociations)
        {
            RunAssociationCommand(register: false);
        }

        if (request.RegisterAssociations)
        {
            RunAssociationCommand(register: true);
        }

        if (request.ShouldOpenViewer)
        {
            //如果想在visual studio 里调试以下，在命令行参数里填入以下参数
            //--view "E:\Work2026\Lexus_Shanghai\LexusBatteryTrace2026\docs\使用文档\02-Web端使用手册.md" 
            SingleFileViewerHost.Run(request.ViewerFiles);
            return;
        }

        if (!request.ShouldOpenMain)
        {
            return;
        }

        var mainArgs = request.MainFiles.ToArray();
        if (!SingleInstanceService.TryAcquireMutex(out _mutex))
        {
            SingleInstanceService.ForwardArgsAndExit(mainArgs);
            return;
        }

        try
        {
            EditorPathStore.Save(Application.ExecutablePath);

            _ipcCts = new CancellationTokenSource();
            var form = new FrmMain(mainArgs);
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
    /// <summary>
    /// 运行关联命令
    /// </summary>
    /// <param name="register">true = 注册, false = 取消注册</param>
    private static void RunAssociationCommand(bool register)
    {
        try
        {
            var service = new ExplorerIntegrationService();
            if (register)
            {
                EditorPathStore.Save(Application.ExecutablePath);
                service.RegisterContextMenu(Application.ExecutablePath);
                MessageBox.Show(
                    "已为当前用户注册 .md / .markdown / .json / .xml。\n" +
                    "双击、右键「打开」和「用 JHViewer 打开」只显示单文件查看窗口，不会打开主界面。\n\n" +
                    "主界面请直接运行本程序。若 Windows「默认应用」仍指向其他程序，请在系统设置中改选 JHViewer。",
                    "JHViewer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                service.UnregisterContextMenu();
                MessageBox.Show(
                    "已取消 .md / .markdown / .json / .xml 的默认打开关联与右键菜单（当前用户）。",
                    "JHViewer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                (register ? "注册失败：" : "取消注册失败：") + ex.Message,
                "JHViewer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
