namespace MonitorBrightnessSensor;

sealed class SettingsForm : Form
{
    readonly ToolTip _tips = new() { AutoPopDelay = 30000, InitialDelay = 300, ReshowDelay = 100 };

    public SettingsForm(Settings s)
    {
        Text = Strings.SettingsTitle;
        MinimumSize = new Size(360, 0);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var table = new TableLayoutPanel { AutoSize = true, ColumnCount = 2 };
        Controls.Add(table);

        var host = new TextBox { Text = s.SensorHost, Width = 180 };
        var min = Num(s.MinBrightness, 0, 100);
        var max = Num(s.MaxBrightness, 0, 100);
        var luxMin = Num(s.LuxForMin, 0, 100000);
        var luxMax = Num(s.LuxForMax, 2, 100000);
        var hyst = Num(s.Hysteresis, 1, 50);
        var gamma = new NumericUpDown { Minimum = 0.2m, Maximum = 1.0m, DecimalPlaces = 2, Increment = 0.05m, Value = (decimal)Math.Clamp(s.Gamma, 0.2, 1.0), Width = 80 };
        var interval = Num(s.MinWriteIntervalSeconds, 1, 600);

        Row(table, Strings.SensorLabel, host, Strings.SensorTip);
        Row(table, Strings.MinBrightnessLabel, min, Strings.MinBrightnessTip);
        Row(table, Strings.MaxBrightnessLabel, max, Strings.MaxBrightnessTip);
        Row(table, Strings.LuxMinLabel, luxMin, Strings.LuxMinTip);
        Row(table, Strings.LuxMaxLabel, luxMax, Strings.LuxMaxTip);
        Row(table, Strings.HysteresisLabel, hyst, Strings.HysteresisTip);
        Row(table, Strings.GammaLabel, gamma, Strings.GammaTip);
        Row(table, Strings.IntervalLabel, interval, Strings.IntervalTip);

        var ok = new Button { Text = Strings.Save, DialogResult = DialogResult.OK, AutoSize = true };
        ok.Click += (_, _) =>
        {
            s.SensorHost = host.Text.Trim();
            s.MinBrightness = (int)min.Value;
            s.MaxBrightness = Math.Max((int)max.Value, (int)min.Value);
            s.LuxForMin = (int)luxMin.Value;
            s.LuxForMax = Math.Max((int)luxMax.Value, (int)luxMin.Value + 1);
            s.Hysteresis = (int)hyst.Value;
            s.Gamma = (double)gamma.Value;
            s.MinWriteIntervalSeconds = (int)interval.Value;
            s.Save();
            Close();
        };
        AcceptButton = ok;
        table.Controls.Add(ok, 1, table.RowCount);
    }

    static NumericUpDown Num(int value, int min, int max) =>
        new() { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 80 };

    void Row(TableLayoutPanel t, string label, Control c, string tip)
    {
        int row = t.RowCount++;
        var l = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 6) };
        t.Controls.Add(l, 0, row);
        t.Controls.Add(c, 1, row);
        _tips.SetToolTip(l, tip);
        _tips.SetToolTip(c, tip);
    }
}
