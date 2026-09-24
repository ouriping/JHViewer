using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MarkdownPreview.Handler;

internal static class HandlerLog
{
    private static readonly object Sync = new object();

    private static readonly string[] LogPaths =
    {
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low",
            "MarkdownContextViewer",
            "preview-handler.log"),
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarkdownContextViewer",
            "preview-handler.log")
    };

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception ex = null)
    {
        Write("ERROR", ex == null ? message : message + " | " + ex);
    }

    private static void Write(string level, string message)
    {
        var line = string.Format(
            "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] [PID {2}] {3}{4}",
            DateTime.Now,
            level,
            Process.GetCurrentProcess().Id,
            message,
            Environment.NewLine);

        lock (Sync)
        {
            foreach (var logPath in LogPaths)
            {
                try
                {
                    var dir = Path.GetDirectoryName(logPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.AppendAllText(logPath, line, Encoding.UTF8);
                    return;
                }
                catch
                {
                    // try next
                }
            }
        }
    }
}
