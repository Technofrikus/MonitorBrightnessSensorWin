using System.Text.Json;

namespace MonitorBrightnessSensor;

public sealed class Settings
{
    /// <summary>Hostname or IP of the Lunar ambient light sensor (ESPHome).</summary>
    public string SensorHost { get; set; } = "lunarsensor.local";

    public bool Enabled { get; set; } = true;

    /// <summary>Monitor brightness (%) in complete darkness.</summary>
    public int MinBrightness { get; set; } = 10;

    /// <summary>Monitor brightness (%) at or above <see cref="LuxForMax"/>.</summary>
    public int MaxBrightness { get; set; } = 100;

    /// <summary>Lux value at which the maximum brightness is reached.</summary>
    public int LuxForMax { get; set; } = 400;

    /// <summary>Minimum brightness change (%) before the monitor is touched. Protects the monitor's EEPROM.</summary>
    public int Hysteresis { get; set; } = 3;

    /// <summary>Curve shape: 1.0 = linear, smaller = brighter already at low light (0.2 - 1.0).</summary>
    public double Gamma { get; set; } = 0.5;

    /// <summary>Minimum pause (s) between two brightness writes to the monitor.</summary>
    public int MinWriteIntervalSeconds { get; set; } = 10;

    static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonitorBrightnessSensor");
    static string FilePath => Path.Combine(Dir, "settings.json");

    public static Settings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
        }
        catch { /* fall back to defaults */ }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* not fatal */ }
    }
}
