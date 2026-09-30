# MonitorBrightnessSensorWin

Kleine Windows-Tray-App, die den [Lunar-Helligkeitssensor](https://lunar.fyi/sensor) (ESP32 + TSL2591, ESPHome)
ausliest und die Helligkeit externer Monitore per DDC/CI anpasst – auch bei zugeklapptem Laptop.

## Funktionsweise

- Sensor: `http://lunarsensor.local/events` (Server-Sent Events, Lux-Werte), Host/IP einstellbar.
- Monitor: Windows Monitor Configuration API (`dxva2.dll`, VCP-Code `0x10`) – kein eigenes DDC-Protokoll.
  Alle DDC-fähigen Monitore werden gesteuert, das interne Laptop-Display wird automatisch übersprungen.
- Kurve: logarithmisch von *Min* (Dunkelheit) bis *Max* (ab „Lux für Maximum“), mit Glättung,
  Hysterese (schont das EEPROM des Monitors) und schrittweisen Übergängen.
- Einstellungen: `%AppData%\MonitorBrightnessSensor\settings.json` bzw. Dialog im Tray-Menü.

## Bauen

Ohne lokale Entwicklungsumgebung: GitHub Actions baut bei jedem Push eine self-contained `MonitorBrightnessSensor.exe`
(Actions → letzter Lauf → Artifact). Bei einem Tag `v*` wird sie zusätzlich an ein Release gehängt:

```bash
git tag v0.1.0 && git push origin v0.1.0
```

Lokal (Mac/Windows/Linux mit .NET 8 SDK):

```bash
dotnet publish src/MonitorBrightnessSensor.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Hinweise

- Die exe ist unsigniert; SmartScreen/Defender können warnen.
- Firmen-VPNs blockieren teils `.local`-Auflösung oder das lokale Netz – dann dem Sensor eine feste IP geben.
