using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MarkdownContextViewer;

internal static class ProcessIntegrity
{
    private const int TokenIntegrityLevel = 25;
    private const uint TokenQuery = 0x0008;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(
        IntPtr tokenHandle,
        int tokenInformationClass,
        IntPtr tokenInformation,
        int tokenInformationLength,
        out int returnLength);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ConvertSidToStringSid(IntPtr sid, out IntPtr stringSid);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);

    public static string GetLabel(int pid)
    {
        using var process = Process.GetProcessById(pid);
        if (!OpenProcessToken(process.Handle, TokenQuery, out var token))
        {
            return "未知";
        }

        try
        {
            GetTokenInformation(token, TokenIntegrityLevel, IntPtr.Zero, 0, out var length);
            if (length <= 0)
            {
                return "未知";
            }

            var buffer = Marshal.AllocHGlobal(length);
            try
            {
                if (!GetTokenInformation(token, TokenIntegrityLevel, buffer, length, out _))
                {
                    return "未知";
                }

                // TOKEN_MANDATORY_LABEL: SID starts after pointer-sized Attributes field on x64.
                var sid = Marshal.ReadIntPtr(buffer);
                if (!ConvertSidToStringSid(sid, out var stringSid) || stringSid == IntPtr.Zero)
                {
                    return "未知";
                }

                try
                {
                    var sidText = Marshal.PtrToStringUni(stringSid) ?? string.Empty;
                    return sidText switch
                    {
                        "S-1-16-4096" => "Low",
                        "S-1-16-8192" => "Medium",
                        "S-1-16-12288" => "High",
                        "S-1-16-16384" => "System",
                        _ => sidText
                    };
                }
                finally
                {
                    LocalFree(stringSid);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            CloseHandle(token);
        }
    }
}
