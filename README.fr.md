# XRatio

[![GitHub](https://img.shields.io/badge/GitHub-Mac--Cipher-blue?style=for-the-badge&logo=github)](https://github.com/Mac-Cipher/XRatio)
![Plateforme](https://img.shields.io/badge/Plateforme-Windows-brightgreen?style=for-the-badge&logo=windows)
![Langage](https://img.shields.io/badge/Langage-C%23-purple?style=for-the-badge&logo=c-sharp)
![Interface](https://img.shields.io/badge/UI-Avalonia-8B5CF6?style=for-the-badge)
![Licence](https://img.shields.io/badge/Licence-GPL--3.0-green?style=for-the-badge)

XRatio est une application desktop tout-en-un pour piloter les ratios annoncés aux trackers BitTorrent. Elle réunit deux moteurs clairement séparés dans une interface Avalonia native, dense et conçue pour un usage quotidien :

- **Interception** : un proxy local HTTP/HTTPS inspiré de [RatioGhost](https://github.com/Mac-Cipher/RatioGhost) réécrit les annonces d’un vrai client torrent.
- **Simulation** : un moteur indépendant inspiré de [RatioMaster](https://github.com/Mac-Cipher/RatioMaster) charge un fichier `.torrent` et envoie des annonces avec des compteurs, vitesses et profils clients contrôlés.

<p align="center">
  <img src="docs/screenshots/overview-current.png" alt="Vue d’ensemble actuelle de XRatio" width="1000">
</p>

<p align="center">
  <a href="https://github.com/Mac-Cipher/XRatio/releases/latest"><strong>Télécharger la dernière version Windows</strong></a>
  ·
  <a href="README.md">Read this page in English</a>
</p>

XRatio est écrit en C#/.NET 10 et cible Windows en priorité. Le cœur et le proxy restent compilables sur Linux/macOS, sans prétendre à une validation desktop native sur ces systèmes.

## Interface

- **Overview** : état du proxy, torrents interceptés, simulations actives et upload reporté.
- **Interception** : compteurs réels/reportés, peers, statut et dernière annonce par info-hash.
- **Simulation** : import `.torrent`, tracker, profil client, vitesses, progression, start/stop/update/remove.
- **Activity** : événements du proxy et des simulations.
- **Settings** : choix de la langue, des thèmes et des accents, règles de ratio, journalisation, autostart, version installée et recherche de mises à jour GitHub.
- **Platform** : activation HTTPS avec consentement explicite et gestion de la CA locale.

Les simulations configurées sont enregistrées dans `%APPDATA%\XRatio\simulations.json` et restaurées arrêtées. Le mot de passe proxy n’est jamais persisté.

## Mode Interception

1. Lancez XRatio; le proxy local écoute par défaut sur `127.0.0.1:3773`.
2. Dans votre client torrent, configurez un proxy **HTTP** sur cette adresse et activez la résolution des noms d’hôtes via le proxy.
3. Appliquez le proxy aux communications avec les trackers, pas aux connexions pair-à-pair.
4. Ajustez les règles dans **Settings** puis utilisez **Interception** et **Activity** pour vérifier les annonces.

L’interception HTTPS reste bloquée tant que vous n’avez pas explicitement approuvé la CA de cette installation dans **Platform** et dans Windows. XRatio échoue en mode fermé si cette confiance manque.

## Mode Simulation

1. Ouvrez **Simulation** et cliquez sur **Choose .torrent**.
2. Choisissez un tracker HTTP/HTTPS contenu dans le torrent.
3. Sélectionnez l’un des 17 profils clients et réglez les vitesses, la progression initiale et la variation.
4. Cliquez sur **Add session** : aucun réseau n’est encore utilisé.
5. Sélectionnez la session puis cliquez sur **Start** pour envoyer `started`; **Update now** force une annonce, **Stop** envoie `stopped`.

Le lecteur `.torrent` est borné à 16 Mio, calcule le SHA-1 sur les octets exacts du dictionnaire `info` et accepte les torrents mono/multifichiers. Le transport conserve la validation TLS du système; il ne reprend pas le contournement de certificat de l’ancien RatioMaster.

## Compiler et tester

Prérequis : .NET SDK `10.0.302`.

```powershell
dotnet restore .\XRatio.slnx
dotnet build .\XRatio.slnx -c Release --disable-build-servers -m:1
dotnet test .\tests-dotnet\XRatio.Core.Tests\XRatio.Core.Tests.csproj -c Release
dotnet test .\tests-dotnet\XRatio.Proxy.Tests\XRatio.Proxy.Tests.csproj -c Release
dotnet test .\tests-dotnet\XRatio.Desktop.Tests\XRatio.Desktop.Tests.csproj -c Release
dotnet run --project .\src\XRatio.Desktop\XRatio.Desktop.csproj
```

Les tests qui lancent le qBittorrent installé sont opt-in :

```powershell
$env:XRATIO_RUN_QBITTORRENT_SMOKE='1'
dotnet test .\tests-dotnet\XRatio.Desktop.Tests\XRatio.Desktop.Tests.csproj -c Release
```

Ils utilisent un profil temporaire isolé. Ne les activez pas pendant une session qBittorrent importante.

## Package Windows

```powershell
.\scripts\package-win-x64.ps1
.\scripts\smoke-win-x64.ps1
```

Le package autonome est créé dans `artifacts\win-x64`, puis archivé dans `artifacts\XRatio-dotnet-win-x64.zip` avec checksums, licence et attributions.

## Architecture

- `src/XRatio.Core/Announcements` : réécriture et persistance inspirées de RatioGhost.
- `src/XRatio.Core/Torrents` : BEncode et métadonnées `.torrent`.
- `src/XRatio.Core/Simulation` : profils, compteurs, sessions, tracker et persistance.
- `src/XRatio.Proxy` : proxy HTTP/HTTPS asynchrone et journalisation masquée.
- `src/XRatio.Desktop` : interface Avalonia, tray, certificats et autostart.
- `tests-dotnet` : tests Core, Proxy et Desktop.

## Utilisation responsable et licence

Les trackers peuvent interdire la modification ou la simulation de statistiques. Utilisez XRatio uniquement sur des services et torrents pour lesquels vous êtes autorisé à le faire, conformément aux règles du tracker et à la loi applicable.

## Inspiration et provenance

Ce dépôt a été développé avec **OpenAI Codex**.

XRatio est une implémentation indépendante en .NET 10/Avalonia, inspirée par deux projets existants :

- **[RatioGhost](https://github.com/Mac-Cipher/RatioGhost)** : inspiration pour le workflow proxy/annonces, l’intégration locale, les certificats, le tray, le packaging et les limites de vérification.
- **[RatioMaster](https://github.com/Mac-Cipher/RatioMaster)** : inspiration pour le workflow de simulation `.torrent`, les sessions tracker, les profils clients, les compteurs, la variation de vitesse et le cycle de vie des annonces.

Le détail des attributions figure dans [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

XRatio est distribué sous GNU GPL v3. Voir [`license.txt`](license.txt) et [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).
