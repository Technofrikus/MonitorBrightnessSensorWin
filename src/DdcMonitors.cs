using System.Runtime.InteropServices;

namespace MonitorBrightnessSensor;

/// <summary>
/// Sets the brightness of all DDC/CI capable monitors using the Windows Monitor Configuration API (dxva2).
/// Internal laptop panels do not answer DDC/CI and are skipped automatically, so this also works
/// with the lid closed and only an external monitor active.
/// </summary>
public static class DdcMonitors
{
    const byte VcpBrightness = 0x10;

    /// <summary>Sets brightness in percent on every DDC monitor. Returns the number of monitors that accepted it.</summary>
    public static int SetBrightnessPercent(int percent)
    {
        percent = Math.Clamp(percent, 0, 100);
        int ok = 0;

        var handles = EnumeratePhysicalMonitors(out var all);
        foreach (var handle in handles)
        {
            if (!GetVCPFeatureAndVCPFeatureReply(handle, VcpBrightness, out _, out _, out var max) || max == 0)
                continue;
            var value = (uint)Math.Round(percent / 100.0 * max);
            if (SetVCPFeature(handle, VcpBrightness, value))
                ok++;
        }

        DestroyPhysicalMonitors((uint)all.Length, all);
        return ok;
    }

    /// <summary>Current brightness in percent of the first DDC monitor, or null.</summary>
    public static int? GetBrightnessPercent()
    {
        int? result = null;
        var handles = EnumeratePhysicalMonitors(out var all);
        foreach (var handle in handles)
        {
            if (GetVCPFeatureAndVCPFeatureReply(handle, VcpBrightness, out _, out var cur, out var max) && max > 0)
            {
                result = (int)Math.Round(cur * 100.0 / max);
                break;
            }
        }
        DestroyPhysicalMonitors((uint)all.Length, all);
        return result;
    }

    static IntPtr[] EnumeratePhysicalMonitors(out PHYSICAL_MONITOR[] all)
    {
        var list = new List<PHYSICAL_MONITOR>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr _, IntPtr _, IntPtr _) =>
        {
            if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out var count) && count > 0)
            {
                var arr = new PHYSICAL_MONITOR[count];
                if (GetPhysicalMonitorsFromHMONITOR(hMonitor, count, arr))
                    list.AddRange(arr);
            }
            return true;
        }, IntPtr.Zero);

        all = list.ToArray();
        return all.Select(m => m.hPhysicalMonitor).ToArray();
    }

    delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, IntPtr lprc, IntPtr data);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szPhysicalMonitorDescription;
    }

    [DllImport("user32.dll")]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    [DllImport("dxva2.dll")]
    static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint count);

    [DllImport("dxva2.dll")]
    static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint count, [Out] PHYSICAL_MONITOR[] monitors);

    [DllImport("dxva2.dll")]
    static extern bool DestroyPhysicalMonitors(uint count, [In] PHYSICAL_MONITOR[] monitors);

    [DllImport("dxva2.dll")]
    static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte code, out uint type, out uint current, out uint max);

    [DllImport("dxva2.dll")]
    static extern bool SetVCPFeature(IntPtr hMonitor, byte code, uint value);
}
