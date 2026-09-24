using System;
using MarkdownPreview.Handler;

namespace MarkdownPreview.Register;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("用法: MarkdownPreview.Register.exe install|uninstall");
            return 1;
        }

        try
        {
            switch (args[0].Trim().ToLowerInvariant())
            {
                case "install":
                    HandlerRegistration.Install();
                    Console.WriteLine("OK: installed");
                    return 0;
                case "uninstall":
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
}
