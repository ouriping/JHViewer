using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MarkdownPreview.Handler;
/// <summary>
/// 日志记录器：记录操作日志。日志文件保存在 %LocalAppData%\MarkdownContextViewer\preview-handler.log。
/// 如果 %LocalAppData%\Low 目录存在，也会记录到 %LocalAppData%\Low\MarkdownContextViewer\preview-handler.log。
/// 如果两个目录都不存在，也会记录到 %LocalAppData%\MarkdownContextViewer\preview-handler.log。
/// 如果记录失败，会尝试下一个目录。如果所有目录都失败，会记录到 %Local
/// </summary>
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
