# Product

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

## Users

Les utilisateurs principaux sont des utilisateurs avancés de clients BitTorrent qui pilotent quotidiennement un proxy local d'interception et des sessions de simulation, souvent sur Windows.

## Product Purpose

XRatio réunit l'interception locale d'annonces de trackers et la simulation indépendante de sessions torrent dans une application desktop native. Le succès se mesure à la capacité de vérifier rapidement l'état du proxy, les annonces, les ratios et les erreurs, puis d'ajuster les réglages sans perdre le contexte.

## Positioning

Le produit sépare explicitement les données d'un client réel intercepté des sessions de simulation contrôlées, tout en exposant les deux moteurs dans une même surface de supervision.

## Operating Context

L'application est utilisée comme outil de supervision compact, avec un proxy local, des fichiers `.torrent`, des trackers HTTP/HTTPS, des journaux d'activité et une icône de zone de notification sous Windows.

## Capabilities and Constraints

- Onglets Overview, Interception, Simulation, Activity, Settings et Platform.
- Démarrage, pause et arrêt du proxy, sélection de port et réglages de réécriture.
- Import et restauration de simulations `.torrent`, profils client, compteurs, vitesses et actions start/update/stop/remove.
- Activation HTTPS soumise à un consentement explicite et à la confiance de la CA locale.
- Les fonctionnalités existantes, les protections de confidentialité, les libellés fonctionnels et la densité d'usage doivent rester opérationnels.
- L'interface doit proposer un thème clair ou sombre sélectionnable par l'utilisateur.

## Brand Commitments

Le nom XRatio, l'icône d'application existante et une interface desktop native Avalonia restent en place. La direction visuelle doit servir la supervision technique sans devenir décorative.

## Evidence on Hand

- `README.fr.md` décrit les workflows et les limites produit.
- `src/XRatio.Desktop/MainWindow.cs` contient la surface UI Avalonia existante.
- `assets/XRatio-app-icon-v5.png` et `.ico` sont les actifs d'identité existants.

## Product Principles

1. L'état opérationnel doit être lisible en quelques secondes.
2. Les actions critiques doivent être explicites et réversibles quand c'est possible.
3. Interception et simulation restent clairement séparées.
4. La densité sert la vérification, jamais le bruit visuel.
5. Le thème choisi ne doit pas changer la hiérarchie ni la compréhension.

## Accessibility & Inclusion

Les contrastes, le focus clavier, les états désactivés et les tailles de cible doivent rester lisibles sur les thèmes clair et sombre, avec les comportements natifs Avalonia conservés.
