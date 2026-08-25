# Third-party notices

XRatio combines and adapts code and behavior from these GNU GPL v3 projects:

- RatioGhost — https://github.com/ratioghost/ratioghost
- RatioMaster.NET — https://github.com/NikolayIT/RatioMaster.NET

The RatioGhost-derived portions include announce transformation, proxying, settings migration, platform services, certificate handling, tray integration, packaging and their tests.

The RatioMaster-derived behavior includes `.torrent` metadata handling, tracker-session simulation, client emulation profiles, counters, speed variation and announce lifecycle concepts. These parts were rewritten for .NET 10, strict TLS validation, asynchronous execution and automated testing.

XRatio remains licensed as a whole under GNU GPL v3. The complete license text is in `license.txt`.
