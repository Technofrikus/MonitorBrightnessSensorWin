namespace MonitorBrightnessSensor;

sealed class SettingsForm : Form
{
    readonly ToolTip _tips = new() { AutoPopDelay = 30000, InitialDelay = 300, ReshowDelay = 100 };

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
        var luxMin = Num(s.LuxForMin, 0, 100000);
        var luxMax = Num(s.LuxForMax, 2, 100000);
        var hyst = Num(s.Hysteresis, 1, 50);
        var gamma = new NumericUpDown { Minimum = 0.2m, Maximum = 1.0m, DecimalPlaces = 2, Increment = 0.05m, Value = (decimal)Math.Clamp(s.Gamma, 0.2, 1.0), Width = 80 };
        var interval = Num(s.MinWriteIntervalSeconds, 1, 600);

        Row(table, "Sensor (Host/IP)", host,
            "Adresse des Lunar-Lichtsensors im WLAN, z. B. lunarsensor.local oder eine feste IP (192.168.x.x).\nBei VPN-Problemen mit .local besser die IP eintragen.");
        Row(table, "Min. Helligkeit (%)", min,
            "Helligkeit des Monitors bei \"Lux für Minimum\" oder weniger.\nNiedrigster Wert, den die Automatik setzt.");
        Row(table, "Max. Helligkeit (%)", max,
            "Höchste Helligkeit, die die Automatik setzt.\nWird erreicht, sobald der Sensor \"Lux für Maximum\" oder mehr misst.");
        Row(table, "Lux für Minimum", luxMin,
            "Umgebungshelligkeit in Lux, bei der (und darunter) die minimale Helligkeit gesetzt wird.\nEin Raum ist nie völlig dunkel, daher z. B. 20 Lux, damit der Monitor im dunklen Zimmer wirklich auf Minimum geht.");
        Row(table, "Lux für Maximum", luxMax,
            "Umgebungshelligkeit in Lux, ab der der Monitor die maximale Helligkeit erreicht.\nRichtwerte: Wohnzimmer abends ca. 50–150, Büro ca. 300–500, helles Fenster ca. 1000+.");
        Row(table, "Hysterese (%)", hyst,
            "Die Helligkeit wird nur geändert, wenn der neue Zielwert mindestens so viele Prozentpunkte vom aktuellen abweicht.\nGrößere Werte bedeuten weniger Unruhe und weniger Schreibvorgänge am Monitor.");
        Row(table, "Kurve (1 = linear, kleiner = heller)", gamma,
            "Form der Kurve zwischen Lux und Helligkeit: Helligkeit = ((Lux − Lux für Min.) / (Lux für Max. − Lux für Min.)) ^ Kurve.\n1,0 = linear. Kleinere Werte (z. B. 0,5) machen den Monitor schon bei wenig Licht deutlich heller,\nwährend hohe Lux-Werte kaum noch etwas ändern.");
        Row(table, "Min. Pause zw. Änderungen (s)", interval,
            "Mindestabstand in Sekunden zwischen zwei Helligkeitsänderungen am Monitor.\nGrößere Werte schonen den Speicher des Monitors, die Reaktion auf Lichtwechsel wird dadurch träger.");

        var ok = new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true };
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
