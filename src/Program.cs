namespace MonitorBrightnessSensor;

static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, "MonitorBrightnessSensor.SingleInstance", out bool isNew);
        if (!isNew) return;

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayContext());
    }
}
