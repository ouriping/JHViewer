namespace MarkdownContextViewer;

/// <summary>
/// 解析启动参数：无参数进入主界面；文档路径或 <c>--view</c> 只打开单文件窗口；<c>--edit</c> 仍进入主界面。
/// </summary>
internal sealed class LaunchRequest
{
    public static readonly string[] ViewerExtensions = [".md", ".markdown", ".mdown", ".json", ".xml"];

    public bool EditInMainShell { get; private set; }

    public bool ViewRequested { get; private set; }

    public bool RegisterAssociations { get; private set; }

    public bool UnregisterAssociations { get; private set; }

    public List<string> ViewerFiles { get; } = [];

    public List<string> MainFiles { get; } = [];

    public bool ShouldOpenViewer =>
        !EditInMainShell && (ViewRequested || ViewerFiles.Count > 0);

    public bool ShouldOpenMain =>
        EditInMainShell
        || (!ViewRequested
            && ViewerFiles.Count == 0
            && !RegisterAssociations
            && !UnregisterAssociations);

    public static LaunchRequest Parse(string[] args)
    {
        var request = new LaunchRequest();
        var forceView = false;

        foreach (var raw in args)
        {
            var arg = raw.Trim().Trim('"');
            if (arg.Length == 0)
            {
                continue;
            }

            if (IsSwitch(arg, "edit") || IsSwitch(arg, "main"))
            {
                request.EditInMainShell = true;
                continue;
            }

            if (IsSwitch(arg, "view") || IsSwitch(arg, "viewer"))
            {
                forceView = true;
                request.ViewRequested = true;
                continue;
            }

            if (IsSwitch(arg, "register"))
            {
                request.RegisterAssociations = true;
                continue;
            }

            if (IsSwitch(arg, "unregister"))
            {
                request.UnregisterAssociations = true;
                continue;
            }

            if (arg.StartsWith('-') || arg.StartsWith('/'))
            {
                continue;
            }

            if (request.EditInMainShell)
            {
                request.MainFiles.Add(arg);
                continue;
            }

            if (forceView || IsViewerExtension(arg))
            {
                request.ViewerFiles.Add(arg);
            }
            else
            {
                request.MainFiles.Add(arg);
            }
        }

        if (request.EditInMainShell)
        {
            request.MainFiles.AddRange(request.ViewerFiles);
            request.ViewerFiles.Clear();
            request.ViewRequested = false;
        }
        else if (forceView)
        {
            request.ViewerFiles.AddRange(request.MainFiles);
            request.MainFiles.Clear();
        }

        return request;
    }

    public static bool IsViewerExtension(string path)
    {
        var ext = Path.GetExtension(path);
        foreach (var candidate in ViewerExtensions)
        {
            if (ext.Equals(candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSwitch(string arg, string name)
    {
        return arg.Equals("--" + name, StringComparison.OrdinalIgnoreCase)
            || arg.Equals("-" + name, StringComparison.OrdinalIgnoreCase)
            || arg.Equals("/" + name, StringComparison.OrdinalIgnoreCase);
    }
}
