using System;
using System.Collections.Generic;
using System.Linq;
using MarkdownPreview.Handler;

namespace MarkdownPreview.Register;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("用法: MarkdownPreview.Register.exe install [扩展名...] | uninstall");
            Console.Error.WriteLine("示例: MarkdownPreview.Register.exe install .md .json .xml");
            return 1;
        }

        try
        {
            switch (args[0].Trim().ToLowerInvariant())
            {
                case "install":
                {
                    var extensions = args.Skip(1)
                        .Select(NormalizeExt)
                        .Where(x => x.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    if (extensions.Length == 0)
                    {
                        Console.WriteLine("安装 COM 预览处理程序（全部扩展）…");
                        HandlerRegistration.Install();
                    }
                    else
                    {
                        Console.WriteLine("安装 COM 预览处理程序…");
                        Console.WriteLine("关联扩展: " + string.Join(" ", extensions));
                        HandlerRegistration.Install(extensions);
                    }
                    Console.WriteLine("OK: installed");
                    return 0;
                }
                case "uninstall":
                    Console.WriteLine("卸载 COM 预览处理程序…");
                    HandlerRegistration.Uninstall();
                    Console.WriteLine("OK: uninstalled");
                    return 0;
                default:
                    Console.Error.WriteLine("未知命令: " + args[0]);
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 2;
        }
    }

    private static string NormalizeExt(string value)
    {
        var ext = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (ext.Length == 0)
        {
            return string.Empty;
        }

        if (!ext.StartsWith(".", StringComparison.Ordinal))
        {
            ext = "." + ext;
        }

        return ext;
    }
}
