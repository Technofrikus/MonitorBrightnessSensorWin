using System.Text.Json;

namespace MonitorBrightnessSensor;

/// <summary>
/// Reads ambient light values (lux) from the ESPHome web server of the Lunar sensor
/// via Server-Sent Events (http://host/events). Reconnects automatically.
/// </summary>
public sealed class SensorClient : IDisposable
{
    readonly Func<string> _host;
    readonly CancellationTokenSource _cts = new();
    readonly HttpClient _http = new() { Timeout = Timeout.InfiniteTimeSpan };

    public double? Lux { get; private set; }
    public DateTime LastUpdateUtc { get; private set; } = DateTime.MinValue;
    public string? Error { get; private set; }

    public SensorClient(Func<string> host)
    {
        _host = host;
        _ = Task.Run(RunAsync);
    }

    async Task RunAsync()
    {
        var delay = TimeSpan.FromSeconds(2);
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                await ReadStreamAsync(_cts.Token);
            }
            catch (OperationCanceledException) when (_cts.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            try { await Task.Delay(delay, _cts.Token); }
            catch (OperationCanceledException) { return; }
            delay = delay < TimeSpan.FromSeconds(30) ? delay * 2 : delay;
        }
    }

    async Task ReadStreamAsync(CancellationToken ct)
    {
        var url = $"http://{_host().Trim()}/events";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        Error = null;

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (true)
        {
            // ESPHome sends a ping roughly every 10 s; no line for 45 s means the connection is dead.
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));
            var line = await reader.ReadLineAsync(timeout.Token);
            if (line == null) throw new IOException("Sensor closed the connection");
            if (line.StartsWith("data:", StringComparison.Ordinal))
                HandleData(line.AsSpan(5).Trim().ToString());
        }
    }

    void HandleData(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;
            if (!root.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String) return;
            // "sensor-ambient_light" (and "sensor-ambient_light_tsl2591" in older firmware);
            // not the infrared / full spectrum channels.
            if (!id.GetString()!.StartsWith("sensor-ambient_light", StringComparison.Ordinal)) return;
            if (!root.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Number) return;

            Lux = value.GetDouble();
            LastUpdateUtc = DateTime.UtcNow;
        }
        catch (JsonException) { /* ping or other non-JSON data */ }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _http.Dispose();
    }
}
