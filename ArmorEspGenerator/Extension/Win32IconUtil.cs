using System;
using System.Drawing;
using System.Runtime.InteropServices;

public static class Win32Icons
{
    [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

    public static Icon Extract(string file, int number, bool largeIcon)
    {
        IntPtr large; IntPtr small;
        ExtractIconEx(file, number, out large, out small, 1);
        try { return Icon.FromHandle(largeIcon ? large : small); }
        catch { return null; }
    }

    public static Icon GetShell32Icon(int index, bool largeIcon = true)
    {
        return Extract("shell32.dll", index, largeIcon);
    }
}
