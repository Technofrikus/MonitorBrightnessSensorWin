namespace MonitorBrightnessSensor;

public sealed record Status(string Text, double? Lux, int? Brightness);

/// <summary>
/// Turns sensor lux values into monitor brightness: logarithmic curve, smoothing,
/// hysteresis and stepwise transitions so DDC writes stay rare and the change is not jarring.
/// </summary>
public sealed class BrightnessController : IDisposable
{
    readonly Settings _s;
    readonly SensorClient _sensor;
    readonly CancellationTokenSource _cts = new();

    double? _smoothedLux;
    DateTime _lastSample = DateTime.MinValue;
    int _current = -1;
    DateTime _lastAttempt = DateTime.MinValue;

    public event Action<Status>? StatusChanged;

    public BrightnessController(Settings settings)
    {
        _s = settings;
        _sensor = new SensorClient(() => _s.SensorHost);
        _ = Task.Run(LoopAsync);
    }

    async Task LoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try { Tick(); }
            catch (Exception ex) { StatusChanged?.Invoke(new Status("Fehler: " + ex.Message, _smoothedLux, null)); }

            try { await Task.Delay(1000, _cts.Token); }
            catch (OperationCanceledException) { return; }
        }
    }

    void Tick()
    {
        if (_sensor.Lux is not double raw || (DateTime.UtcNow - _sensor.LastUpdateUtc) > TimeSpan.FromSeconds(60))
        {
            StatusChanged?.Invoke(new Status(
                _sensor.Error is { } e ? $"Sensor nicht erreichbar ({e})" : "Warte auf Sensor …", _smoothedLux, _current < 0 ? null : _current));
            return;
        }

        if (_sensor.LastUpdateUtc != _lastSample)
        {
            _lastSample = _sensor.LastUpdateUtc;
            _smoothedLux = _smoothedLux is double prev ? prev + 0.3 * (raw - prev) : raw;
        }
        var lux = _smoothedLux!.Value;

        if (!_s.Enabled)
        {
            StatusChanged?.Invoke(new Status("Pausiert", lux, _current < 0 ? null : _current));
            return;
        }

        int target = TargetFor(lux);
        int diff = target - _current;
        bool first = _current < 0;
        bool atEdge = (target == _s.MinBrightness || target == _s.MaxBrightness) && diff != 0;
        bool needed = first || Math.Abs(diff) >= Math.Max(1, _s.Hysteresis) || atEdge;

        string text = "OK";
        if (needed && (DateTime.UtcNow - _lastAttempt) >= TimeSpan.FromSeconds(1.5))
        {
            _lastAttempt = DateTime.UtcNow;
            int next = first ? target : _current + Math.Clamp(diff, -Math.Max(1, _s.MaxStep), Math.Max(1, _s.MaxStep));
            if (DdcMonitors.SetBrightnessPercent(next) > 0)
                _current = next;
            else
                text = "Kein DDC-Monitor gefunden";
        }

        StatusChanged?.Invoke(new Status(text, lux, _current < 0 ? null : _current));
    }

    int TargetFor(double lux)
    {
        int min = Math.Clamp(_s.MinBrightness, 0, 100);
        int max = Math.Clamp(_s.MaxBrightness, min, 100);
        double luxMax = Math.Max(2, _s.LuxForMax);
        double t = Math.Clamp(Math.Log10(Math.Max(0, lux) + 1) / Math.Log10(luxMax + 1), 0, 1);
        return (int)Math.Round(min + (max - min) * t);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _sensor.Dispose();
    }
}
