namespace MonitorBrightnessSensor;

sealed class SettingsForm : Form
{
    public SettingsForm(Settings s)
    {
        Text = "Einstellungen";
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
        var luxMax = Num(s.LuxForMax, 2, 100000);
        var hyst = Num(s.Hysteresis, 1, 50);
        var gamma = new NumericUpDown { Minimum = 0.2m, Maximum = 1.0m, DecimalPlaces = 2, Increment = 0.05m, Value = (decimal)Math.Clamp(s.Gamma, 0.2, 1.0), Width = 80 };
        var interval = Num(s.MinWriteIntervalSeconds, 1, 600);

        Row(table, "Sensor (Host/IP)", host);
        Row(table, "Min. Helligkeit (%)", min);
        Row(table, "Max. Helligkeit (%)", max);
        Row(table, "Lux für Maximum", luxMax);
        Row(table, "Hysterese (%)", hyst);
        Row(table, "Kurve (1 = linear, kleiner = heller)", gamma);
        Row(table, "Min. Pause zw. Änderungen (s)", interval);

        var ok = new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true };
        ok.Click += (_, _) =>
        {
            s.SensorHost = host.Text.Trim();
            s.MinBrightness = (int)min.Value;
            s.MaxBrightness = Math.Max((int)max.Value, (int)min.Value);
            s.LuxForMax = (int)luxMax.Value;
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

    static void Row(TableLayoutPanel t, string label, Control c)
    {
        int row = t.RowCount++;
        t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 6) }, 0, row);
        t.Controls.Add(c, 1, row);
    }
}
