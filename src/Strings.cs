using System.Runtime.InteropServices;

namespace MonitorBrightnessSensor;

/// <summary>
/// UI texts in English (default) and German. The language follows the Windows display language.
/// (InvariantGlobalization is on, so CultureInfo.CurrentUICulture is not usable here.)
/// </summary>
static class Strings
{
    [DllImport("kernel32.dll")]
    static extern ushort GetUserDefaultUILanguage();

    const int LangGerman = 0x07;

    static readonly bool De = DetectGerman();

    static bool DetectGerman()
    {
        try { return (GetUserDefaultUILanguage() & 0x3FF) == LangGerman; }
        catch { return false; }
    }

    static string T(string en, string de) => De ? de : en;

    // Tray menu
    public static string AutoEnabled => T("Automatic enabled", "Automatik aktiv");
    public static string StartWithWindows => T("Start with Windows", "Mit Windows starten");
    public static string SettingsMenu => T("Settings …", "Einstellungen …");
    public static string Exit => T("Exit", "Beenden");

    // Status
    public static string StatusError(string msg) => T("Error: " + msg, "Fehler: " + msg);
    public static string SensorUnreachable(string err) => T($"Sensor unreachable ({err})", $"Sensor nicht erreichbar ({err})");
    public static string WaitingForSensor => T("Waiting for sensor …", "Warte auf Sensor …");
    public static string Paused => T("Paused", "Pausiert");
    public static string NoDdcMonitor => T("No DDC monitor found", "Kein DDC-Monitor gefunden");

    // Settings dialog
    public static string SettingsTitle => T("Settings", "Einstellungen");
    public static string Save => T("Save", "Speichern");

    public static string SensorLabel => T("Sensor (host/IP)", "Sensor (Host/IP)");
    public static string SensorTip => T(
        "Address of the Lunar ambient light sensor on your Wi-Fi, e.g. lunarsensor.local or a fixed IP (192.168.x.x).\nIf .local does not work (e.g. with a VPN), enter the IP instead.",
        "Adresse des Lunar-Lichtsensors im WLAN, z. B. lunarsensor.local oder eine feste IP (192.168.x.x).\nBei VPN-Problemen mit .local besser die IP eintragen.");

    public static string MinBrightnessLabel => T("Min. brightness (%)", "Min. Helligkeit (%)");
    public static string MinBrightnessTip => T(
        "Monitor brightness at \"Lux for minimum\" or below.\nThe lowest value the automatic mode will set.",
        "Helligkeit des Monitors bei \"Lux für Minimum\" oder weniger.\nNiedrigster Wert, den die Automatik setzt.");

    public static string MaxBrightnessLabel => T("Max. brightness (%)", "Max. Helligkeit (%)");
    public static string MaxBrightnessTip => T(
        "The highest brightness the automatic mode will set.\nReached once the sensor measures \"Lux for maximum\" or more.",
        "Höchste Helligkeit, die die Automatik setzt.\nWird erreicht, sobald der Sensor \"Lux für Maximum\" oder mehr misst.");

    public static string LuxMinLabel => T("Lux for minimum", "Lux für Minimum");
    public static string LuxMinTip => T(
        "Ambient light in lux at (and below) which minimum brightness is set.\nA room is never completely dark, so use e.g. 20 lux to make the monitor really go to minimum in a dark room.",
        "Umgebungshelligkeit in Lux, bei der (und darunter) die minimale Helligkeit gesetzt wird.\nEin Raum ist nie völlig dunkel, daher z. B. 20 Lux, damit der Monitor im dunklen Zimmer wirklich auf Minimum geht.");

    public static string LuxMaxLabel => T("Lux for maximum", "Lux für Maximum");
    public static string LuxMaxTip => T(
        "Ambient light in lux from which the monitor reaches maximum brightness.\nRules of thumb: living room in the evening approx. 50–150, office approx. 300–500, bright window approx. 1000+.",
        "Umgebungshelligkeit in Lux, ab der der Monitor die maximale Helligkeit erreicht.\nRichtwerte: Wohnzimmer abends ca. 50–150, Büro ca. 300–500, helles Fenster ca. 1000+.");

    public static string HysteresisLabel => T("Hysteresis (%)", "Hysterese (%)");
    public static string HysteresisTip => T(
        "Brightness is only changed when the new target differs from the current value by at least this many percentage points.\nLarger values mean less flicker and fewer writes to the monitor.",
        "Die Helligkeit wird nur geändert, wenn der neue Zielwert mindestens so viele Prozentpunkte vom aktuellen abweicht.\nGrößere Werte bedeuten weniger Unruhe und weniger Schreibvorgänge am Monitor.");

    public static string GammaLabel => T("Curve (1 = linear, lower = brighter)", "Kurve (1 = linear, kleiner = heller)");
    public static string GammaTip => T(
        "Shape of the curve between lux and brightness: brightness = ((lux − lux for min.) / (lux for max. − lux for min.)) ^ curve.\n1.0 = linear. Smaller values (e.g. 0.5) make the monitor noticeably brighter even in dim light,\nwhile high lux values change little.",
        "Form der Kurve zwischen Lux und Helligkeit: Helligkeit = ((Lux − Lux für Min.) / (Lux für Max. − Lux für Min.)) ^ Kurve.\n1,0 = linear. Kleinere Werte (z. B. 0,5) machen den Monitor schon bei wenig Licht deutlich heller,\nwährend hohe Lux-Werte kaum noch etwas ändern.");

    public static string IntervalLabel => T("Min. pause between changes (s)", "Min. Pause zw. Änderungen (s)");
    public static string IntervalTip => T(
        "Minimum time in seconds between two brightness changes on the monitor.\nLarger values protect the monitor's memory but make the reaction to light changes slower.",
        "Mindestabstand in Sekunden zwischen zwei Helligkeitsänderungen am Monitor.\nGrößere Werte schonen den Speicher des Monitors, die Reaktion auf Lichtwechsel wird dadurch träger.");
}
