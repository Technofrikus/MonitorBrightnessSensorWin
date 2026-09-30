using System.Drawing.Drawing2D;

namespace MonitorBrightnessSensor;

sealed class TrayContext : ApplicationContext
{
    readonly Settings _settings = Settings.Load();
    readonly BrightnessController _controller;
    readonly NotifyIcon _tray;
    readonly Control _ui = new();
    readonly ToolStripMenuItem _statusItem = new() { Enabled = false };
    readonly ToolStripMenuItem _enabledItem = new("Automatik aktiv") { CheckOnClick = true };
    readonly ToolStripMenuItem _autostartItem = new("Mit Windows starten") { CheckOnClick = true };

    public TrayContext()
    {
        _ = _ui.Handle; // force handle creation so BeginInvoke works from the worker thread

        _enabledItem.Checked = _settings.Enabled;
        _enabledItem.CheckedChanged += (_, _) => { _settings.Enabled = _enabledItem.Checked; _settings.Save(); };

        _autostartItem.Checked = Autostart.IsEnabled;
        _autostartItem.CheckedChanged += (_, _) => Autostart.Set(_autostartItem.Checked);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_enabledItem);
        menu.Items.Add("Einstellungen …", null, (_, _) => ShowSettings());
        menu.Items.Add(_autostartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Beenden", null, (_, _) => ExitThread());

        _tray = new NotifyIcon
        {
            Icon = CreateIcon(),
            Text = "Monitor Brightness Sensor",
            ContextMenuStrip = menu,
            Visible = true,
        };
        _tray.DoubleClick += (_, _) => ShowSettings();

        _controller = new BrightnessController(_settings);
        _controller.StatusChanged += s => _ui.BeginInvoke(() => ShowStatus(s));
    }

    void ShowStatus(Status s)
    {
        var lux = s.Lux is double l ? $"{l:0.#} lx" : "– lx";
        var br = s.Brightness is int b ? $"{b} %" : "– %";
        _statusItem.Text = $"{lux} → {br} · {s.Text}";
        var tip = $"{lux} → {br}";
        _tray.Text = tip.Length > 63 ? tip[..63] : tip;
    }

    SettingsForm? _form;

    void ShowSettings()
    {
        if (_form is { IsDisposed: false }) { _form.Activate(); return; }
        _form = new SettingsForm(_settings);
        _form.Show();
    }

    protected override void ExitThreadCore()
    {
        _tray.Visible = false;
        _tray.Dispose();
        _controller.Dispose();
        _ui.Dispose();
        base.ExitThreadCore();
    }

    static Icon CreateIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.Gold, 3);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;
                g.DrawLine(pen, 16 + (float)Math.Cos(a) * 10, 16 + (float)Math.Sin(a) * 10,
                                16 + (float)Math.Cos(a) * 15, 16 + (float)Math.Sin(a) * 15);
            }
            g.FillEllipse(Brushes.Gold, 8, 8, 16, 16);
        }
        return Icon.FromHandle(bmp.GetHicon());
    }
}
