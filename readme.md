# XRatio

XRatio is a .NET 10/Avalonia desktop application that combines two explicit BitTorrent tracker-ratio engines:

- **Interception** — the RatioGhost-derived local HTTP/HTTPS proxy rewrites announces from a real torrent client.
- **Simulation** — the RatioMaster-derived engine loads `.torrent` metadata and runs independent tracker sessions with controlled counters and client profiles.

The production desktop UI provides Overview, Interception, Simulation, Activity, Settings and Platform views. Simulation sessions persist under `%APPDATA%\XRatio` and are restored stopped. TLS certificate validation remains enabled for direct tracker simulation.

## Build and test

```powershell
dotnet restore .\XRatio.slnx
dotnet build .\XRatio.slnx -c Release --disable-build-servers -m:1
dotnet test .\XRatio.slnx -c Release --disable-build-servers -m:1
dotnet run --project .\src\XRatio.Desktop\XRatio.Desktop.csproj
```

Installed-qBittorrent smoke tests are disabled by default. Enable them explicitly with `XRATIO_RUN_QBITTORRENT_SMOKE=1` only when an isolated qBittorrent launch is acceptable.

Create and smoke-test the self-contained Windows package with:

```powershell
.\scripts\package-win-x64.ps1
.\scripts\smoke-win-x64.ps1
```

Trackers may prohibit altered or simulated statistics. Use XRatio only where you are authorized and follow tracker rules and applicable law.

Licensed under GNU GPL v3. See [`license.txt`](license.txt), [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md), and the [French documentation](README.fr.md).
