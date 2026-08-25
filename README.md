# XRatio

[![GitHub](https://img.shields.io/badge/GitHub-Mac--Cipher-blue?style=for-the-badge&logo=github)](https://github.com/Mac-Cipher/XRatio)
![Platform](https://img.shields.io/badge/Platform-Windows-brightgreen?style=for-the-badge&logo=windows)
![Language](https://img.shields.io/badge/Language-C%23-purple?style=for-the-badge&logo=c-sharp)
![UI](https://img.shields.io/badge/UI-Avalonia-8B5CF6?style=for-the-badge)
![License](https://img.shields.io/badge/License-GPL--3.0-green?style=for-the-badge)

> A native control plane for tracker announce interception and controlled torrent simulation.

XRatio is a Windows-first .NET 10 / Avalonia desktop application for people who need a clear, local view of tracker announces. It brings two deliberately separate workflows into one compact tool:

- **Interception** — a local HTTP/HTTPS proxy inspired by [RatioGhost](https://github.com/ratioghost/ratioghost) sits between your torrent client and the tracker, changes the upload/download values sent in announces so the reported ratio can stay flat or increase, and leaves files and peer traffic untouched.
- **Simulation** — a standalone engine inspired by [RatioMaster.NET](https://github.com/NikolayIT/RatioMaster.NET) reads a `.torrent` file and sends controlled tracker announces with your chosen client profile, upload/download speeds, progress, and counters; it does not transfer the real torrent files.

<p align="center">
  <img src="docs/screenshots/overview-light.png" alt="XRatio current overview" width="1000">
</p>

<p align="center">
  <a href="https://github.com/Mac-Cipher/XRatio/releases/latest"><strong>Download the latest Windows build</strong></a>
  ·
  <a href="README.fr.md">Lire la documentation en français</a>
</p>

**Windows-first · Avalonia · .NET 10 · GPL-3.0**

## Install and configure a torrent client

### Install XRatio

1. Download `XRatio.exe` directly from [Releases](https://github.com/Mac-Cipher/XRatio/releases).
2. Start `XRatio.exe`; it is self-contained and does not require extraction or a separate .NET installation.
3. Confirm that the header shows the local proxy on `127.0.0.1:3773`.
4. Keep simulations stopped unless you intentionally want to run a separate controlled torrent session.

### Configure qBittorrent

The same settings are available in most clients that support an HTTP proxy. Stop active torrents before changing them.

1. Open **Tools > Options > Connection > Proxy Server**.
2. Choose **HTTP**, set the host to `127.0.0.1` and the port to `3773`.
3. Enable **Use proxy for BitTorrent purposes** and **Perform hostname lookup via proxy**.
4. Leave **Use proxy for peer connections** disabled. XRatio handles tracker announces only; payload and peer traffic stay on the normal client path.
5. Apply the settings, then start or manually update a torrent announce.
6. Use **Interception** and **Activity** in XRatio to confirm the announce and its counters.

For HTTPS trackers, open **Platform** in XRatio and explicitly trust the installation CA before enabling HTTPS interception. HTTP interception works without this step.

Example qBittorrent configuration:

<p align="center">
  <img src="assets/qbittorrent-proxy-settings.png" alt="qBittorrent proxy settings for XRatio" width="760">
</p>

## Product surface

| View | What it is for |
| --- | --- |
| **Overview** | See whether the proxy is running, how many torrents are tracked, how many simulations are active, and how much upload is reported. |
| **Interception** | Compare original and reported upload/download, ratio, peers, status, and last announce for each torrent. |
| **Simulation** | Load a `.torrent`, choose a tracker and client profile, set speeds and progress, then send controlled announces without transferring files. |
| **Activity** | Read proxy and simulation events as they happen. |
| **Settings** | Change language, theme, tray icon, ratio rules, logging, autostart, and update checks. |
| **Platform** | Enable startup/tray behavior and trust or remove the local CA for HTTPS tracker interception. |

## Design principles

- **One control plane, two paths.** Interception and simulation remain visibly separate so a real client session is never confused with an independent simulation.
- **Local and explicit.** The proxy is bound locally by default (`127.0.0.1:3773`), and HTTPS trust requires a deliberate confirmation.
- **Fail closed.** Direct tracker simulation keeps the operating system's TLS validation; XRatio does not copy the legacy accept-all certificate behavior.
- **Observable by default.** Counters, statuses, activity, and validation messages stay close to the action that produced them.

## Download and run

End users should use the packaged Windows build from [Releases](https://github.com/Mac-Cipher/XRatio/releases). The self-contained package does not require a separate .NET installation.

When running from source, the desktop app starts with:

```powershell
dotnet run --project .\src\XRatio.Desktop\XRatio.Desktop.csproj
```

The first launch keeps simulations stopped. Configured sessions are stored in `%APPDATA%\XRatio\simulations.json`; the proxy password is never persisted.

## Build, test, and package

Prerequisite: .NET SDK `10.0.302`.

```powershell
dotnet restore .\XRatio.slnx
dotnet build .\XRatio.slnx -c Release --disable-build-servers -m:1
dotnet test .\XRatio.slnx -c Release --disable-build-servers -m:1
```

Create and smoke-test the self-contained Windows package:

```powershell
.\scripts\package-win-x64.ps1
.\scripts\smoke-win-x64.ps1
```

Installed-qBittorrent smoke tests are opt-in. Enable them only when an isolated qBittorrent launch is acceptable:

```powershell
$env:XRATIO_RUN_QBITTORRENT_SMOKE='1'
dotnet test .\tests-dotnet\XRatio.Desktop.Tests\XRatio.Desktop.Tests.csproj -c Release
```

## Repository layout

```text
src/XRatio.Core        shared announce, torrent, simulation, and settings logic
src/XRatio.Proxy       local HTTP/HTTPS proxy and redacted activity logging
src/XRatio.Desktop     Avalonia desktop shell, tray, themes, localization, and platform trust
tests-dotnet            Core, Proxy, and Desktop test suites
scripts                 Windows packaging and smoke-test entry points
```

## Responsible use

Trackers can prohibit modified or simulated statistics. Use XRatio only with services and torrents for which you are authorized, and follow the tracker rules and applicable law. XRatio handles tracker announces only; it is not a peer-to-peer client and does not process payload traffic.

## Inspiration and provenance

This repository was developed with **OpenAI Codex**.

XRatio is an independent .NET 10/Avalonia implementation inspired by two existing projects:

- **[RatioGhost](https://github.com/ratioghost/ratioghost)** — inspiration for the announce/proxy workflow, local platform integration, certificates, tray behavior, packaging, and verification boundaries.
- **[RatioMaster.NET](https://github.com/NikolayIT/RatioMaster.NET)** — inspiration for the `.torrent` simulation workflow, tracker sessions, client profiles, counters, speed variation, and announce lifecycle.

The complete attribution is available in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## License

XRatio is distributed under the [GNU GPL v3](license.txt). Third-party attributions are listed in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
