namespace XRatio.Desktop;

internal static class UiText
{
    public const string English = "English";
    public const string French = "French";
    public const string Spanish = "Spanish";
    public const string German = "German";
    public const string Italian = "Italian";
    public const string Portuguese = "Portuguese";
    public const string Japanese = "Japanese";
    public const string Chinese = "Chinese";
    public const string Arabic = "Arabic";
    public const string Russian = "Russian";

    public static readonly IReadOnlyList<string> LanguageCodes =
    [
        English,
        French,
        Spanish,
        German,
        Italian,
        Portuguese,
        Japanese,
        Chinese,
        Arabic,
        Russian
    ];

    public static readonly IReadOnlyList<string> LanguageLabels =
    [
        "🇺🇸 English",
        "🇫🇷 Français",
        "🇪🇸 Español",
        "🇩🇪 Deutsch",
        "🇮🇹 Italiano",
        "🇵🇹 Português",
        "🇯🇵 日本語",
        "🇨🇳 中文",
        "🇸🇦 العربية",
        "🇷🇺 Русский"
    ];

    // Keep the display data separate from the emoji labels above. Avalonia can
    // render regional-indicator emoji as two letters on Windows, so the actual
    // ComboBox template draws these flag codes as small vector-like controls.
    public static readonly IReadOnlyList<string> LanguageFlagCodes =
    [
        "US", "FR", "ES", "DE", "IT", "PT", "JP", "CN", "SA", "RU"
    ];

    public static readonly IReadOnlyList<string> LanguageDisplayNames =
    [
        "English", "Français", "Español", "Deutsch", "Italiano", "Português", "日本語", "中文", "العربية", "Русский"
    ];

    private static readonly IReadOnlyDictionary<string, string> FrenchMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Anglais",
            ["French"] = "Français",
            ["Spanish"] = "Espagnol",
            ["German"] = "Allemand",
            ["Italian"] = "Italien",
            ["Portuguese"] = "Portugais",
            ["Japanese"] = "Japonais",
            ["Chinese"] = "Chinois",
            ["Arabic"] = "Arabe",
            ["Russian"] = "Russe",
            ["Overview"] = "Vue d’ensemble",
            ["Interception"] = "Interception",
            ["Simulation"] = "Simulation",
            ["Activity"] = "Activité",
            ["Settings"] = "Réglages",
            ["Platform"] = "Plateforme",
            ["Monitoring"] = "Supervision",
            ["Control"] = "Contrôle",
            ["System"] = "Système",
            ["Support"] = "Assistance",
            ["Guide"] = "Guide",
            ["LOCAL RATIO CONTROL"] = "CONTRÔLE RATIO LOCAL",
            ["LOCAL / MONITORING"] = "LOCAL / SUPERVISION",
            ["Loading configuration…"] = "Chargement de la configuration…",
            ["Start"] = "Démarrer",
            ["Stop"] = "Arrêter",
            ["▶  Start"] = "▶  Démarrer",
            ["■  Stop"] = "■  Arrêter",
            ["Retry"] = "Réessayer",
            ["Pause"] = "Pause",
            ["Resume"] = "Reprendre",
            ["Save changes"] = "Enregistrer",
            ["To tray"] = "Réduire dans la zone de notification",
            ["Close"] = "Fermer",
            ["Open Settings"] = "Ouvrir les réglages",
            ["Current runtime status."] = "État actuel du service.",
            ["Start or pause the proxy from the top bar; the overview updates as activity changes."] = "Démarrez ou mettez le proxy en pause depuis la barre supérieure ; la vue d’ensemble se met à jour avec l’activité.",
            ["The summary shows proxy state, tracked torrents, active versus configured simulations and reported upload."] = "Le résumé affiche l’état du proxy, les torrents suivis, les simulations actives et configurées, ainsi que l’upload annoncé.",
            ["XRatio listens locally and does not handle payload or peer traffic."] = "XRatio écoute localement et ne traite ni les payloads ni le trafic pair-à-pair.",
            ["A timestamped view of proxy, simulation and configuration events."] = "Journal horodaté du proxy, des simulations et de la configuration.",
            ["Time"] = "Heure",
            ["Level · source"] = "Niveau · source",
            ["Event details"] = "Détails de l’événement",
            ["Interception could not start"] = "L’interception n’a pas pu démarrer",
            ["Interception needs attention"] = "L’interception nécessite votre attention",
            ["PROXY CHANNEL"] = "CANAL PROXY",
            ["Local tracker interception · HTTP / HTTPS"] = "Interception locale des trackers · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrents suivis",
            ["Announcements observed"] = "Annonces observées",
            ["Simulations"] = "Simulations",
            ["Active / configured"] = "Actives / configurées",
            ["Reported upload"] = "Upload annoncé",
            ["Current session"] = "Session actuelle",
            ["OPERATING MODES"] = "MODES D’EXÉCUTION",
            ["Two paths, one local control plane."] = "Deux chemins, un seul plan de contrôle local.",
            ["Rewrite tracker announces from a real client through the local proxy."] = "Réécrit les annonces d’un vrai client via le proxy local.",
            ["Run independent .torrent sessions with controlled counters and rates."] = "Lance des sessions .torrent indépendantes aux compteurs et débits contrôlés.",
            ["LOCAL"] = "LOCAL",
            ["CONTROLLED"] = "CONTRÔLÉ",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Annonces de trackers uniquement — les payloads et le trafic pair-à-pair restent intacts.",
            ["Torrent file"] = "Fichier torrent",
            ["Select a .torrent file"] = "Sélectionnez un fichier .torrent",
            ["Browse…"] = "Parcourir…",
            ["Torrent info"] = "Informations du torrent",
            ["Account"] = "Compte",
            ["Tracker"] = "Tracker",
            ["Hash"] = "Hash",
            ["Size"] = "Taille",
            ["Info hash"] = "Hash info",
            ["Selected torrent"] = "Torrent sélectionné",
            ["Choose a torrent"] = "Choisir un torrent",
            ["Choose a .torrent file first."] = "Choisissez d’abord un fichier .torrent.",
            ["Choose a tracker."] = "Choisissez un tracker.",
            ["Choose a client profile."] = "Choisissez un profil client.",
            ["Completed percentage"] = "Pourcentage terminé",
            ["Upload rate"] = "Débit montant",
            ["Download rate"] = "Débit descendant",
            ["Minimum random upload"] = "Upload aléatoire minimum",
            ["Maximum random upload"] = "Upload aléatoire maximum",
            ["Minimum random download"] = "Download aléatoire minimum",
            ["Maximum random download"] = "Download aléatoire maximum",
            ["Speed options"] = "Options de débit",
            ["Upload speed (kB/s)"] = "Débit montant (kB/s)",
            ["Download speed (kB/s)"] = "Débit descendant (kB/s)",
            ["+ Random values"] = "+ Valeurs aléatoires",
            ["Options"] = "Options",
            ["Update interval (s)"] = "Intervalle de mise à jour (s)",
            ["Client"] = "Client",
            ["Finished (%)"] = "Terminé (%)",
            ["Never"] = "Jamais",
            ["After minutes"] = "Après un nombre de minutes",
            ["Uploaded MiB"] = "MiB envoyés",
            ["Downloaded MiB"] = "MiB reçus",
            ["Ratio"] = "Ratio",
            ["Uploaded"] = "Envoyé",
            ["Downloaded"] = "Reçu",
            ["Peers"] = "Pairs",
            ["Next announce"] = "Prochaine annonce",
            ["Running"] = "En cours",
            ["Starting"] = "Démarrage…",
            ["Stopping"] = "Arrêt en cours",
            ["Error"] = "Erreur",
            ["Stopped"] = "Arrêté",
            ["Value"] = "Valeur",
            ["Tracker identity"] = "Identité tracker",
            ["Listening port"] = "Port d’écoute",
            ["Peers requested"] = "Pairs demandés",
            ["Outbound proxy"] = "Proxy sortant",
            ["Proxy address"] = "Adresse du proxy",
            ["Proxy username"] = "Nom d’utilisateur du proxy",
            ["Optional"] = "Facultatif",
            ["Show full path and tracker URL"] = "Afficher le chemin complet et l’URL du tracker",
            ["Main"] = "Principal",
            ["Advanced"] = "Avancé",
            ["No simulation sessions"] = "Aucune session de simulation",
            ["Choose a .torrent, configure its announce profile, then add a session."] = "Choisissez un .torrent, configurez son profil d’annonce, puis ajoutez une session.",
            ["Add session"] = "Ajouter la session",
            ["Manual update"] = "Mise à jour manuelle",
            ["Remove…"] = "Supprimer…",
            ["The tracker is contacted only after the session is added and Start is pressed."] = "Le tracker n’est contacté qu’après l’ajout de la session et l’action Démarrer.",
            ["The account label stays local; the tracker name is read automatically from the announce URL."] = "Le libellé du compte reste local ; le nom du tracker est lu automatiquement depuis l’URL d’annonce.",
            ["Session added."] = "Session ajoutée.",
            ["Could not add session."] = "Impossible d’ajouter la session.",
            ["Already added — the existing session is selected."] = "Déjà ajoutée — la session existante est sélectionnée.",
            ["Simulation sessions"] = "Sessions de simulation",
            ["No tracked torrents yet"] = "Aucun torrent suivi",
            ["Tracker announcements will appear here automatically."] = "Les annonces des trackers apparaîtront ici automatiquement.",
            ["Select a row to access its available actions, including copying the info hash or resetting statistics."] = "Sélectionnez une ligne pour accéder aux actions disponibles, notamment copier le hash info ou réinitialiser les statistiques.",
            ["Hash · tracker · peers · status · transfer counters · last announce"] = "Hash · tracker · pairs · état · compteurs de transfert · dernière annonce",
            ["Tracked sessions stay visible here as announcements arrive."] = "Les sessions suivies restent visibles ici à mesure que les annonces arrivent.",
            ["Appearance"] = "Apparence",
            ["Updates"] = "Mises à jour",
            ["Check the official GitHub release without changing files automatically."] = "Vérifiez la release officielle GitHub sans modifier automatiquement les fichiers.",
            ["Current version"] = "Version actuelle",
            ["Check for updates"] = "Rechercher les mises à jour",
            ["Download update"] = "Télécharger la mise à jour",
            ["Not checked yet"] = "Pas encore vérifié",
            ["Checking for updates…"] = "Recherche de mises à jour…",
            ["You are up to date"] = "Vous utilisez la dernière version",
            ["Unable to check for updates"] = "Impossible de rechercher les mises à jour",
            ["Update available: {0}"] = "Mise à jour disponible : {0}",
            ["Could not open update download"] = "Impossible d’ouvrir le téléchargement de la mise à jour",
            ["Theme"] = "Thème",
            ["Tray icon"] = "Icône de notification",
            ["Light"] = "Clair",
            ["Dim"] = "Sombre doux",
            ["Soft Dark"] = "Sombre feutré",
            ["Dark"] = "Sombre",
            ["Accent color"] = "Couleur d’accentuation",
            ["Color"] = "Couleur",
            ["Monochrome"] = "Monochrome",
            ["Blue"] = "Bleu",
            ["Teal"] = "Turquoise",
            ["Violet"] = "Violet",
            ["Amber"] = "Ambre",
            ["Rose"] = "Rose",
            ["Green"] = "Vert",
            ["Language"] = "Langue",
            ["Choose the visual mode and signal color for the XRatio control plane. Blue is the default; the hierarchy stays the same in all themes."] = "Choisissez le mode visuel et la couleur de signal du plan de contrôle XRatio. Le bleu est utilisé par défaut ; la hiérarchie reste identique dans tous les thèmes.",
            ["Choose the language used by the XRatio interface."] = "Choisissez la langue de l’interface XRatio.",
            ["Color mode uses a red X when stopped and orange when paused; Monochrome keeps the whole icon neutral."] = "Le mode Couleur utilise une croix rouge à l’arrêt et orange en pause ; le mode Monochrome garde toute l’icône neutre.",
            ["Connection"] = "Connexion",
            ["Use a free localhost port from 1 to 65534. Minimum leechers must be between 0 and 100."] = "Utilisez un port localhost libre de 1 à 65534. Le nombre minimal de leechers doit être compris entre 0 et 100.",
            ["HTTP proxy port"] = "Port du proxy HTTP",
            ["Minimum leechers"] = "Leechers minimum",
            ["Accept tracker traffic only"] = "Accepter uniquement le trafic des trackers",
            ["Listen on localhost only (required)"] = "Écouter uniquement sur localhost (requis)",
            ["Write redacted proxy debug log"] = "Écrire le journal proxy anonymisé",
            ["Ratio shaping"] = "Réglage du ratio",
            ["Connection, ratio shaping and reporting options are grouped by purpose."] = "Les options de connexion, de réglage du ratio et de reporting sont regroupées par fonction.",
            ["Minimum values must not exceed maximum values. Multipliers and boost values cannot be negative."] = "Les minima ne doivent pas dépasser les maxima. Les multiplicateurs et le boost ne peuvent pas être négatifs.",
            ["Upload/download multiplier min"] = "Multiplicateur upload/download min",
            ["Upload/download multiplier max"] = "Multiplicateur upload/download max",
            ["Upload/upload multiplier min"] = "Multiplicateur upload/upload min",
            ["Upload/upload multiplier max"] = "Multiplicateur upload/upload max",
            ["Boost maximum (KiB/s)"] = "Boost maximum (KiB/s)",
            ["Boost chance (%)"] = "Chance de boost (%)",
            ["Boost chance"] = "Chance de boost",
            ["Update interval"] = "Intervalle de mise à jour",
            ["Rate is outside the supported range."] = "Le débit est en dehors de la plage prise en charge.",
            ["Announce behavior"] = "Comportement des annonces",
            ["Choose the information the proxy reports to trackers."] = "Choisissez les informations que le proxy annonce aux trackers.",
            ["Report download as zero"] = "Annoncer un téléchargement nul",
            ["Pretend to seed"] = "Simuler le seeding",
            ["Tune the proxy while keeping safe defaults."] = "Réglez le proxy en conservant des valeurs sûres.",
            ["Loading settings…"] = "Chargement des réglages…",
            ["Unsaved changes"] = "Modifications non enregistrées",
            ["All changes saved"] = "Toutes les modifications sont enregistrées",
            ["Configuration"] = "Configuration",
            ["Configuration saved."] = "Configuration enregistrée.",
            ["Configuration error"] = "Erreur de configuration",
            ["Copied info hash"] = "Hash info copié",
            ["Reset all tracked statistics"] = "Réinitialiser toutes les statistiques suivies",
            ["Remove simulation"] = "Supprimer la simulation",
            ["Remove trust"] = "Supprimer la confiance",
            ["Reset all tracked statistics for"] = "Réinitialiser toutes les statistiques suivies pour",
            ["Select a simulation session first."] = "Sélectionnez d’abord une session de simulation.",
            ["This exact simulation already exists; selected the existing session."] = "Cette simulation existe déjà ; la session existante est sélectionnée.",
            ["HTTPS interception enabled for the current Windows user."] = "Interception HTTPS activée pour l’utilisateur Windows actuel.",
            ["HTTPS was not enabled: explicit CA trust confirmation is required."] = "L’interception HTTPS n’a pas été activée : la confirmation explicite de confiance envers la CA est requise.",
            ["XRatio CA trust removed from the current Windows user."] = "La confiance envers la CA XRatio a été retirée pour l’utilisateur Windows actuel.",
            ["New entries are added as proxy decisions, imports and simulation actions happen."] = "Les nouvelles entrées apparaissent au fil des décisions du proxy, des imports et des actions de simulation.",
            ["Scroll to review recent events; the list keeps the latest 500 entries."] = "Faites défiler pour consulter les événements récents ; la liste conserve les 500 dernières entrées.",
            ["Startup"] = "Démarrage",
            ["Choose how XRatio should behave when your session begins."] = "Choisissez le comportement de XRatio au démarrage de votre session.",
            ["Start automatically with the user session"] = "Démarrer automatiquement avec la session utilisateur",
            ["Show icon in notification area"] = "Afficher l’icône dans la zone de notification",
            ["Start minimized to tray"] = "Démarrer réduit dans la zone de notification",
            ["HTTPS interception"] = "Interception HTTPS",
            ["The installation CA is used only to inspect HTTPS tracker traffic through the local proxy."] = "La CA d’installation sert uniquement à inspecter le trafic HTTPS des trackers via le proxy local.",
            ["Trust is explicit and scoped to the current Windows user."] = "La confiance est explicite et limitée à l’utilisateur Windows actuel.",
            ["I understand that XRatio will add its installation CA to my Windows user trust store."] = "Je comprends que XRatio va ajouter sa CA d’installation au magasin de confiance de mon utilisateur Windows.",
            ["Trust CA and enable"] = "Faire confiance à la CA et activer",
            ["Remove CA trust…"] = "Supprimer la confiance CA…",
            ["System integrations and HTTPS trust live here."] = "Les intégrations système et la confiance HTTPS sont gérées ici.",
            ["Configure whether XRatio starts with the user session and whether it opens minimized to the tray."] = "Configurez le démarrage de XRatio avec la session utilisateur et son ouverture réduite dans la zone de notification.",
            ["Review the platform capability text before enabling an integration."] = "Consultez les capacités de la plateforme avant d’activer une intégration.",
            ["Enable HTTPS interception when needed, and remove CA trust when XRatio should no longer be trusted by the current Windows user."] = "Activez l’interception HTTPS si nécessaire et retirez la confiance CA lorsque XRatio ne doit plus être approuvé par l’utilisateur Windows actuel.",
            ["Copy Info Hash"] = "Copier le hash info",
            ["Reset Statistics"] = "Réinitialiser les statistiques",
            ["Remove"] = "Supprimer",
            ["Remove CA trust"] = "Supprimer la confiance CA",
            ["Remove XRatio's CA from the current Windows user trust store? HTTPS tracker interception will stop."] = "Supprimer la CA XRatio du magasin de confiance de l’utilisateur Windows actuel ? L’interception HTTPS des trackers sera arrêtée.",
            ["Cancel"] = "Annuler",
            ["Reset"] = "Réinitialiser",
            ["BitTorrent metadata"] = "Métadonnées BitTorrent",
            ["Simulation guide"] = "Guide de simulation",
            ["Build a session in a few steps, then control it from the session list."] = "Créez une session en quelques étapes, puis contrôlez-la depuis la liste des sessions.",
            ["Interception guide"] = "Guide de l’interception",
            ["Follow tracker activity observed through the local proxy."] = "Suivez l’activité des trackers observée via le proxy local.",
            ["Activity guide"] = "Guide de l’activité",
            ["Use the event stream to understand what the proxy and simulations are doing."] = "Utilisez le flux d’événements pour comprendre l’activité du proxy et des simulations.",
            ["Settings guide"] = "Guide des réglages",
            ["Adjust the local proxy behavior and save the changes from this tab."] = "Ajustez le comportement du proxy local et enregistrez les changements depuis cet onglet.",
            ["Platform guide"] = "Guide de la plateforme",
            ["Manage system integration and HTTPS trust for the current machine."] = "Gérez l’intégration système et la confiance HTTPS de cette machine.",
            ["Overview guide"] = "Guide de la vue d’ensemble",
            ["Use this tab to check the current runtime at a glance."] = "Utilisez cet onglet pour vérifier l’état actuel en un coup d’œil.",
            ["1. Import a torrent"] = "1. Importer un torrent",
            ["The torrent file provides the metadata and the tracker list used by the session."] = "Le fichier torrent fournit les métadonnées et la liste des trackers utilisés par la session.",
            ["In Torrent file, click Browse… and select one .torrent file."] = "Dans Fichier torrent, cliquez sur Parcourir… et sélectionnez un fichier .torrent.",
            ["Check the selected tracker, info hash and size in Torrent info."] = "Vérifiez le tracker sélectionné, le hash info et la taille dans Informations du torrent.",
            ["2. Set the announce profile"] = "2. Définir le profil d’annonce",
            ["The main form contains the values that will be announced to the selected tracker."] = "Le formulaire principal contient les valeurs qui seront annoncées au tracker sélectionné.",
            ["Set upload and download speeds, or keep + Random values enabled to vary them between the configured limits."] = "Définissez les débits montant et descendant, ou laissez + Valeurs aléatoires activé pour les faire varier entre les limites configurées.",
            ["Choose the client profile and the finished percentage in Options."] = "Choisissez le profil client et le pourcentage terminé dans Options.",
            ["Use the Stop controls only when the session should end automatically."] = "Utilisez les contrôles d’arrêt uniquement lorsque la session doit se terminer automatiquement.",
            ["3. Review Advanced"] = "3. Vérifier les options avancées",
            ["Advanced contains the tracker identity and optional outbound proxy settings."] = "Avancé contient l’identité du tracker et les réglages facultatifs du proxy sortant.",
            ["Listening port and Peers requested control the identity sent by the session."] = "Le port d’écoute et le nombre de pairs demandés contrôlent l’identité envoyée par la session.",
            ["Proxy address is optional; enter an absolute address when the tracker connection must go through a proxy."] = "L’adresse du proxy est facultative ; saisissez une adresse absolue lorsque la connexion au tracker doit passer par un proxy.",
            ["4. Add, then start"] = "4. Ajouter, puis démarrer",
            ["Adding saves the configuration. Starting is the explicit action that begins tracker communication."] = "L’ajout enregistre la configuration. Le démarrage est l’action explicite qui lance la communication avec le tracker.",
            ["Click Add session to create the session in the list."] = "Cliquez sur Ajouter la session pour créer la session dans la liste.",
            ["Select the new session, then click ▶  Start. The action changes to ■  Stop while it is running."] = "Sélectionnez la nouvelle session, puis cliquez sur ▶  Démarrer. L’action devient ■  Arrêter pendant l’exécution.",
            ["5. Monitor the session"] = "5. Surveiller la session",
            ["The session row shows state, ratio, transfer counters, peers and the next announce time."] = "La ligne de session affiche l’état, le ratio, les compteurs de transfert, les pairs et l’heure de la prochaine annonce.",
            ["Use Manual update for an immediate update while the session is running."] = "Utilisez Mise à jour manuelle pour actualiser immédiatement une session en cours.",
            ["Select a stopped session and click Remove when it is no longer needed."] = "Sélectionnez une session arrêtée et cliquez sur Supprimer lorsqu’elle n’est plus nécessaire.",
            ["Start the proxy"] = "Démarrer le proxy",
            ["Use Start in the top bar after checking the proxy settings."] = "Utilisez Démarrer dans la barre supérieure après avoir vérifié les réglages du proxy.",
            ["The status indicator in the header shows whether the proxy is active or paused."] = "L’indicateur d’état dans l’en-tête indique si le proxy est actif ou en pause.",
            ["Read tracked sessions"] = "Lire les sessions suivies",
            ["Each row contains the torrent hash, tracker, peers, status, counters and the last announce time."] = "Chaque ligne contient le hash du torrent, le tracker, les pairs, l’état, les compteurs et l’heure de la dernière annonce.",
            ["Read the latest events"] = "Lire les derniers événements",
            ["Change a value"] = "Modifier une valeur",
            ["Edit the fields, review the toggles, then click Save changes in the Settings tab."] = "Modifiez les champs, vérifiez les options, puis cliquez sur Enregistrer dans l’onglet Réglages.",
            ["Configure the qBittorrent client"] = "Configurer le client qBittorrent",
            ["Route qBittorrent tracker announces through the local XRatio proxy before checking the ratio."] = "Faites passer les annonces des trackers qBittorrent par le proxy local XRatio avant de vérifier le ratio.",
            ["Start XRatio and verify that the header shows HTTP/HTTPS active on 127.0.0.1:3773."] = "Démarrez XRatio et vérifiez que l’en-tête indique HTTP/HTTPS actif sur 127.0.0.1:3773.",
            ["In qBittorrent, open Tools > Options > Connection."] = "Dans qBittorrent, ouvrez Outils > Options > Connexion.",
            ["Under Proxy Server, choose HTTP, set Host to 127.0.0.1 and Port to 3773."] = "Dans Proxy Server, choisissez HTTP, indiquez 127.0.0.1 comme hôte et 3773 comme port.",
            ["Enable Perform hostname lookup via proxy and Use proxy for BitTorrent purposes. Leave Use proxy for peer connections disabled because XRatio handles tracker announces only."] = "Activez Perform hostname lookup via proxy et Use proxy for BitTorrent purposes. Laissez Use proxy for peer connections désactivé, car XRatio ne traite que les annonces des trackers.",
            ["In XRatio Settings > Announce behavior, use Report download as zero or Pretend to seed only when that reporting mode is allowed for your test tracker; these options change the announce values and do not freeze a tracker-owned ratio."] = "Dans Réglages XRatio > Comportement des annonces, utilisez Annoncer un téléchargement nul ou Simuler le seeding uniquement si ce mode est autorisé par votre tracker de test ; ces options modifient les valeurs annoncées et ne figent pas un ratio détenu par le tracker.",
            ["Click Apply, then OK. Check the Interception tab in XRatio for the next tracker announce."] = "Cliquez sur Apply, puis sur OK. Vérifiez l’onglet Interception de XRatio à la prochaine annonce du tracker.",
            ["If the ratio still changes, check the port, proxy type and tracker policy. A proxy cannot force a tracker to accept or freeze a ratio."] = "Si le ratio change encore, vérifiez le port, le type de proxy et la politique du tracker. Un proxy ne peut pas obliger un tracker à accepter ou à figer un ratio.",
            ["Keep the scope clear"] = "Garder le périmètre clair",
            ["Keep Listen on localhost only enabled unless you have a specific, authorized reason to change the deployment boundary."] = "Laissez Écouter uniquement sur localhost activé, sauf raison précise et autorisée de modifier la limite d’exposition.",
            ["Use only torrents and trackers for which you are authorized, and follow the tracker rules."] = "Utilisez uniquement des torrents et des trackers pour lesquels vous êtes autorisé, et respectez les règles du tracker.",
            ["Startup behavior"] = "Comportement au démarrage",
            ["Safety note"] = "Note de sécurité",
            ["Use simulation only with torrents and trackers for which you are authorized."] = "Utilisez la simulation uniquement avec des torrents et des trackers pour lesquels vous êtes autorisé.",
            ["This guide follows the active XRatio tab."] = "Ce guide suit l’onglet XRatio actif.",
            ["If an action is unavailable, select the relevant row or complete the required fields first."] = "Si une action est indisponible, sélectionnez la ligne concernée ou complétez d’abord les champs requis.",
            ["Get started"] = "Premiers pas",
            ["Use the visible controls from top to bottom, then check the activity and status feedback."] = "Utilisez les contrôles visibles de haut en bas, puis vérifiez l’activité et le retour d’état.",
            ["Read the status"] = "Lire l’état",
            ["Tracker rules still apply to every session."] = "Les règles du tracker s’appliquent à chaque session.",
            ["Active"] = "Actif",
            ["Paused"] = "En pause",
            ["Proxy stopped"] = "Proxy arrêté",
            ["Ready"] = "Prêt",
            ["Seed-only standby"] = "Veille seed-only",
            ["Waiting for leechers"] = "En attente de leechers",
            ["Clipboard is unavailable."] = "Le presse-papiers est indisponible.",
            ["Configuration loaded."] = "Configuration chargée.",
            ["Using default configuration."] = "Configuration par défaut utilisée.",
            ["Imported settings.dat into settings.json; the Tcl file was left unchanged."] = "settings.dat a été importé dans settings.json ; le fichier Tcl reste inchangé.",
            ["Imported settings.dat.bak after the primary Tcl settings were invalid; both Tcl files were left unchanged."] = "settings.dat.bak a été importé car les réglages Tcl principaux étaient invalides ; les deux fichiers Tcl restent inchangés.",
            ["Loaded the JSON settings backup because settings.json was invalid."] = "La sauvegarde JSON a été chargée car settings.json était invalide.",
            ["Unavailable on this platform"] = "Indisponible sur cette plateforme",
            ["Trusted and enabled"] = "De confiance et activée",
            ["Not trusted — HTTPS interception is off"] = "Non approuvée — l’interception HTTPS est désactivée",
            ["XRatio cannot install or inspect a user-scoped CA here."] = "XRatio ne peut pas installer ou inspecter une CA limitée à l’utilisateur ici.",
            ["The installation CA is trusted for the current Windows user, so HTTPS tracker announces can be inspected."] = "La CA d’installation est approuvée pour l’utilisateur Windows actuel ; les annonces HTTPS des trackers peuvent donc être inspectées.",
            ["HTTP interception still works. Trust the installation CA only if you need HTTPS tracker interception."] = "L’interception HTTP fonctionne toujours. N’approuvez la CA d’installation que si vous avez besoin de l’interception HTTPS des trackers.",
            ["Upload/download minimum cannot exceed its maximum."] = "Le minimum upload/download ne peut pas dépasser son maximum.",
            ["Upload/upload minimum cannot exceed its maximum."] = "Le minimum upload/upload ne peut pas dépasser son maximum.",
            ["Proxy port must be between 1 and 65534."] = "Le port du proxy doit être compris entre 1 et 65534.",
            ["Minimum leechers must be between 0 and 100."] = "Le nombre minimal de leechers doit être compris entre 0 et 100.",
            ["Boost chance must be between 0 and 100%."] = "La chance de boost doit être comprise entre 0 et 100 %.",
            ["Multipliers and boost values cannot be negative."] = "Les multiplicateurs et valeurs de boost ne peuvent pas être négatifs."
        };

    private static readonly IReadOnlyDictionary<string, string> SpanishMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglés", ["French"] = "Francés", ["Spanish"] = "Español", ["German"] = "Alemán",
            ["Italian"] = "Italiano", ["Portuguese"] = "Portugués", ["Japanese"] = "Japonés", ["Chinese"] = "Chino",
            ["Arabic"] = "Árabe", ["Russian"] = "Ruso", ["Overview"] = "Vista general", ["Interception"] = "Interceptación",
            ["Simulation"] = "Simulación", ["Activity"] = "Actividad", ["Settings"] = "Ajustes", ["Platform"] = "Plataforma",
            ["Monitoring"] = "Supervisión", ["Control"] = "Control", ["System"] = "Sistema", ["Support"] = "Ayuda", ["Guide"] = "Guía",
            ["LOCAL RATIO CONTROL"] = "CONTROL DE RATIO LOCAL", ["LOCAL / MONITORING"] = "LOCAL / SUPERVISIÓN",
            ["Loading configuration…"] = "Cargando configuración…", ["Start"] = "Iniciar", ["Stop"] = "Detener", ["Retry"] = "Reintentar",
            ["Pause"] = "Pausa", ["Resume"] = "Reanudar", ["Save changes"] = "Guardar cambios", ["To tray"] = "Minimizar a la bandeja",
            ["Close"] = "Cerrar", ["Open Settings"] = "Abrir ajustes", ["Current runtime status."] = "Estado actual del servicio.",
            ["PROXY CHANNEL"] = "CANAL PROXY", ["Local tracker interception · HTTP / HTTPS"] = "Interceptación local de trackers · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrents seguidos", ["Announcements observed"] = "Anuncios observados", ["Simulations"] = "Simulaciones",
            ["Active / configured"] = "Activas / configuradas", ["Reported upload"] = "Upload anunciado", ["Current session"] = "Sesión actual",
            ["OPERATING MODES"] = "MODOS DE OPERACIÓN", ["Two paths, one local control plane."] = "Dos rutas, un solo plano de control local.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Solo anuncios de trackers — los payloads y el tráfico entre pares permanecen intactos.",
            ["Appearance"] = "Apariencia", ["Updates"] = "Actualizaciones", ["Check the official GitHub release without changing files automatically."] = "Busca la versión oficial de GitHub sin cambiar archivos automáticamente.", ["Current version"] = "Versión actual", ["Check for updates"] = "Buscar actualizaciones", ["Not checked yet"] = "Aún no comprobado", ["Checking for updates…"] = "Buscando actualizaciones…", ["You are up to date"] = "Está actualizado", ["Unable to check for updates"] = "No se pueden buscar actualizaciones", ["Update available: {0}"] = "Actualización disponible: {0}", ["Theme"] = "Tema", ["Light"] = "Claro", ["Dim"] = "Tenue", ["Soft Dark"] = "Oscuro suave", ["Dark"] = "Oscuro",
            ["Accent color"] = "Color de acento", ["Blue"] = "Azul", ["Teal"] = "Verde azulado", ["Violet"] = "Violeta", ["Amber"] = "Ámbar", ["Rose"] = "Rosa", ["Green"] = "Verde",
            ["Language"] = "Idioma", ["Choose the language used by the XRatio interface."] = "Elige el idioma de la interfaz de XRatio.",
            ["Connection"] = "Conexión", ["HTTP proxy port"] = "Puerto del proxy HTTP", ["Minimum leechers"] = "Leechers mínimos",
            ["Accept tracker traffic only"] = "Aceptar solo tráfico de trackers", ["Listen on localhost only (required)"] = "Escuchar solo en localhost (obligatorio)",
            ["Write redacted proxy debug log"] = "Escribir registro de depuración anonimizado", ["Configuration"] = "Configuración",
            ["Configuration saved."] = "Configuración guardada.", ["Active"] = "Activo", ["Paused"] = "En pausa", ["Proxy stopped"] = "Proxy detenido", ["Ready"] = "Listo"
        };

    private static readonly IReadOnlyDictionary<string, string> GermanMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Englisch", ["French"] = "Französisch", ["Spanish"] = "Spanisch", ["German"] = "Deutsch",
            ["Italian"] = "Italienisch", ["Portuguese"] = "Portugiesisch", ["Japanese"] = "Japanisch", ["Chinese"] = "Chinesisch",
            ["Arabic"] = "Arabisch", ["Russian"] = "Russisch", ["Overview"] = "Übersicht", ["Interception"] = "Abfangen",
            ["Simulation"] = "Simulation", ["Activity"] = "Aktivität", ["Settings"] = "Einstellungen", ["Platform"] = "Plattform",
            ["Monitoring"] = "Überwachung", ["Control"] = "Steuerung", ["System"] = "System", ["Support"] = "Hilfe", ["Guide"] = "Anleitung",
            ["LOCAL RATIO CONTROL"] = "LOKALE RATIO-STEUERUNG", ["LOCAL / MONITORING"] = "LOKAL / ÜBERWACHUNG",
            ["Loading configuration…"] = "Konfiguration wird geladen…", ["Start"] = "Starten", ["Stop"] = "Stoppen", ["Retry"] = "Erneut versuchen",
            ["Pause"] = "Pause", ["Resume"] = "Fortsetzen", ["Save changes"] = "Änderungen speichern", ["To tray"] = "In den Infobereich minimieren",
            ["Close"] = "Schließen", ["Open Settings"] = "Einstellungen öffnen", ["Current runtime status."] = "Aktueller Dienststatus.",
            ["PROXY CHANNEL"] = "PROXY-KANAL", ["Local tracker interception · HTTP / HTTPS"] = "Lokales Tracker-Abfangen · HTTP / HTTPS",
            ["Tracked torrents"] = "Überwachte Torrents", ["Announcements observed"] = "Beobachtete Ankündigungen", ["Simulations"] = "Simulationen",
            ["Active / configured"] = "Aktiv / konfiguriert", ["Reported upload"] = "Gemeldeter Upload", ["Current session"] = "Aktuelle Sitzung",
            ["OPERATING MODES"] = "BETRIEBSMODI", ["Two paths, one local control plane."] = "Zwei Wege, eine lokale Steuerung.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Nur Tracker-Ankündigungen — Nutzdaten und Peer-Verkehr bleiben unverändert.",
            ["Appearance"] = "Darstellung", ["Updates"] = "Aktualisierungen", ["Check the official GitHub release without changing files automatically."] = "Prüfe die offizielle GitHub-Version, ohne Dateien automatisch zu ändern.", ["Current version"] = "Aktuelle Version", ["Check for updates"] = "Nach Updates suchen", ["Not checked yet"] = "Noch nicht geprüft", ["Checking for updates…"] = "Suche nach Updates…", ["You are up to date"] = "Du bist auf dem neuesten Stand", ["Unable to check for updates"] = "Updates konnten nicht geprüft werden", ["Update available: {0}"] = "Update verfügbar: {0}", ["Theme"] = "Design", ["Light"] = "Hell", ["Dim"] = "Gedämpft", ["Soft Dark"] = "Sanft dunkel", ["Dark"] = "Dunkel",
            ["Accent color"] = "Akzentfarbe", ["Blue"] = "Blau", ["Teal"] = "Türkis", ["Violet"] = "Violett", ["Amber"] = "Bernstein", ["Rose"] = "Rosa", ["Green"] = "Grün",
            ["Language"] = "Sprache", ["Choose the language used by the XRatio interface."] = "Wähle die Sprache der XRatio-Oberfläche.",
            ["Connection"] = "Verbindung", ["HTTP proxy port"] = "HTTP-Proxy-Port", ["Minimum leechers"] = "Minimale Leecher",
            ["Accept tracker traffic only"] = "Nur Tracker-Verkehr zulassen", ["Listen on localhost only (required)"] = "Nur auf localhost lauschen (erforderlich)",
            ["Write redacted proxy debug log"] = "Anonymisiertes Proxy-Debugprotokoll schreiben", ["Configuration"] = "Konfiguration",
            ["Configuration saved."] = "Konfiguration gespeichert.", ["Active"] = "Aktiv", ["Paused"] = "Pausiert", ["Proxy stopped"] = "Proxy gestoppt", ["Ready"] = "Bereit"
        };

    private static readonly IReadOnlyDictionary<string, string> ItalianMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglese", ["French"] = "Francese", ["Spanish"] = "Spagnolo", ["German"] = "Tedesco",
            ["Italian"] = "Italiano", ["Portuguese"] = "Portoghese", ["Japanese"] = "Giapponese", ["Chinese"] = "Cinese",
            ["Arabic"] = "Arabo", ["Russian"] = "Russo", ["Overview"] = "Panoramica", ["Interception"] = "Intercettazione",
            ["Simulation"] = "Simulazione", ["Activity"] = "Attività", ["Settings"] = "Impostazioni", ["Platform"] = "Piattaforma",
            ["Monitoring"] = "Monitoraggio", ["Control"] = "Controllo", ["System"] = "Sistema", ["Support"] = "Supporto", ["Guide"] = "Guida",
            ["LOCAL RATIO CONTROL"] = "CONTROLLO RATIO LOCALE", ["LOCAL / MONITORING"] = "LOCALE / MONITORAGGIO",
            ["Loading configuration…"] = "Caricamento configurazione…", ["Start"] = "Avvia", ["Stop"] = "Arresta", ["Retry"] = "Riprova",
            ["Pause"] = "Pausa", ["Resume"] = "Riprendi", ["Save changes"] = "Salva modifiche", ["To tray"] = "Riduci nell’area di notifica",
            ["Close"] = "Chiudi", ["Open Settings"] = "Apri impostazioni", ["Current runtime status."] = "Stato attuale del servizio.",
            ["PROXY CHANNEL"] = "CANALE PROXY", ["Local tracker interception · HTTP / HTTPS"] = "Intercettazione tracker locale · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrent monitorati", ["Announcements observed"] = "Annunci osservati", ["Simulations"] = "Simulazioni",
            ["Active / configured"] = "Attive / configurate", ["Reported upload"] = "Upload annunciato", ["Current session"] = "Sessione corrente",
            ["OPERATING MODES"] = "MODALITÀ OPERATIVE", ["Two paths, one local control plane."] = "Due percorsi, un solo piano di controllo locale.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Solo annunci tracker — payload e traffico peer restano invariati.",
            ["Appearance"] = "Aspetto", ["Updates"] = "Aggiornamenti", ["Check the official GitHub release without changing files automatically."] = "Controlla la release ufficiale GitHub senza modificare automaticamente i file.", ["Current version"] = "Versione attuale", ["Check for updates"] = "Cerca aggiornamenti", ["Not checked yet"] = "Non ancora verificato", ["Checking for updates…"] = "Ricerca aggiornamenti…", ["You are up to date"] = "È installata l’ultima versione", ["Unable to check for updates"] = "Impossibile cercare aggiornamenti", ["Update available: {0}"] = "Aggiornamento disponibile: {0}", ["Theme"] = "Tema", ["Light"] = "Chiaro", ["Dim"] = "Attenuato", ["Soft Dark"] = "Scuro morbido", ["Dark"] = "Scuro",
            ["Accent color"] = "Colore accento", ["Blue"] = "Blu", ["Teal"] = "Verde acqua", ["Violet"] = "Viola", ["Amber"] = "Ambra", ["Rose"] = "Rosa", ["Green"] = "Verde",
            ["Language"] = "Lingua", ["Choose the language used by the XRatio interface."] = "Scegli la lingua dell’interfaccia XRatio.",
            ["Connection"] = "Connessione", ["HTTP proxy port"] = "Porta proxy HTTP", ["Minimum leechers"] = "Leecher minimi",
            ["Accept tracker traffic only"] = "Accetta solo traffico tracker", ["Listen on localhost only (required)"] = "Ascolta solo su localhost (obbligatorio)",
            ["Write redacted proxy debug log"] = "Scrivi log proxy anonimizzato", ["Configuration"] = "Configurazione",
            ["Configuration saved."] = "Configurazione salvata.", ["Active"] = "Attivo", ["Paused"] = "In pausa", ["Proxy stopped"] = "Proxy arrestato", ["Ready"] = "Pronto"
        };

    private static readonly IReadOnlyDictionary<string, string> PortugueseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglês", ["French"] = "Francês", ["Spanish"] = "Espanhol", ["German"] = "Alemão",
            ["Italian"] = "Italiano", ["Portuguese"] = "Português", ["Japanese"] = "Japonês", ["Chinese"] = "Chinês",
            ["Arabic"] = "Árabe", ["Russian"] = "Russo", ["Overview"] = "Visão geral", ["Interception"] = "Interceptação",
            ["Simulation"] = "Simulação", ["Activity"] = "Atividade", ["Settings"] = "Configurações", ["Platform"] = "Plataforma",
            ["Monitoring"] = "Monitoramento", ["Control"] = "Controle", ["System"] = "Sistema", ["Support"] = "Suporte", ["Guide"] = "Guia",
            ["LOCAL RATIO CONTROL"] = "CONTROLE DE RATIO LOCAL", ["LOCAL / MONITORING"] = "LOCAL / MONITORAMENTO",
            ["Loading configuration…"] = "Carregando configuração…", ["Start"] = "Iniciar", ["Stop"] = "Parar", ["Retry"] = "Tentar novamente",
            ["Pause"] = "Pausar", ["Resume"] = "Retomar", ["Save changes"] = "Salvar alterações", ["To tray"] = "Minimizar para a bandeja",
            ["Close"] = "Fechar", ["Open Settings"] = "Abrir configurações", ["Current runtime status."] = "Estado atual do serviço.",
            ["PROXY CHANNEL"] = "CANAL PROXY", ["Local tracker interception · HTTP / HTTPS"] = "Interceptação local de trackers · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrents monitorados", ["Announcements observed"] = "Anúncios observados", ["Simulations"] = "Simulações",
            ["Active / configured"] = "Ativas / configuradas", ["Reported upload"] = "Upload anunciado", ["Current session"] = "Sessão atual",
            ["OPERATING MODES"] = "MODOS DE OPERAÇÃO", ["Two paths, one local control plane."] = "Dois caminhos, um único plano de controle local.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Somente anúncios de trackers — payloads e tráfego entre pares permanecem intactos.",
            ["Appearance"] = "Aparência", ["Updates"] = "Atualizações", ["Check the official GitHub release without changing files automatically."] = "Verifique a versão oficial do GitHub sem alterar arquivos automaticamente.", ["Current version"] = "Versão atual", ["Check for updates"] = "Verificar atualizações", ["Not checked yet"] = "Ainda não verificado", ["Checking for updates…"] = "Verificando atualizações…", ["You are up to date"] = "Você está usando a versão mais recente", ["Unable to check for updates"] = "Não foi possível verificar atualizações", ["Update available: {0}"] = "Atualização disponível: {0}", ["Theme"] = "Tema", ["Light"] = "Claro", ["Dim"] = "Suave", ["Soft Dark"] = "Escuro suave", ["Dark"] = "Escuro",
            ["Accent color"] = "Cor de destaque", ["Blue"] = "Azul", ["Teal"] = "Turquesa", ["Violet"] = "Violeta", ["Amber"] = "Âmbar", ["Rose"] = "Rosa", ["Green"] = "Verde",
            ["Language"] = "Idioma", ["Choose the language used by the XRatio interface."] = "Escolha o idioma da interface XRatio.",
            ["Connection"] = "Conexão", ["HTTP proxy port"] = "Porta do proxy HTTP", ["Minimum leechers"] = "Leechers mínimos",
            ["Accept tracker traffic only"] = "Aceitar apenas tráfego de trackers", ["Listen on localhost only (required)"] = "Escutar apenas no localhost (obrigatório)",
            ["Write redacted proxy debug log"] = "Escrever log de depuração anonimizado", ["Configuration"] = "Configuração",
            ["Configuration saved."] = "Configuração salva.", ["Active"] = "Ativo", ["Paused"] = "Pausado", ["Proxy stopped"] = "Proxy parado", ["Ready"] = "Pronto"
        };

    private static readonly IReadOnlyDictionary<string, string> JapaneseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "概要", ["Interception"] = "インターセプト", ["Simulation"] = "シミュレーション", ["Activity"] = "アクティビティ",
            ["Settings"] = "設定", ["Platform"] = "プラットフォーム", ["Monitoring"] = "監視", ["Control"] = "制御", ["System"] = "システム", ["Support"] = "サポート", ["Guide"] = "ガイド",
            ["Appearance"] = "外観", ["Updates"] = "更新", ["Check the official GitHub release without changing files automatically."] = "ファイルを自動変更せず、GitHub の公式リリースを確認します。", ["Current version"] = "現在のバージョン", ["Check for updates"] = "更新を確認", ["Not checked yet"] = "未確認", ["Checking for updates…"] = "更新を確認中…", ["You are up to date"] = "最新バージョンです", ["Unable to check for updates"] = "更新を確認できません", ["Update available: {0}"] = "更新があります: {0}", ["Theme"] = "テーマ", ["Light"] = "ライト", ["Dim"] = "控えめ", ["Soft Dark"] = "ソフトダーク", ["Dark"] = "ダーク",
            ["Accent color"] = "アクセントカラー", ["Blue"] = "ブルー", ["Teal"] = "ティール", ["Violet"] = "バイオレット", ["Amber"] = "アンバー", ["Rose"] = "ローズ", ["Green"] = "グリーン",
            ["Language"] = "言語", ["Connection"] = "接続", ["Configuration"] = "設定", ["Start"] = "開始", ["Stop"] = "停止", ["Pause"] = "一時停止", ["Resume"] = "再開", ["Save changes"] = "変更を保存"
        };

    private static readonly IReadOnlyDictionary<string, string> ChineseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "概览", ["Interception"] = "拦截", ["Simulation"] = "模拟", ["Activity"] = "活动",
            ["Settings"] = "设置", ["Platform"] = "平台", ["Monitoring"] = "监控", ["Control"] = "控制", ["System"] = "系统", ["Support"] = "支持", ["Guide"] = "指南",
            ["Appearance"] = "外观", ["Updates"] = "更新", ["Check the official GitHub release without changing files automatically."] = "检查 GitHub 官方版本，不会自动修改文件。", ["Current version"] = "当前版本", ["Check for updates"] = "检查更新", ["Not checked yet"] = "尚未检查", ["Checking for updates…"] = "正在检查更新…", ["You are up to date"] = "已是最新版本", ["Unable to check for updates"] = "无法检查更新", ["Update available: {0}"] = "有可用更新：{0}", ["Theme"] = "主题", ["Light"] = "浅色", ["Dim"] = "柔和", ["Soft Dark"] = "柔和深色", ["Dark"] = "深色",
            ["Accent color"] = "强调色", ["Blue"] = "蓝色", ["Teal"] = "青绿色", ["Violet"] = "紫色", ["Amber"] = "琥珀色", ["Rose"] = "玫瑰色", ["Green"] = "绿色",
            ["Language"] = "语言", ["Connection"] = "连接", ["Configuration"] = "配置", ["Start"] = "启动", ["Stop"] = "停止", ["Pause"] = "暂停", ["Resume"] = "继续", ["Save changes"] = "保存更改"
        };

    private static readonly IReadOnlyDictionary<string, string> ArabicMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "نظرة عامة", ["Interception"] = "الاعتراض", ["Simulation"] = "المحاكاة", ["Activity"] = "النشاط",
            ["Settings"] = "الإعدادات", ["Platform"] = "المنصة", ["Monitoring"] = "المراقبة", ["Control"] = "التحكم", ["System"] = "النظام", ["Support"] = "الدعم", ["Guide"] = "الدليل",
            ["Appearance"] = "المظهر", ["Updates"] = "التحديثات", ["Check the official GitHub release without changing files automatically."] = "تحقق من إصدار GitHub الرسمي دون تغيير الملفات تلقائياً.", ["Current version"] = "الإصدار الحالي", ["Check for updates"] = "البحث عن تحديثات", ["Not checked yet"] = "لم يتم التحقق بعد", ["Checking for updates…"] = "جارٍ البحث عن تحديثات…", ["You are up to date"] = "لديك أحدث إصدار", ["Unable to check for updates"] = "تعذر البحث عن تحديثات", ["Update available: {0}"] = "يتوفر تحديث: {0}", ["Theme"] = "السمة", ["Light"] = "فاتح", ["Dim"] = "خافت", ["Soft Dark"] = "داكن ناعم", ["Dark"] = "داكن",
            ["Accent color"] = "لون التمييز", ["Blue"] = "أزرق", ["Teal"] = "تركوازي", ["Violet"] = "بنفسجي", ["Amber"] = "كهرماني", ["Rose"] = "وردي", ["Green"] = "أخضر",
            ["Language"] = "اللغة", ["Connection"] = "الاتصال", ["Configuration"] = "الإعدادات", ["Start"] = "بدء", ["Stop"] = "إيقاف", ["Pause"] = "إيقاف مؤقت", ["Resume"] = "استئناف", ["Save changes"] = "حفظ التغييرات"
        };

    private static readonly IReadOnlyDictionary<string, string> RussianMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "Обзор", ["Interception"] = "Перехват", ["Simulation"] = "Симуляция", ["Activity"] = "Активность",
            ["Settings"] = "Настройки", ["Platform"] = "Платформа", ["Monitoring"] = "Мониторинг", ["Control"] = "Управление", ["System"] = "Система", ["Support"] = "Поддержка", ["Guide"] = "Справка",
            ["Appearance"] = "Внешний вид", ["Updates"] = "Обновления", ["Check the official GitHub release without changing files automatically."] = "Проверяйте официальный выпуск GitHub без автоматического изменения файлов.", ["Current version"] = "Текущая версия", ["Check for updates"] = "Проверить обновления", ["Not checked yet"] = "Ещё не проверено", ["Checking for updates…"] = "Проверка обновлений…", ["You are up to date"] = "Установлена последняя версия", ["Unable to check for updates"] = "Не удалось проверить обновления", ["Update available: {0}"] = "Доступно обновление: {0}", ["Theme"] = "Тема", ["Light"] = "Светлая", ["Dim"] = "Приглушённая", ["Soft Dark"] = "Мягкая тёмная", ["Dark"] = "Тёмная",
            ["Accent color"] = "Цвет акцента", ["Blue"] = "Синий", ["Teal"] = "Бирюзовый", ["Violet"] = "Фиолетовый", ["Amber"] = "Янтарный", ["Rose"] = "Розовый", ["Green"] = "Зелёный",
            ["Language"] = "Язык", ["Connection"] = "Подключение", ["Configuration"] = "Конфигурация", ["Start"] = "Запустить", ["Stop"] = "Остановить", ["Pause"] = "Пауза", ["Resume"] = "Продолжить", ["Save changes"] = "Сохранить изменения"
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> TranslationMaps =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            [French] = FrenchMap,
            [Spanish] = SpanishMap,
            [German] = GermanMap,
            [Italian] = ItalianMap,
            [Portuguese] = PortugueseMap,
            [Japanese] = JapaneseMap,
            [Chinese] = ChineseMap,
            [Arabic] = ArabicMap,
            [Russian] = RussianMap
        };

    private static readonly IReadOnlyDictionary<string, string> EnglishMap = BuildEnglishMap();

    private static IReadOnlyDictionary<string, string> BuildEnglishMap()
    {
        var reverse = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var map in TranslationMaps.Values)
        {
            foreach (var pair in map)
            {
                if (!reverse.ContainsKey(pair.Value))
                    reverse[pair.Value] = pair.Key;
            }
        }
        return reverse;
    }

    public static string Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return English;

        var trimmed = language.Trim();
        var code = LanguageCodes.FirstOrDefault(value =>
            string.Equals(value, trimmed, StringComparison.OrdinalIgnoreCase));
        if (code is not null)
            return code;

        var labelIndex = LanguageLabels
            .Select((label, index) => (label, index))
            .FirstOrDefault(item => string.Equals(item.label, trimmed, StringComparison.Ordinal));
        return labelIndex.label is not null ? LanguageCodes[labelIndex.index] : English;
    }

    public static string At(int index) =>
        index >= 0 && index < LanguageCodes.Count ? LanguageCodes[index] : English;

    public static int LanguageIndex(string? value)
    {
        var code = Normalize(value);
        var match = LanguageCodes
            .Select((item, index) => (item, index))
            .FirstOrDefault(pair => string.Equals(pair.item, code, StringComparison.Ordinal));
        return match.item is not null ? match.index : 0;
    }

    public static string FlagCodeAt(int index) =>
        index >= 0 && index < LanguageFlagCodes.Count ? LanguageFlagCodes[index] : LanguageFlagCodes[0];

    public static string DisplayNameAt(int index) =>
        index >= 0 && index < LanguageDisplayNames.Count ? LanguageDisplayNames[index] : LanguageDisplayNames[0];

    public static int IndexOf(string? language)
    {
        var normalized = Normalize(language);
        var index = LanguageCodes
            .Select((value, itemIndex) => (value, itemIndex))
            .FirstOrDefault(item => string.Equals(item.value, normalized, StringComparison.Ordinal));
        return index.value is not null ? index.itemIndex : 0;
    }

    public static string Translate(string key, string language)
    {
        var map = TranslationMaps.TryGetValue(Normalize(language), out var selected)
            ? selected
            : null;
        if (map is not null && map.TryGetValue(key, out var translation))
            return translation;
        return key;
    }

    public static string TranslateAny(string? value, string language)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;
        if (FrenchMap.ContainsKey(value))
            return Translate(value, language);
        if (EnglishMap.TryGetValue(value, out var englishKey))
            return Translate(englishKey, language);
        return value;
    }

    public static string TranslateMessage(string message, string language)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var normalized = Normalize(language);
        var direct = TranslateAny(message, normalized);
        if (!string.Equals(direct, message, StringComparison.Ordinal))
            return direct;

        const string englishGuidePrefix = "XRatio Guide · ";
        const string frenchGuidePrefix = "Guide XRatio · ";
        if (normalized == French && message.StartsWith(englishGuidePrefix, StringComparison.Ordinal))
        {
            var tab = message[englishGuidePrefix.Length..];
            return $"Guide XRatio · {TranslateAny(tab, normalized)}";
        }
        if (normalized == English && message.StartsWith(frenchGuidePrefix, StringComparison.Ordinal))
        {
            var tab = message[frenchGuidePrefix.Length..];
            return $"XRatio Guide · {TranslateAny(tab, normalized)}";
        }

        if (normalized == French)
        {
            const string removePrefix = "Remove the stopped simulation “";
            const string removeSuffix = "”? This does not delete the .torrent file.";
            if (message.StartsWith(removePrefix, StringComparison.Ordinal) &&
                message.EndsWith(removeSuffix, StringComparison.Ordinal))
            {
                var name = message[removePrefix.Length..^removeSuffix.Length];
                return $"Supprimer la simulation arrêtée «{name}» ? Cela ne supprime pas le fichier .torrent.";
            }

            if (message.StartsWith("Port ", StringComparison.Ordinal) &&
                message.Contains(" is already in use. Interception is stopped until you choose a free port or close the other listener.", StringComparison.Ordinal))
            {
                var suffix = " is already in use. Interception is stopped until you choose a free port or close the other listener.";
                return message[..^suffix.Length] + " est déjà utilisé. L’interception est arrêtée jusqu’à ce que vous choisissiez un port libre ou fermiez l’autre écouteur.";
            }

            var simulationStatus = TranslateSimulationStatus(message, toFrench: true);
            if (!string.Equals(simulationStatus, message, StringComparison.Ordinal))
                return simulationStatus;

            return ReplacePrefix(message, [
                ("HTTP/HTTPS active on", "HTTP/HTTPS actif sur"),
                ("Paused on", "En pause sur"),
                ("Interception is stopped. ", "L’interception est arrêtée. "),
                ("Simulation added: ", "Simulation ajoutée : "),
                ("Simulation start failed: ", "Échec du démarrage de la simulation : "),
                ("Simulation update failed: ", "Échec de la mise à jour de la simulation : "),
                ("Could not add simulation: ", "Impossible d’ajouter la simulation : "),
                ("Could not restore simulation form: ", "Impossible de restaurer le formulaire de simulation : "),
                ("Removed simulation: ", "Simulation supprimée : "),
                ("Loaded torrent: ", "Torrent chargé : "),
                ("Torrent import failed: ", "Échec de l’import du torrent : "),
                ("Could not enable HTTPS: ", "Impossible d’activer HTTPS : "),
                ("Could not remove CA trust: ", "Impossible de supprimer la confiance CA : "),
                ("Configuration error: ", "Erreur de configuration : "),
                ("Proxy cleanup error: ", "Erreur de nettoyage du proxy : "),
                ("Simulation settings persistence error: ", "Erreur de persistance des réglages de simulation : "),
                ("State persistence error: ", "Erreur de persistance de l’état : "),
                ("Skipped saved simulation: ", "Simulation enregistrée ignorée : "),
                ("Copied info hash to clipboard: ", "Hash info copié dans le presse-papiers : "),
                ("Reset stats for torrent hash: ", "Statistiques réinitialisées pour le hash du torrent : "),
                ("Reset all tracked statistics for ", "Réinitialiser toutes les statistiques suivies pour "),
                ("Remove the stopped simulation “", "Supprimer la simulation arrêtée «"),
                ("Startup error: ", "Erreur de démarrage : "),
                ("Removed ", "Supprimé "),
                ("Restored ", "Restauré ")
            ], out var translated)
                ? translated
                : TranslateProgressMessage(message, toFrench: true);
        }

        const string frenchRemovePrefix = "Supprimer la simulation arrêtée «";
        const string frenchRemoveSuffix = "» ? Cela ne supprime pas le fichier .torrent.";
        if (message.StartsWith(frenchRemovePrefix, StringComparison.Ordinal) &&
            message.EndsWith(frenchRemoveSuffix, StringComparison.Ordinal))
        {
            var name = message[frenchRemovePrefix.Length..^frenchRemoveSuffix.Length];
            return $"Remove the stopped simulation “{name}”? This does not delete the .torrent file.";
        }

        if (message.StartsWith("Port ", StringComparison.Ordinal) &&
            message.Contains(" est déjà utilisé. L’interception est arrêtée jusqu’à ce que vous choisissiez un port libre ou fermiez l’autre écouteur.", StringComparison.Ordinal))
        {
            var suffix = " est déjà utilisé. L’interception est arrêtée jusqu’à ce que vous choisissiez un port libre ou fermiez l’autre écouteur.";
            return message[..^suffix.Length] + " is already in use. Interception is stopped until you choose a free port or close the other listener.";
        }

        var englishSimulationStatus = TranslateSimulationStatus(message, toFrench: false);
        if (!string.Equals(englishSimulationStatus, message, StringComparison.Ordinal))
            return englishSimulationStatus;

        return ReplacePrefix(message, [
            ("HTTP/HTTPS actif sur", "HTTP/HTTPS active on"),
            ("En pause sur", "Paused on"),
            ("L’interception est arrêtée. ", "Interception is stopped. "),
            ("Simulation ajoutée : ", "Simulation added: "),
            ("Échec du démarrage de la simulation : ", "Simulation start failed: "),
            ("Échec de la mise à jour de la simulation : ", "Simulation update failed: "),
            ("Impossible d’ajouter la simulation : ", "Could not add simulation: "),
            ("Impossible de restaurer le formulaire de simulation : ", "Could not restore simulation form: "),
            ("Simulation supprimée : ", "Removed simulation: "),
            ("Torrent chargé : ", "Loaded torrent: "),
            ("Échec de l’import du torrent : ", "Torrent import failed: "),
            ("Impossible d’activer HTTPS : ", "Could not enable HTTPS: "),
            ("Impossible de supprimer la confiance CA : ", "Could not remove CA trust: "),
            ("Erreur de configuration : ", "Configuration error: "),
            ("Erreur de nettoyage du proxy : ", "Proxy cleanup error: "),
            ("Erreur de persistance des réglages de simulation : ", "Simulation settings persistence error: "),
            ("Erreur de persistance de l’état : ", "State persistence error: "),
            ("Simulation enregistrée ignorée : ", "Skipped saved simulation: "),
            ("Hash info copié dans le presse-papiers : ", "Copied info hash to clipboard: "),
            ("Statistiques réinitialisées pour le hash du torrent : ", "Reset stats for torrent hash: "),
            ("Réinitialiser toutes les statistiques suivies pour ", "Reset all tracked statistics for "),
            ("Supprimer la simulation arrêtée «", "Remove the stopped simulation “"),
            ("Erreur de démarrage : ", "Startup error: "),
            ("Supprimé ", "Removed "),
            ("Restauré ", "Restored ")
        ], out var english)
            ? english
            : TranslateProgressMessage(message, toFrench: false);
    }

    private static bool ReplacePrefix(
        string message,
        IReadOnlyList<(string From, string To)> replacements,
        out string translated)
    {
        foreach (var (from, to) in replacements)
        {
            if (message.StartsWith(from, StringComparison.Ordinal))
            {
                translated = to + message[from.Length..];
                return true;
            }
        }

        translated = message;
        return false;
    }

    private static string TranslateProgressMessage(string message, bool toFrench)
    {
        if (toFrench && message.StartsWith("Downloaded ", StringComparison.Ordinal))
        {
            var rest = message["Downloaded ".Length..]
                .Replace(" of ", " sur ", StringComparison.Ordinal)
                .Replace(" at ", " à ", StringComparison.Ordinal);
            return "Téléchargé " + rest;
        }
        if (toFrench && message.StartsWith("Uploaded ", StringComparison.Ordinal))
            return "Envoyé " + message["Uploaded ".Length..].Replace(" at ", " à ", StringComparison.Ordinal);
        if (!toFrench && message.StartsWith("Téléchargé ", StringComparison.Ordinal))
        {
            var rest = message["Téléchargé ".Length..]
                .Replace(" sur ", " of ", StringComparison.Ordinal)
                .Replace(" à ", " at ", StringComparison.Ordinal);
            return "Downloaded " + rest;
        }
        if (!toFrench && message.StartsWith("Envoyé ", StringComparison.Ordinal))
            return "Uploaded " + message["Envoyé ".Length..].Replace(" à ", " at ", StringComparison.Ordinal);
        if (toFrench && message.StartsWith("Peers: ", StringComparison.Ordinal))
            return "Pairs : " + message["Peers: ".Length..].Replace("Next announce: ", "Prochaine annonce : ", StringComparison.Ordinal);
        if (!toFrench && message.StartsWith("Pairs : ", StringComparison.Ordinal))
            return "Peers: " + message["Pairs : ".Length..].Replace("Prochaine annonce : ", "Next announce: ", StringComparison.Ordinal);
        return message;
    }

    private static string TranslateSimulationStatus(string message, bool toFrench)
    {
        var statuses = toFrench
            ? new[]
            {
                ("●  Running", "●  En cours"),
                ("▶  Starting", "▶  Démarrage…"),
                ("■  Stopping", "■  Arrêt en cours"),
                ("!  Error", "!  Erreur"),
                ("■  Stopped", "■  Arrêté")
            }
            : new[]
            {
                ("●  En cours", "●  Running"),
                ("▶  Démarrage…", "▶  Starting"),
                ("■  Arrêt en cours", "■  Stopping"),
                ("!  Erreur", "!  Error"),
                ("■  Arrêté", "■  Stopped")
            };

        return ReplacePrefix(message, statuses, out var translated)
            ? translated
            : message;
    }
}
