# MonitorBrightnessSensorWin

A small Windows tray app that reads the [Lunar ambient light sensor](https://lunar.fyi/sensor) (ESP32 + TSL2591, ESPHome)
and adjusts the brightness of external monitors via DDC/CI, even with the laptop lid closed.

## How it works

- Sensor: `http://lunarsensor.local/events` (Server-Sent Events, lux values; event id `sensor/Ambient Light`, or `sensor-ambient_light` on older firmware). Host/IP is configurable.
- Monitors: Windows Monitor Configuration API (`dxva2.dll`, VCP code `0x10`), no custom DDC protocol.
  All DDC-capable monitors are controlled; the built-in laptop display is skipped automatically.
- Curve: `t = ((lux − LuxForMin) / (LuxForMax − LuxForMin))^Gamma`, from *Min* (up to "Lux for minimum", default 20) to *Max* (from "Lux for maximum" upwards); gamma 1 = linear,
  default 0.5. In addition there is smoothing, hysteresis and a minimum pause between writes (default 10 s)
  to protect the monitor's memory.
- Language: the UI is English, or German if the Windows display language is German (texts live in `src/Strings.cs`).
- Settings: `%AppData%\MonitorBrightnessSensor\settings.json`, or the dialog in the tray menu.

## Building

Without a local development environment: GitHub Actions builds a self-contained `MonitorBrightnessSensor.exe` on every push
(Actions → latest run → artifact). Pushing a `v*` tag additionally attaches it to a release:

```bash
git tag v0.1.0 && git push origin v0.1.0
```

Locally (Mac/Windows/Linux with the .NET 8 SDK):

```bash
dotnet publish src/MonitorBrightnessSensor.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Notes

- The exe is unsigned; SmartScreen/Defender may warn.
- Corporate VPNs sometimes block `.local` name resolution or access to the local network. In that case, give the sensor a fixed IP address.
