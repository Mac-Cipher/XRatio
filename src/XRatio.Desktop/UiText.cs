using System.Globalization;
using System.Text.RegularExpressions;

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

    /// <summary>
    /// Canonical English keys used by the desktop surface. Keeping this list
    /// public makes it possible for the localization contract test to catch a
    /// newly added label before it reaches a packaged build.
    /// </summary>
    public static IReadOnlyCollection<string> TranslationKeys => FrenchMap.Keys.ToArray();

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
            ["Reset to defaults"] = "Réinitialiser par défaut",
            ["Reset settings"] = "Réinitialiser les réglages",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "Réinitialiser tous les réglages configurables par défaut ? Les statistiques des torrents suivis, la progression de l’onboarding et les sessions de simulation seront conservées.",
            ["To tray"] = "Réduire dans la zone de notification",
            ["Close"] = "Fermer",
            ["Open Settings"] = "Ouvrir les réglages",
            ["Report a bug"] = "Signaler un bug",
            ["Report a bug on GitHub"] = "Signaler un bug sur GitHub",
            ["Could not open bug report"] = "Impossible d’ouvrir le formulaire de signalement",
            ["Open GitHub in browser"] = "Ouvrir GitHub dans le navigateur",
            ["This will open the XRatio GitHub page in your default browser."] = "Cette action va ouvrir la page GitHub de XRatio dans votre navigateur par défaut.",
            ["Open XRatio on GitHub"] = "Ouvrir XRatio sur GitHub",
            ["Could not open GitHub repository"] = "Impossible d’ouvrir le dépôt GitHub",
            ["Open bug report in browser"] = "Ouvrir le signalement dans le navigateur",
            ["This will open the GitHub issue form in your default browser."] = "Cette action va ouvrir le formulaire de signalement GitHub dans votre navigateur par défaut.",
            ["Open update in browser"] = "Ouvrir la mise à jour dans le navigateur",
            ["This will open the verified update download in your default browser."] = "Cette action va ouvrir le téléchargement vérifié de la mise à jour dans votre navigateur par défaut.",
            ["Install update"] = "Installer la mise à jour",
            ["This will download and install the verified Windows update, then restart XRatio."] = "Cette action va télécharger et installer la mise à jour Windows vérifiée, puis redémarrer XRatio.",
            ["Open browser"] = "Ouvrir le navigateur",
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
            ["Timer (minutes)"] = "Minuteur (minutes)",
            ["Timer"] = "Minuteur",
            ["Minutes"] = "Minutes",
            ["Hours"] = "Heures",
            ["Duration"] = "Durée",
            ["MiB"] = "MiB",
            ["Not used"] = "Non utilisé",
            ["Timer starts when Start is pressed and stops this session after the selected duration."] = "Le minuteur démarre quand vous appuyez sur Démarrer et arrête cette session après la durée choisie.",
            ["Stop automatically after this session uploads the selected amount."] = "Arrêter automatiquement après l’envoi de la quantité choisie.",
            ["Stop automatically after this session downloads the selected amount."] = "Arrêter automatiquement après la réception de la quantité choisie.",
            ["Stop automatically when the selected upload/download ratio is reached."] = "Arrêter automatiquement lorsque le ratio upload/download choisi est atteint.",
            ["Leave Never selected for manual stopping, or choose a rule above to stop automatically."] = "Laissez Jamais pour arrêter manuellement, ou choisissez une règle ci-dessus pour arrêter automatiquement.",
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
            ["Stop value"] = "Valeur d’arrêt",
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
            ["Torrent name · tracker · peers · status · transfer counters · last announce"] = "Nom du torrent · tracker · pairs · état · compteurs de transfert · dernière annonce",
            ["Hash · tracker · peers · status · transfer counters · last announce"] = "Hash · tracker · pairs · état · compteurs de transfert · dernière annonce",
            ["Tracked sessions stay visible here as announcements arrive."] = "Les sessions suivies restent visibles ici à mesure que les annonces arrivent.",
            ["Appearance"] = "Apparence",
            ["Updates"] = "Mises à jour",
            ["Check GitHub and install a verified Windows update automatically when one is available."] = "Vérifiez GitHub et installez automatiquement une mise à jour Windows vérifiée lorsqu’elle est disponible.",
            ["Check the official GitHub release without changing files automatically."] = "Vérifiez la release officielle GitHub sans modifier automatiquement les fichiers.",
            ["Current version"] = "Version actuelle",
            ["Check for updates"] = "Rechercher les mises à jour",
            ["Download update"] = "Télécharger la mise à jour",
            ["Update"] = "Mise à jour",
            ["Download the new version"] = "Télécharger la nouvelle version",
            ["Check for updates at startup"] = "Rechercher les mises à jour au démarrage",
            ["Checks GitHub automatically when XRatio starts."] = "Vérifie automatiquement GitHub au démarrage de XRatio.",
            ["Not checked yet"] = "Pas encore vérifié",
            ["Checking for updates…"] = "Recherche de mises à jour…",
            ["You are up to date"] = "Vous utilisez la dernière version",
            ["Unable to check for updates"] = "Impossible de rechercher les mises à jour",
            ["Update available: {0}"] = "Mise à jour disponible : {0}",
            ["Downloading update…"] = "Téléchargement de la mise à jour…",
            ["Installing update…"] = "Installation de la mise à jour…",
            ["Automatic update failed"] = "La mise à jour automatique a échoué",
            ["Automatic update is unavailable for this release"] = "La mise à jour automatique n’est pas disponible pour cette release",
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
            ["Changes the visual theme without changing proxy behavior."] = "Change le thème visuel sans modifier le comportement du proxy.",
            ["Changes the interface accent color without changing proxy behavior."] = "Change la couleur d’accentuation sans modifier le comportement du proxy.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Choisit si l’icône de notification utilise des états colorés ou le mode monochrome.",
            ["Changes the language used by the XRatio interface."] = "Change la langue utilisée par l’interface XRatio.",
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
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "Le port localhost utilisé par le proxy HTTP XRatio. Gardez-le libre et utilisez le même port dans qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Nombre minimal de pairs incomplets requis avant que le réglage du ratio ajoute de l’upload calculé.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Bloque le trafic qui n’est pas celui d’un tracker pour que XRatio reste centré sur les annonces.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Garde le proxy lié à localhost. Cette limite de sécurité obligatoire ne peut pas être désactivée.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Écrit les diagnostics anonymisés du proxy dans %APPDATA%\\XRatio\\proxy_debug.log. Les journaux sont conservés jusqu’à 7 jours et renouvelés à 1 MiB. Activez cette option uniquement pour diagnostiquer un problème.",
            ["Ratio shaping"] = "Réglage du ratio",
            ["Connection, ratio shaping and reporting options are grouped by purpose."] = "Les options de connexion, de réglage du ratio et de reporting sont regroupées par fonction.",
            ["Minimum values must not exceed maximum values. Changing these values affects tracker reporting; use Pause or Stop for temporary control."] = "Les minima ne doivent pas dépasser les maxima. Modifier ces valeurs affecte le reporting au tracker ; utilisez Pause ou Arrêter pour un contrôle temporaire.",
            ["Upload/download multiplier min"] = "Multiplicateur upload/download min",
            ["Upload/download multiplier max"] = "Multiplicateur upload/download max",
            ["Upload/upload multiplier min"] = "Multiplicateur upload/upload min",
            ["Upload/upload multiplier max"] = "Multiplicateur upload/upload max",
            ["Boost maximum (KiB/s)"] = "Boost maximum (KiB/s)",
            ["Boost chance (%)"] = "Chance de boost (%)",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Borne basse de l’upload crédité pour chaque téléchargement réel pendant le réglage des annonces.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Borne haute de l’upload crédité pour chaque téléchargement réel pendant le réglage des annonces.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Borne basse du multiplicateur appliqué à l’upload réel.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Borne haute du multiplicateur appliqué à l’upload réel.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Boost d’upload supplémentaire maximal utilisé pendant une annonce réglée, en KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Probabilité, de 0 à 100 %, d’appliquer le boost d’upload supplémentaire.",
            ["Boost chance"] = "Chance de boost",
            ["Update interval"] = "Intervalle de mise à jour",
            ["Rate is outside the supported range."] = "Le débit est en dehors de la plage prise en charge.",
            ["Change ratio shaping"] = "Modifier le réglage du ratio",
            ["These values change the upload/download data XRatio announces to trackers. Change them only for an authorized, understood purpose; use Pause or Stop for temporary control."] = "Ces valeurs modifient les données upload/download qu’XRatio annonce aux trackers. Ne les changez que pour un usage autorisé et compris ; utilisez Pause ou Arrêter pour un contrôle temporaire.",
            ["I understand"] = "J’ai compris",
            ["Announce behavior"] = "Comportement des annonces",
            ["Download reporting stays at zero; use Pause or Stop to suspend announcements."] = "Le téléchargement annoncé reste à zéro ; utilisez Pause ou Arrêter pour suspendre les annonces.",
            ["Choose the information the proxy reports to trackers."] = "Choisissez les informations que le proxy annonce aux trackers.",
            ["Report download as zero"] = "Annoncer un téléchargement nul",
            ["Pretend to seed (completed torrents only)"] = "Simuler le seeding (torrents terminés uniquement)",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Toujours activé : annonce zéro octet téléchargé. Utilisez Pause ou Arrêter pour suspendre la réécriture.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "N’augmente pas votre ratio. Lorsque l’option est activée, les torrents terminés sont annoncés avec left=0 afin que le tracker les voie comme des seeds ; les téléchargements actifs conservent leur quantité restante.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Restaure les réglages configurables par défaut. Les torrents suivis, les statistiques, la progression de l’onboarding et les sessions de simulation sont conservés.",
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
            ["Select a simulation session first."] = "Sélectionnez d’abord une session de simulation.",
            ["This exact simulation already exists; selected the existing session."] = "Cette simulation existe déjà ; la session existante est sélectionnée.",
            ["HTTPS interception enabled for the current Windows user."] = "Interception HTTPS activée pour l’utilisateur Windows actuel.",
            ["HTTPS was not enabled: explicit CA trust confirmation is required."] = "L’interception HTTPS n’a pas été activée : la confirmation explicite de confiance envers la CA est requise.",
            ["XRatio CA trust removed from the current Windows user."] = "La confiance envers la CA XRatio a été retirée pour l’utilisateur Windows actuel.",
            ["New entries are added as proxy decisions, imports and simulation actions happen."] = "Les nouvelles entrées apparaissent au fil des décisions du proxy, des imports et des actions de simulation.",
            ["Scroll to review recent events; the list keeps the latest 500 entries."] = "Faites défiler pour consulter les événements récents ; la liste conserve les 500 dernières entrées.",
            ["Startup"] = "Démarrage",
            ["Starts XRatio automatically with your Windows session."] = "Démarre XRatio automatiquement avec votre session Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Garde une icône XRatio dans la zone de notification Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Démarre XRatio masqué dans la zone de notification au lieu d’ouvrir la fenêtre principale.",
            ["Choose how XRatio should behave when your session begins."] = "Choisissez le comportement de XRatio au démarrage de votre session.",
            ["Start automatically with the user session"] = "Démarrer automatiquement avec la session utilisateur",
            ["Show icon in notification area"] = "Afficher l’icône dans la zone de notification",
            ["Start minimized to tray"] = "Démarrer réduit dans la zone de notification",
            ["HTTPS interception"] = "Interception HTTPS",
            ["The installation CA is used only to inspect HTTPS tracker traffic through the local proxy."] = "La CA d’installation sert uniquement à inspecter le trafic HTTPS des trackers via le proxy local.",
            ["Trust is explicit and scoped to the current Windows user."] = "La confiance est explicite et limitée à l’utilisateur Windows actuel.",
            ["I understand that XRatio will add its installation CA to my Windows user trust store."] = "Je comprends que XRatio va ajouter sa CA d’installation au magasin de confiance de mon utilisateur Windows.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Confirmez que XRatio peut ajouter sa CA locale au magasin de confiance de l’utilisateur Windows actuel pour l’interception HTTPS.",
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
            ["Route tracker announces through the local XRatio HTTP proxy before checking the ratio."] = "Faites passer les annonces des trackers par le proxy HTTP local XRatio avant de vérifier le ratio.",
            ["Start XRatio and verify that the header shows HTTP/HTTPS active on 127.0.0.1:3773."] = "Démarrez XRatio et vérifiez que l’en-tête indique HTTP/HTTPS actif sur 127.0.0.1:3773.",
            ["In qBittorrent, open Tools > Options > Connection."] = "Dans qBittorrent, ouvrez Outils > Options > Connexion.",
            ["Under Proxy Server, choose HTTP, set Host to 127.0.0.1 and Port to 3773."] = "Dans Proxy Server, choisissez HTTP, indiquez 127.0.0.1 comme hôte et 3773 comme port.",
            ["Enable Perform hostname lookup via proxy and Use proxy for BitTorrent purposes. Leave Use proxy for peer connections disabled because XRatio handles tracker announces only."] = "Activez Perform hostname lookup via proxy et Use proxy for BitTorrent purposes. Laissez Use proxy for peer connections désactivé, car XRatio ne traite que les annonces des trackers.",
            ["For Deluge, Transmission, Tixati, BiglyBT, Vuze or another client, open Settings/Preferences and find Connection, Network or Proxy. Use HTTP, server 127.0.0.1 and port 3773. Enable tracker/BitTorrent proxying and leave peer connections disabled when that option is separate."] = "Pour Deluge, Transmission, Tixati, BiglyBT, Vuze ou un autre client, ouvrez Réglages/Préférences et cherchez Connexion, Réseau ou Proxy. Utilisez HTTP, le serveur 127.0.0.1 et le port 3773. Activez le proxy tracker/BitTorrent et laissez les connexions aux pairs désactivées si cette option est séparée.",
            ["In XRatio Settings > Announce behavior, download reporting is kept at zero. Use Pause or Stop when you need to suspend announce rewriting; Pretend to seed remains optional."] = "Dans Réglages XRatio > Comportement des annonces, le téléchargement annoncé reste à zéro. Utilisez Pause ou Arrêter pour suspendre la réécriture des annonces ; Simuler le seeding reste facultatif.",
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
            ["Get started"] = "Pour commencer",
            ["Settings →"] = "Réglages →",
            ["Guide →"] = "Guide →",
            ["Platform →"] = "Plateforme →",
            ["Start →"] = "Démarrer →",
            ["Interception →"] = "Interception →",
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
            ["Multipliers and boost values cannot be negative."] = "Les multiplicateurs et valeurs de boost ne peuvent pas être négatifs.",
            ["Step"] = "Étape",
            ["of"] = "sur",
            ["Quick setup"] = "Configuration rapide",
            ["Five small steps to get XRatio ready."] = "Cinq petites étapes pour préparer XRatio.",
            ["Replay from Settings"] = "Relancer depuis les Réglages",
            ["Review the local proxy"] = "Vérifier le proxy local",
            ["Connect your torrent client"] = "Configurer votre client torrent",
            ["Open qBittorrent →"] = "Ouvrir qBittorrent →",
            ["Setup guide →"] = "Guide de configuration →",
            ["Mark as configured"] = "Marquer comme configuré",
            ["Confirm the localhost port that qBittorrent will use."] = "Confirmez le port localhost qu’utilisera qBittorrent.",
            ["Open Settings → Connection, keep localhost-only enabled, then save your changes."] = "Ouvrez Réglages → Connexion, gardez localhost activé, puis enregistrez vos changements.",
            ["Connect qBittorrent"] = "Connecter qBittorrent",
            ["Route tracker announces through XRatio before checking activity."] = "Faites passer les annonces des trackers par XRatio avant de vérifier l’activité.",
            ["In qBittorrent, choose HTTP proxy 127.0.0.1 and the same port as XRatio. Peer traffic stays outside this proxy."] = "Dans qBittorrent, choisissez le proxy HTTP 127.0.0.1 et le même port que XRatio. Le trafic pair-à-pair reste en dehors de ce proxy.",
            ["Open qBittorrent guide"] = "Ouvrir le guide qBittorrent",
            ["Enable HTTPS when needed"] = "Activer HTTPS si nécessaire",
            ["Trust XRatio’s local CA only when you need HTTPS tracker interception."] = "N’approuvez la CA locale de XRatio que si vous avez besoin de l’interception HTTPS des trackers.",
            ["Open Platform → HTTPS interception. HTTP works without this optional step."] = "Ouvrez Plateforme → Interception HTTPS. HTTP fonctionne sans cette étape facultative.",
            ["Open Platform"] = "Ouvrir la plateforme",
            ["Keep the local proxy active while qBittorrent announces."] = "Gardez le proxy local actif pendant les annonces de qBittorrent.",
            ["The step checks itself when the header reports HTTP/HTTPS active on 127.0.0.1."] = "L’étape se coche lorsque l’en-tête indique HTTP/HTTPS actif sur 127.0.0.1.",
            ["Use interception"] = "Utiliser Interception",
            ["Open the live tracker view and learn what appears there."] = "Ouvrez la vue des trackers en direct et découvrez comment la lire.",
            ["Torrent · tracker · peers · counters · last announce"] = "Torrent · tracker · pairs · compteurs · dernière annonce",
            ["Show me how"] = "Montrez-moi comment",
            ["Show me how →"] = "Voir comment l’utiliser →",
            ["Use simulation"] = "Utiliser Simulation",
            ["Create and run an independent tracker session from a .torrent file."] = "Créez et lancez une session tracker indépendante depuis un fichier .torrent.",
            ["Choose file · add session · start · monitor"] = "Choisir le fichier · ajouter · démarrer · surveiller",
            ["Confirm the port used by qBittorrent."] = "Confirmez le port utilisé par qBittorrent.",
            ["Settings → Connection · localhost only"] = "Réglages → Connexion · localhost uniquement",
            ["Use XRatio as qBittorrent’s HTTP tracker proxy."] = "Utilisez XRatio comme proxy HTTP des trackers de qBittorrent.",
            ["HTTP · 127.0.0.1 · same port"] = "HTTP · 127.0.0.1 · même port",
            ["qBittorrent detected. Open it to configure the proxy."] = "qBittorrent a été détecté. Ouvrez-le pour configurer le proxy.",
            ["Detected locally · ready to open"] = "Détecté localement · prêt à ouvrir",
            ["qBittorrent was not found. Follow the guide to install or configure it."] = "qBittorrent est introuvable. Suivez le guide pour l’installer ou le configurer.",
            ["Not detected · use the guide"] = "Non détecté · utilisez le guide",
            ["Open qBittorrent"] = "Ouvrir qBittorrent",
            ["Optional: trust the local CA for HTTPS trackers."] = "Facultatif : approuvez la CA locale pour les trackers HTTPS.",
            ["Platform → HTTPS interception"] = "Plateforme → Interception HTTPS",
            ["GET STARTED"] = "POUR COMMENCER",
            ["ONBOARDING"] = "PARCOURS GUIDÉ",
            ["Header status turns active"] = "L’état de l’en-tête devient actif",
            ["Completed"] = "Terminé",
            ["To do"] = "À faire",
            ["Done"] = "Fait",
            ["Mark as done"] = "Marquer comme fait",
            ["Next step  →"] = "Étape suivante  →",
            ["Finish"] = "Terminer",
            ["←  Back"] = "←  Retour",
            ["Close onboarding"] = "Fermer l’onboarding",
            ["Complete this step or use × to close."] = "Terminez cette étape ou utilisez × pour fermer.",
            ["Onboarding"] = "Onboarding",
            ["Replay the guided setup at any time. Your completed steps stay checked."] = "Relancez le parcours guidé à tout moment. Les étapes terminées restent cochées.",
            ["Show onboarding again"] = "Afficher l’onboarding",
            ["Loading onboarding…"] = "Chargement de l’onboarding…",
            ["Onboarding is hidden. You can show it again whenever you need it."] = "L’onboarding est masqué. Vous pouvez l’afficher à nouveau quand vous le souhaitez.",
            ["The onboarding focus panel is hidden. Use the sidebar checklist or show it again from Settings."] = "La fenêtre de guidage est masquée. Utilisez la checklist latérale ou réaffichez-la depuis les Réglages.",
            ["Onboarding is available from the first run and stays here until you close it."] = "L’onboarding est disponible au premier lancement et reste affiché jusqu’à sa fermeture.",
            ["Onboarding closed."] = "Onboarding fermé.",
            ["Onboarding reopened."] = "Onboarding rouvert.",
            ["qBittorrent opened from onboarding."] = "qBittorrent a été ouvert depuis l’onboarding.",
            ["Could not open qBittorrent. Use the qBittorrent guide instead."] = "Impossible d’ouvrir qBittorrent. Utilisez plutôt le guide qBittorrent.",
            ["qBITTORRENT — PROXY SERVER (FULL VIEW)"] = "qBITTORRENT — SERVEUR PROXY (VUE COMPLÈTE)",
            ["OTHER CLIENTS — DELUGE, TRANSMISSION, TIXATI, BIGLYBT, VUZE…"] = "AUTRES CLIENTS — DELUGE, TRANSMISSION, TIXATI, BIGLYBT, VUZE…",
            ["1. Open Settings/Preferences > Connection, Network or Proxy."] = "1. Ouvrez Préférences/Réglages > Connexion, Réseau ou Proxy.",
            ["2. Select HTTP. Enter server 127.0.0.1 and port {0}."] = "2. Sélectionnez HTTP. Saisissez le serveur 127.0.0.1 et le port {0}.",
            ["3. Enable the proxy for tracker/BitTorrent traffic. Leave peer connections disabled when that option is separate."] = "3. Activez le proxy pour les trackers/BitTorrent. Laissez les connexions aux pairs désactivées si cette option est séparée.",
            ["1. Open Tools > Options > Connection."] = "1. Ouvrez Outils > Options > Connexion.",
            ["2. In Proxy Server, select Type: HTTP."] = "2. Dans Serveur proxy, choisissez Type : HTTP.",
            ["3. Enter Host: 127.0.0.1 and Port: {0}."] = "3. Saisissez Hôte : 127.0.0.1 et Port : {0}.",
            ["4. Enable “Use proxy for BitTorrent purposes”, leave peer connections disabled, then click Apply."] = "4. Cochez « Utiliser le proxy pour BitTorrent », laissez les connexions aux pairs décochées, puis cliquez sur Appliquer.",
            ["TYPE  HTTP     HOST  127.0.0.1     PORT  {0}"] = "TYPE  HTTP     HÔTE  127.0.0.1     PORT  {0}",
            [" · qBittorrent detected"] = " · qBittorrent détecté",
            [" · qBittorrent not detected; use the guide"] = " · qBittorrent non détecté ; utilisez le guide",
            ["Platform > HTTPS interception"] = "Plateforme > Interception HTTPS",
            ["How to use Interception"] = "Comment utiliser Interception",
            ["1  Start or refresh a torrent in your client."] = "1  Lancez ou actualisez un torrent dans votre client.",
            ["2  Its tracker announce appears in this list automatically."] = "2  Son annonce tracker apparaît automatiquement dans cette liste.",
            ["3  Select a row to read the tracker, peers and transfer counters."] = "3  Sélectionnez une ligne pour lire le tracker, les pairs et les compteurs.",
            ["4  Right-click a row to copy its info hash or reset its statistics."] = "4  Clic droit : copier le hash info ou réinitialiser les statistiques.",
            ["Nothing appears? Check HTTP proxy 127.0.0.1:{0} in your client and that the header says Active."] = "Rien n’apparaît ? Vérifiez le proxy HTTP 127.0.0.1:{0} dans votre client et que l’en-tête indique Actif.",
            ["Nothing appears? Check that your torrent client uses XRatio’s HTTP proxy and that the header says Active."] = "Rien n’apparaît ? Vérifiez que votre client torrent utilise le proxy HTTP de XRatio et que l’en-tête indique Actif.",
            ["Got it"] = "J’ai compris",
            ["How to use Simulation"] = "Comment utiliser Simulation",
            ["1  Choose a .torrent file and check the detected tracker."] = "1  Choisissez un fichier .torrent et vérifiez le tracker détecté.",
            ["2  Set the client profile, ratios and transfer speeds."] = "2  Réglez le profil client, les ratios et les vitesses de transfert.",
            ["3  Click Add session, select it in the list, then press Start."] = "3  Cliquez sur Ajouter la session, sélectionnez-la, puis sur Démarrer.",
            ["4  Use Manual update while it runs; press Stop when finished."] = "4  Utilisez Mise à jour manuelle pendant l’exécution ; Arrêter termine la session.",
            ["Adding only saves the session. The tracker is contacted when you press Start."] = "Ajouter enregistre seulement la session. Le tracker est contacté quand vous cliquez sur Démarrer.",
            ["Min"] = "Min",
            ["Max"] = "Max",
            ["seeders"] = "seeders",
            ["leechers"] = "leechers",
            ["{0} seeders"] = "{0} seeders",
            ["{0} leechers"] = "{0} leechers",
            ["Downloaded {0} of {1}"] = "Reçu {0} sur {1}",
            ["{0} · {1}/{2} peers · {3} · last {4}"] = "{0} · {1}/{2} pairs · {3} · dernière annonce {4}",
            ["Actual ↓ {0} ↑ {1} left {2}   ·   Reported ↓ {3} ↑ {4} left {5}"] = "Réel ↓ {0} ↑ {1} restant {2}   ·   Annoncé ↓ {3} ↑ {4} restant {5}",
            ["●  Running"] = "●  En cours",
            ["▶  Starting"] = "▶  Démarrage…",
            ["■  Stopping"] = "■  Arrêt en cours",
            ["!  Error"] = "!  Erreur",
            ["■  Stopped"] = "■  Arrêté",
            ["Autostart"] = "Démarrage automatique",
            ["Certificates"] = "Certificats",
            ["Windows per-user startup registry entry."] = "Entrée de démarrage Windows par utilisateur.",
            ["Autostart is not implemented or tested on this operating system."] = "Le démarrage automatique n’est pas implémenté ou testé sur ce système d’exploitation.",
            ["HTTPS MITM is disabled until per-installation CA generation and explicit OS trust are implemented."] = "L’interception HTTPS est désactivée tant que la génération d’une CA par installation et la confiance explicite du système ne sont pas implémentées.",
            ["Windows CurrentUser certificate stores; trust is installed only after explicit confirmation."] = "Magasins de certificats CurrentUser de Windows ; la confiance n’est installée qu’après confirmation explicite.",
            ["Linux XDG autostart desktop entry (tested under Ubuntu 20.04 WSL; full desktop integration remains unverified)."] = "Entrée de démarrage Linux XDG (testée sous Ubuntu 20.04 WSL ; l’intégration complète au bureau reste non vérifiée).",
            ["macOS LaunchAgent (tested file integration; native session launch pending on macOS)."] = "LaunchAgent macOS (intégration des fichiers testée ; lancement natif de session en attente sur macOS).",
            ["Select a torrent before copying its info hash."] = "Sélectionnez un torrent avant de copier son hash info.",
            ["Select a torrent before resetting its statistics."] = "Sélectionnez un torrent avant de réinitialiser ses statistiques.",
            ["Simulation added: {0}. Press Start to contact the tracker."] = "Simulation ajoutée : {0}. Appuyez sur Démarrer pour contacter le tracker.",
            ["Loaded torrent: {0} · {1} · {2} tracker(s)."] = "Torrent chargé : {0} · {1} · {2} tracker(s).",
            ["Reset all tracked statistics for {0}?"] = "Réinitialiser toutes les statistiques suivies pour {0} ?",
            ["Configuration reset to defaults."] = "Réglages réinitialisés par défaut.",
            ["Interception is stopped. {0}"] = "L’interception est arrêtée. {0}",
            ["HTTP/HTTPS active on 127.0.0.1:{0}"] = "HTTP/HTTPS actif sur 127.0.0.1:{0}",
            ["HTTP active on 127.0.0.1:{0}"] = "HTTP actif sur 127.0.0.1:{0}",
            ["Paused on 127.0.0.1:{0}"] = "En pause sur 127.0.0.1:{0}",
            ["Open setup guide"] = "Ouvrir le guide de configuration",
            ["Configure qBittorrent or another torrent client to use XRatio as its HTTP proxy."] = "Configurez qBittorrent ou un autre client torrent pour utiliser XRatio comme proxy HTTP.",
            ["Host 127.0.0.1 · use the XRatio port"] = "Hôte 127.0.0.1 · utilisez le port XRatio",
            ["HTTP/HTTPS active on"] = "HTTP/HTTPS actif sur",
            ["HTTP active on"] = "HTTP actif sur",
            ["Paused on"] = "En pause sur",
            ["Interception is stopped."] = "L’interception est arrêtée.",
            ["Simulation added:"] = "Simulation ajoutée :",
            ["Simulation start failed:"] = "Échec du démarrage de la simulation :",
            ["Simulation update failed:"] = "Échec de la mise à jour de la simulation :",
            ["Could not add simulation:"] = "Impossible d’ajouter la simulation :",
            ["Could not restore simulation form:"] = "Impossible de restaurer le formulaire de simulation :",
            ["Removed simulation:"] = "Simulation supprimée :",
            ["Loaded torrent:"] = "Torrent chargé :",
            ["Torrent import failed:"] = "Échec de l’import du torrent :",
            ["Could not enable HTTPS:"] = "Impossible d’activer HTTPS :",
            ["Could not remove CA trust:"] = "Impossible de supprimer la confiance CA :",
            ["Configuration error:"] = "Erreur de configuration :",
            ["Proxy cleanup error:"] = "Erreur de nettoyage du proxy :",
            ["Simulation settings persistence error:"] = "Erreur de persistance des réglages de simulation :",
            ["State persistence error:"] = "Erreur de persistance de l’état :",
            ["Skipped saved simulation:"] = "Simulation enregistrée ignorée :",
            ["Copied info hash to clipboard:"] = "Hash info copié dans le presse-papiers :",
            ["Reset stats for torrent hash:"] = "Statistiques réinitialisées pour le hash du torrent :",
            ["Reset all tracked statistics for"] = "Réinitialiser toutes les statistiques suivies pour",
            ["Startup error:"] = "Erreur de démarrage :",
            ["Removed"] = "Supprimé",
            ["Restored"] = "Restauré",
            ["Onboarding close error:"] = "Erreur de fermeture de l’onboarding :",
            ["Onboarding restore error:"] = "Erreur de restauration de l’onboarding :",
            ["Onboarding progress error:"] = "Erreur d’enregistrement de la progression de l’onboarding :",
            ["Could not open qBittorrent:"] = "Impossible d’ouvrir qBittorrent :",
            ["Ratio shaping warning could not be shown:"] = "Impossible d’afficher l’avertissement du réglage du ratio :",
            ["Simulation started."] = "Simulation démarrée.",
            ["Start failed:"] = "Échec du démarrage :",
            ["Stop announce failed:"] = "Échec de l’annonce d’arrêt :",
            ["Simulation stopped."] = "Simulation arrêtée.",
            ["Torrent reached 100%; completed announce sent."] = "Torrent arrivé à 100 % ; annonce de fin envoyée.",
            ["Simulation failed:"] = "Échec de la simulation :",
            ["Forwarded"] = "Transmis",
            ["Rewritten"] = "Réécrit",
            ["BlockedNonTracker"] = "Bloqué (hors tracker)",
            ["RejectedInvalid"] = "Rejeté (invalide)",
            ["Proxy listening on"] = "Proxy à l’écoute sur",
            ["Unexpected connection failure:"] = "Échec inattendu de la connexion :",
            ["Connection header or TLS handshake deadline exceeded."] = "Le délai de l’en-tête de connexion ou de la négociation TLS a été dépassé.",
            ["Connection failed:"] = "Échec de la connexion :",
            ["Tracker connection failed for"] = "Échec de la connexion au tracker pour",
            ["Tracker response failed for"] = "Échec de la réponse du tracker pour",
            ["Tracker connection failed"] = "Échec de la connexion au tracker",
            ["Tracker response failed"] = "Échec de la réponse du tracker",
            ["Tracker response exceeds 4 MiB."] = "La réponse du tracker dépasse 4 MiB.",
            ["Unsupported target scheme."] = "Schéma de destination non pris en charge.",
            ["Blocked non-tracker traffic."] = "Trafic hors tracker bloqué.",
            ["Forwarding non-tracker traffic."] = "Transmission du trafic hors tracker.",
            ["Forwarding non-announce tracker traffic."] = "Transmission du trafic tracker hors annonce.",
            ["Invalid info_hash."] = "info_hash invalide.",
            ["Upload regression rejected to preserve tracker consistency."] = "Régression d’upload rejetée pour préserver la cohérence du tracker.",
            ["The proxy is already running."] = "Le proxy est déjà démarré.",
            ["{0} must be an integer."] = "{0} doit être un entier.",
            ["{0} must be a finite number using '.' as decimal separator."] = "{0} doit être un nombre fini utilisant « . » comme séparateur décimal.",
            ["{0} must be greater than zero."] = "{0} doit être supérieur à zéro.",
            ["{0} must be an absolute URI."] = "{0} doit être une URI absolue.",
            ["Timer duration must be greater than zero."] = "La durée du minuteur doit être supérieure à zéro.",
            ["Timer duration is too small."] = "La durée du minuteur est trop courte.",
            ["Onboarding could not be closed:"] = "Impossible de fermer l’onboarding :",
            ["Onboarding could not be restored:"] = "Impossible de restaurer l’onboarding :",
            ["Onboarding progress could not be saved:"] = "Impossible d’enregistrer la progression de l’onboarding :",
            ["Port {0} is already in use. Interception is stopped until you choose a free port or close the other listener."] = "Le port {0} est déjà utilisé. L’interception est arrêtée jusqu’à ce que vous choisissiez un port libre ou fermiez l’autre écouteur.",
            ["Remove the stopped simulation “{0}”? This does not delete the .torrent file."] = "Supprimer la simulation arrêtée « {0} » ? Cela ne supprime pas le fichier .torrent.",
            ["Removed {0} duplicate saved simulation(s)."] = "{0} simulation(s) enregistrée(s) en double supprimée(s).",
            ["Restored {0} stopped simulation session(s)."] = "{0} session(s) de simulation arrêtée(s) restaurée(s).",
            ["Peers: {0} seeders, {1} leechers. Next announce: {2}."] = "Pairs : {0} seeders, {1} leechers. Prochaine annonce : {2}.",
            ["Uploaded {0} at {1}/s."] = "Envoyé {0} à {1}/s.",
            ["Downloaded {0} at {1}/s."] = "Reçu {0} à {1}/s.",
            ["Torrent"] = "Torrent",
            ["Proxy"] = "Proxy",
            ["HTTPS"] = "HTTPS",
            ["Info"] = "Info",
            ["Success"] = "Succès",
            ["Warning"] = "Avertissement",
            ["Proxy listening on {0}."] = "Proxy à l’écoute sur {0}.",
            ["Unexpected connection failure: {0}"] = "Échec inattendu de la connexion : {0}",
            ["Connection failed: {0}"] = "Échec de la connexion : {0}",
            ["Tracker connection failed for {0}: {1}"] = "Échec de la connexion au tracker pour {0} : {1}",
            ["Tracker response failed for {0}: {1}"] = "Échec de la réponse du tracker pour {0} : {1}",
            ["Show XRatio"] = "Afficher XRatio",
            ["Pause / resume rewriting"] = "Mettre en pause / reprendre la réécriture",
            ["Exit"] = "Quitter",
            ["XRatio — OFF"] = "XRatio — ARRÊTÉ",
            ["XRatio — ON (paused)"] = "XRatio — ACTIVÉ (en pause)",
            ["XRatio — ON"] = "XRatio — ACTIVÉ",
        };

    private static readonly IReadOnlyDictionary<string, string> SpanishMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglés",
            ["French"] = "Francés",
            ["Spanish"] = "Español",
            ["German"] = "Alemán",
            ["Italian"] = "Italiano",
            ["Portuguese"] = "Portugués",
            ["Japanese"] = "Japonés",
            ["Chinese"] = "Chino",
            ["Arabic"] = "Árabe",
            ["Russian"] = "Ruso",
            ["Overview"] = "Vista general",
            ["Interception"] = "Interceptación",
            ["Simulation"] = "Simulación",
            ["Activity"] = "Actividad",
            ["Settings"] = "Ajustes",
            ["Platform"] = "Plataforma",
            ["Monitoring"] = "Supervisión",
            ["Control"] = "Control",
            ["System"] = "Sistema",
            ["Support"] = "Ayuda",
            ["Guide"] = "Guía",
            ["LOCAL RATIO CONTROL"] = "CONTROL DE RATIO LOCAL",
            ["LOCAL / MONITORING"] = "LOCAL / SUPERVISIÓN",
            ["Loading configuration…"] = "Cargando configuración…",
            ["Start"] = "Iniciar",
            ["Stop"] = "Detener",
            ["Retry"] = "Reintentar",
            ["Pause"] = "Pausa",
            ["Resume"] = "Reanudar",
            ["Save changes"] = "Guardar cambios",
            ["Reset to defaults"] = "Restablecer valores predeterminados",
            ["Reset settings"] = "Restablecer ajustes",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "¿Restablecer todos los ajustes configurables a sus valores predeterminados? Se conservarán las estadísticas de torrents seguidos, el progreso de incorporación y las sesiones de simulación.",
            ["To tray"] = "Minimizar a la bandeja",
            ["Close"] = "Cerrar",
            ["Open Settings"] = "Abrir ajustes",
            ["Current runtime status."] = "Estado actual del servicio.",
            ["PROXY CHANNEL"] = "CANAL PROXY",
            ["Local tracker interception · HTTP / HTTPS"] = "Interceptación local de trackers · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrents seguidos",
            ["Announcements observed"] = "Anuncios observados",
            ["Simulations"] = "Simulaciones",
            ["Active / configured"] = "Activas / configuradas",
            ["Reported upload"] = "Upload anunciado",
            ["Current session"] = "Sesión actual",
            ["OPERATING MODES"] = "MODOS DE OPERACIÓN",
            ["Two paths, one local control plane."] = "Dos rutas, un solo plano de control local.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Solo anuncios de trackers — los payloads y el tráfico entre pares permanecen intactos.",
            ["Appearance"] = "Apariencia",
            ["Updates"] = "Actualizaciones",
            ["Check the official GitHub release without changing files automatically."] = "Busca la versión oficial de GitHub sin cambiar archivos automáticamente.",
            ["Current version"] = "Versión actual",
            ["Check for updates"] = "Buscar actualizaciones",
            ["Not checked yet"] = "Aún no comprobado",
            ["Checking for updates…"] = "Buscando actualizaciones…",
            ["You are up to date"] = "Está actualizado",
            ["Unable to check for updates"] = "No se pueden buscar actualizaciones",
            ["Update available: {0}"] = "Actualización disponible: {0}",
            ["Theme"] = "Tema",
            ["Light"] = "Claro",
            ["Dim"] = "Tenue",
            ["Soft Dark"] = "Oscuro suave",
            ["Dark"] = "Oscuro",
            ["Changes the visual theme without changing proxy behavior."] = "Cambia el tema visual sin modificar el comportamiento del proxy.",
            ["Changes the interface accent color without changing proxy behavior."] = "Cambia el color de acento de la interfaz sin modificar el comportamiento del proxy.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Elige si el icono del área de notificación usa estados de color o monocromo.",
            ["Changes the language used by the XRatio interface."] = "Cambia el idioma de la interfaz de XRatio.",
            ["Accent color"] = "Color de acento",
            ["Blue"] = "Azul",
            ["Teal"] = "Verde azulado",
            ["Violet"] = "Violeta",
            ["Amber"] = "Ámbar",
            ["Rose"] = "Rosa",
            ["Green"] = "Verde",
            ["Language"] = "Idioma",
            ["Choose the language used by the XRatio interface."] = "Elige el idioma de la interfaz de XRatio.",
            ["Connection"] = "Conexión",
            ["HTTP proxy port"] = "Puerto del proxy HTTP",
            ["Minimum leechers"] = "Leechers mínimos",
            ["Accept tracker traffic only"] = "Aceptar solo tráfico de trackers",
            ["Listen on localhost only (required)"] = "Escuchar solo en localhost (obligatorio)",
            ["Write redacted proxy debug log"] = "Escribir registro de depuración anonimizado",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "El puerto localhost que usa el proxy HTTP de XRatio. Déjalo libre y usa el mismo puerto en qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Número mínimo de pares incompletos necesario para que el ajuste del ratio añada upload calculado.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Bloquea el tráfico que no es de trackers para que XRatio se centre en las solicitudes announce de los trackers.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Mantiene el proxy vinculado a localhost. Este límite de seguridad obligatorio no se puede desactivar.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Escribe diagnósticos anonimizados del proxy en %APPDATA%\\XRatio\\proxy_debug.log. Los archivos de registro se conservan hasta 7 días y rotan al alcanzar 1 MiB. Actívalo solo para solucionar problemas.",
            ["Configuration"] = "Configuración",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Límite inferior del upload acreditado por cada descarga real durante el ajuste de announces.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Límite superior del upload acreditado por cada descarga real durante el ajuste de announces.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Límite inferior del multiplicador de upload aplicado al upload real.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Límite superior del multiplicador de upload aplicado al upload real.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Boost de upload adicional máximo usado durante un announce ajustado, en KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Probabilidad, de 0 a 100, de aplicar el boost de upload adicional.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Siempre activado: anuncia cero bytes descargados. Usa Pausa o Detener para suspender la reescritura.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "No aumenta tu ratio. Al activarla, los torrents completados se anuncian con left=0 para que el tracker los vea como seeds; las descargas activas conservan sus bytes restantes.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Restaura los ajustes configurables a sus valores predeterminados. Se conservan los torrents seguidos, las estadísticas, el progreso de incorporación y las sesiones de simulación.",
            ["Starts XRatio automatically with your Windows session."] = "Inicia XRatio automáticamente con tu sesión de Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Mantiene un icono de XRatio en el área de notificación de Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Inicia XRatio oculto en el área de notificación en lugar de abrir la ventana principal.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Confirma que XRatio puede añadir su CA local al almacén de confianza del usuario actual de Windows para la interceptación HTTPS.",
            ["Configuration saved."] = "Configuración guardada.",
            ["Configuration reset to defaults."] = "Ajustes restablecidos a los valores predeterminados.",
            ["Active"] = "Activo",
            ["Paused"] = "En pausa",
            ["Proxy stopped"] = "Proxy detenido",
            ["Ready"] = "Listo"
        };

    private static readonly IReadOnlyDictionary<string, string> GermanMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Englisch",
            ["French"] = "Französisch",
            ["Spanish"] = "Spanisch",
            ["German"] = "Deutsch",
            ["Italian"] = "Italienisch",
            ["Portuguese"] = "Portugiesisch",
            ["Japanese"] = "Japanisch",
            ["Chinese"] = "Chinesisch",
            ["Arabic"] = "Arabisch",
            ["Russian"] = "Russisch",
            ["Overview"] = "Übersicht",
            ["Interception"] = "Abfangen",
            ["Simulation"] = "Simulation",
            ["Activity"] = "Aktivität",
            ["Settings"] = "Einstellungen",
            ["Platform"] = "Plattform",
            ["Monitoring"] = "Überwachung",
            ["Control"] = "Steuerung",
            ["System"] = "System",
            ["Support"] = "Hilfe",
            ["Guide"] = "Anleitung",
            ["LOCAL RATIO CONTROL"] = "LOKALE RATIO-STEUERUNG",
            ["LOCAL / MONITORING"] = "LOKAL / ÜBERWACHUNG",
            ["Loading configuration…"] = "Konfiguration wird geladen…",
            ["Start"] = "Starten",
            ["Stop"] = "Stoppen",
            ["Retry"] = "Erneut versuchen",
            ["Pause"] = "Pause",
            ["Resume"] = "Fortsetzen",
            ["Save changes"] = "Änderungen speichern",
            ["Reset to defaults"] = "Auf Standard zurücksetzen",
            ["Reset settings"] = "Einstellungen zurücksetzen",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "Alle konfigurierbaren Einstellungen auf ihre Standardwerte zurücksetzen? Verfolgte Torrent-Statistiken, Onboarding-Fortschritt und Simulationssitzungen bleiben erhalten.",
            ["To tray"] = "In den Infobereich minimieren",
            ["Close"] = "Schließen",
            ["Open Settings"] = "Einstellungen öffnen",
            ["Current runtime status."] = "Aktueller Dienststatus.",
            ["PROXY CHANNEL"] = "PROXY-KANAL",
            ["Local tracker interception · HTTP / HTTPS"] = "Lokales Tracker-Abfangen · HTTP / HTTPS",
            ["Tracked torrents"] = "Überwachte Torrents",
            ["Announcements observed"] = "Beobachtete Ankündigungen",
            ["Simulations"] = "Simulationen",
            ["Active / configured"] = "Aktiv / konfiguriert",
            ["Reported upload"] = "Gemeldeter Upload",
            ["Current session"] = "Aktuelle Sitzung",
            ["OPERATING MODES"] = "BETRIEBSMODI",
            ["Two paths, one local control plane."] = "Zwei Wege, eine lokale Steuerung.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Nur Tracker-Ankündigungen — Nutzdaten und Peer-Verkehr bleiben unverändert.",
            ["Appearance"] = "Darstellung",
            ["Updates"] = "Aktualisierungen",
            ["Check the official GitHub release without changing files automatically."] = "Prüfe die offizielle GitHub-Version, ohne Dateien automatisch zu ändern.",
            ["Current version"] = "Aktuelle Version",
            ["Check for updates"] = "Nach Updates suchen",
            ["Not checked yet"] = "Noch nicht geprüft",
            ["Checking for updates…"] = "Suche nach Updates…",
            ["You are up to date"] = "Du bist auf dem neuesten Stand",
            ["Unable to check for updates"] = "Updates konnten nicht geprüft werden",
            ["Update available: {0}"] = "Update verfügbar: {0}",
            ["Theme"] = "Design",
            ["Light"] = "Hell",
            ["Dim"] = "Gedämpft",
            ["Soft Dark"] = "Sanft dunkel",
            ["Dark"] = "Dunkel",
            ["Changes the visual theme without changing proxy behavior."] = "Ändert das visuelle Design, ohne das Proxy-Verhalten zu ändern.",
            ["Changes the interface accent color without changing proxy behavior."] = "Ändert die Akzentfarbe der Oberfläche, ohne das Proxy-Verhalten zu ändern.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Legt fest, ob das Symbol im Infobereich farbige Zustände oder Monochrom verwendet.",
            ["Changes the language used by the XRatio interface."] = "Ändert die Sprache der XRatio-Oberfläche.",
            ["Accent color"] = "Akzentfarbe",
            ["Blue"] = "Blau",
            ["Teal"] = "Türkis",
            ["Violet"] = "Violett",
            ["Amber"] = "Bernstein",
            ["Rose"] = "Rosa",
            ["Green"] = "Grün",
            ["Language"] = "Sprache",
            ["Choose the language used by the XRatio interface."] = "Wähle die Sprache der XRatio-Oberfläche.",
            ["Connection"] = "Verbindung",
            ["HTTP proxy port"] = "HTTP-Proxy-Port",
            ["Minimum leechers"] = "Minimale Leecher",
            ["Accept tracker traffic only"] = "Nur Tracker-Verkehr zulassen",
            ["Listen on localhost only (required)"] = "Nur auf localhost lauschen (erforderlich)",
            ["Write redacted proxy debug log"] = "Anonymisiertes Proxy-Debugprotokoll schreiben",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "Der localhost-Port, den XRatios HTTP-Proxy verwendet. Halte ihn frei und verwende denselben Port in qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Mindestens erforderliche Anzahl unvollständiger Peers, bevor die Ratio-Steuerung berechneten Upload hinzufügt.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Blockiert Nicht-Tracker-Verkehr, damit XRatio auf Tracker-Announce-Anfragen beschränkt bleibt.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Bindet den Proxy an localhost. Diese erforderliche Sicherheitsgrenze kann nicht deaktiviert werden.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Schreibt bereinigte Proxy-Diagnosen nach %APPDATA%\\XRatio\\proxy_debug.log. Protokolldateien werden bis zu 7 Tage aufbewahrt und bei 1 MiB rotiert. Nur zur Fehlersuche aktivieren.",
            ["Configuration"] = "Konfiguration",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Untere Grenze des pro tatsächlichem Download beim Announce-Shaping angerechneten Uploads.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Obere Grenze des pro tatsächlichem Download beim Announce-Shaping angerechneten Uploads.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Untere Grenze des auf den tatsächlichen Upload angewendeten Upload-Multiplikators.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Obere Grenze des auf den tatsächlichen Upload angewendeten Upload-Multiplikators.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Maximaler zusätzlicher Upload-Boost während eines geformten Announces in KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Wahrscheinlichkeit von 0 bis 100 %, dass der zusätzliche Upload-Boost angewendet wird.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Immer aktiviert: meldet null heruntergeladene Bytes. Pause oder Stop verwenden, um die Umschreibung auszusetzen.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "Erhöht dein Ratio nicht. Wenn aktiviert, werden abgeschlossene Torrents mit left=0 gemeldet, damit der Tracker sie als Seeder sieht; aktive Downloads behalten ihre verbleibenden Bytes.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Stellt konfigurierbare Einstellungen auf ihre Standardwerte zurück. Verfolgte Torrents, Statistiken, Onboarding-Fortschritt und Simulationssitzungen bleiben erhalten.",
            ["Starts XRatio automatically with your Windows session."] = "Startet XRatio automatisch mit der Windows-Sitzung.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Behält ein XRatio-Symbol im Windows-Infobereich.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Startet XRatio im Infobereich ausgeblendet, statt das Hauptfenster zu öffnen.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Bestätigt, dass XRatio seine lokale CA für die HTTPS-Abfangung zum Vertrauensspeicher des aktuellen Windows-Benutzers hinzufügen darf.",
            ["Configuration saved."] = "Konfiguration gespeichert.",
            ["Configuration reset to defaults."] = "Konfiguration auf Standardwerte zurückgesetzt.",
            ["Active"] = "Aktiv",
            ["Paused"] = "Pausiert",
            ["Proxy stopped"] = "Proxy gestoppt",
            ["Ready"] = "Bereit"
        };

    private static readonly IReadOnlyDictionary<string, string> ItalianMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglese",
            ["French"] = "Francese",
            ["Spanish"] = "Spagnolo",
            ["German"] = "Tedesco",
            ["Italian"] = "Italiano",
            ["Portuguese"] = "Portoghese",
            ["Japanese"] = "Giapponese",
            ["Chinese"] = "Cinese",
            ["Arabic"] = "Arabo",
            ["Russian"] = "Russo",
            ["Overview"] = "Panoramica",
            ["Interception"] = "Intercettazione",
            ["Simulation"] = "Simulazione",
            ["Activity"] = "Attività",
            ["Settings"] = "Impostazioni",
            ["Platform"] = "Piattaforma",
            ["Monitoring"] = "Monitoraggio",
            ["Control"] = "Controllo",
            ["System"] = "Sistema",
            ["Support"] = "Supporto",
            ["Guide"] = "Guida",
            ["LOCAL RATIO CONTROL"] = "CONTROLLO RATIO LOCALE",
            ["LOCAL / MONITORING"] = "LOCALE / MONITORAGGIO",
            ["Loading configuration…"] = "Caricamento configurazione…",
            ["Start"] = "Avvia",
            ["Stop"] = "Arresta",
            ["Retry"] = "Riprova",
            ["Pause"] = "Pausa",
            ["Resume"] = "Riprendi",
            ["Save changes"] = "Salva modifiche",
            ["Reset to defaults"] = "Ripristina valori predefiniti",
            ["Reset settings"] = "Reimposta impostazioni",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "Ripristinare tutte le impostazioni configurabili ai valori predefiniti? Le statistiche dei torrent monitorati, l’avanzamento dell’onboarding e le sessioni di simulazione saranno conservati.",
            ["To tray"] = "Riduci nell’area di notifica",
            ["Close"] = "Chiudi",
            ["Open Settings"] = "Apri impostazioni",
            ["Current runtime status."] = "Stato attuale del servizio.",
            ["PROXY CHANNEL"] = "CANALE PROXY",
            ["Local tracker interception · HTTP / HTTPS"] = "Intercettazione tracker locale · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrent monitorati",
            ["Announcements observed"] = "Annunci osservati",
            ["Simulations"] = "Simulazioni",
            ["Active / configured"] = "Attive / configurate",
            ["Reported upload"] = "Upload annunciato",
            ["Current session"] = "Sessione corrente",
            ["OPERATING MODES"] = "MODALITÀ OPERATIVE",
            ["Two paths, one local control plane."] = "Due percorsi, un solo piano di controllo locale.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Solo annunci tracker — payload e traffico peer restano invariati.",
            ["Appearance"] = "Aspetto",
            ["Updates"] = "Aggiornamenti",
            ["Check the official GitHub release without changing files automatically."] = "Controlla la release ufficiale GitHub senza modificare automaticamente i file.",
            ["Current version"] = "Versione attuale",
            ["Check for updates"] = "Cerca aggiornamenti",
            ["Not checked yet"] = "Non ancora verificato",
            ["Checking for updates…"] = "Ricerca aggiornamenti…",
            ["You are up to date"] = "È installata l’ultima versione",
            ["Unable to check for updates"] = "Impossibile cercare aggiornamenti",
            ["Update available: {0}"] = "Aggiornamento disponibile: {0}",
            ["Theme"] = "Tema",
            ["Light"] = "Chiaro",
            ["Dim"] = "Attenuato",
            ["Soft Dark"] = "Scuro morbido",
            ["Dark"] = "Scuro",
            ["Changes the visual theme without changing proxy behavior."] = "Cambia il tema visivo senza modificare il comportamento del proxy.",
            ["Changes the interface accent color without changing proxy behavior."] = "Cambia il colore di accento dell’interfaccia senza modificare il comportamento del proxy.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Sceglie se l’icona nell’area di notifica usa stati colorati o monocromatico.",
            ["Changes the language used by the XRatio interface."] = "Cambia la lingua dell’interfaccia XRatio.",
            ["Accent color"] = "Colore accento",
            ["Blue"] = "Blu",
            ["Teal"] = "Verde acqua",
            ["Violet"] = "Viola",
            ["Amber"] = "Ambra",
            ["Rose"] = "Rosa",
            ["Green"] = "Verde",
            ["Language"] = "Lingua",
            ["Choose the language used by the XRatio interface."] = "Scegli la lingua dell’interfaccia XRatio.",
            ["Connection"] = "Connessione",
            ["HTTP proxy port"] = "Porta proxy HTTP",
            ["Minimum leechers"] = "Leecher minimi",
            ["Accept tracker traffic only"] = "Accetta solo traffico tracker",
            ["Listen on localhost only (required)"] = "Ascolta solo su localhost (obbligatorio)",
            ["Write redacted proxy debug log"] = "Scrivi log proxy anonimizzato",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "La porta localhost usata dal proxy HTTP di XRatio. Lasciala libera e usa la stessa porta in qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Numero minimo di peer incompleti richiesto prima che il ratio shaping aggiunga upload calcolato.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Blocca il traffico non proveniente dai tracker, così XRatio resta concentrato sulle richieste announce dei tracker.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Mantiene il proxy associato a localhost. Questo limite di sicurezza obbligatorio non può essere disattivato.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Scrive i diagnostici anonimizzati del proxy in %APPDATA%\\XRatio\\proxy_debug.log. I file di log vengono conservati fino a 7 giorni e ruotati a 1 MiB. Abilita solo durante la risoluzione dei problemi.",
            ["Configuration"] = "Configurazione",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Limite inferiore dell’upload accreditato per ogni download effettivo durante il ratio shaping.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Limite superiore dell’upload accreditato per ogni download effettivo durante il ratio shaping.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Limite inferiore del moltiplicatore di upload applicato all’upload effettivo.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Limite superiore del moltiplicatore di upload applicato all’upload effettivo.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Boost massimo di upload aggiuntivo usato durante un announce regolato, in KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Probabilità, da 0 a 100, di applicare il boost di upload aggiuntivo.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Sempre attivo: segnala zero byte scaricati. Usa Pausa o Arresta per sospendere la riscrittura.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "Non aumenta il tuo ratio. Se attivata, segnala i torrent completati con left=0 così il tracker li vede come seed; i download attivi mantengono i byte rimanenti.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Ripristina le impostazioni configurabili ai valori predefiniti. Torrent monitorati, statistiche, avanzamento dell’onboarding e sessioni di simulazione vengono conservati.",
            ["Starts XRatio automatically with your Windows session."] = "Avvia XRatio automaticamente con la sessione Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Mantiene un’icona XRatio nell’area di notifica di Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Avvia XRatio nascosto nell’area di notifica invece di aprire la finestra principale.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Conferma che XRatio può aggiungere la propria CA locale all’archivio attendibile dell’utente Windows corrente per l’intercettazione HTTPS.",
            ["Configuration saved."] = "Configurazione salvata.",
            ["Configuration reset to defaults."] = "Impostazioni ripristinate ai valori predefiniti.",
            ["Active"] = "Attivo",
            ["Paused"] = "In pausa",
            ["Proxy stopped"] = "Proxy arrestato",
            ["Ready"] = "Pronto"
        };

    private static readonly IReadOnlyDictionary<string, string> PortugueseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Inglês",
            ["French"] = "Francês",
            ["Spanish"] = "Espanhol",
            ["German"] = "Alemão",
            ["Italian"] = "Italiano",
            ["Portuguese"] = "Português",
            ["Japanese"] = "Japonês",
            ["Chinese"] = "Chinês",
            ["Arabic"] = "Árabe",
            ["Russian"] = "Russo",
            ["Overview"] = "Visão geral",
            ["Interception"] = "Interceptação",
            ["Simulation"] = "Simulação",
            ["Activity"] = "Atividade",
            ["Settings"] = "Configurações",
            ["Platform"] = "Plataforma",
            ["Monitoring"] = "Monitoramento",
            ["Control"] = "Controle",
            ["System"] = "Sistema",
            ["Support"] = "Suporte",
            ["Guide"] = "Guia",
            ["LOCAL RATIO CONTROL"] = "CONTROLE DE RATIO LOCAL",
            ["LOCAL / MONITORING"] = "LOCAL / MONITORAMENTO",
            ["Loading configuration…"] = "Carregando configuração…",
            ["Start"] = "Iniciar",
            ["Stop"] = "Parar",
            ["Retry"] = "Tentar novamente",
            ["Pause"] = "Pausar",
            ["Resume"] = "Retomar",
            ["Save changes"] = "Salvar alterações",
            ["Reset to defaults"] = "Redefinir padrões",
            ["Reset settings"] = "Redefinir configurações",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "Redefinir todas as configurações ajustáveis para os valores padrão? As estatísticas dos torrents monitorados, o progresso da integração e as sessões de simulação serão preservados.",
            ["To tray"] = "Minimizar para a bandeja",
            ["Close"] = "Fechar",
            ["Open Settings"] = "Abrir configurações",
            ["Current runtime status."] = "Estado atual do serviço.",
            ["PROXY CHANNEL"] = "CANAL PROXY",
            ["Local tracker interception · HTTP / HTTPS"] = "Interceptação local de trackers · HTTP / HTTPS",
            ["Tracked torrents"] = "Torrents monitorados",
            ["Announcements observed"] = "Anúncios observados",
            ["Simulations"] = "Simulações",
            ["Active / configured"] = "Ativas / configuradas",
            ["Reported upload"] = "Upload anunciado",
            ["Current session"] = "Sessão atual",
            ["OPERATING MODES"] = "MODOS DE OPERAÇÃO",
            ["Two paths, one local control plane."] = "Dois caminhos, um único plano de controle local.",
            ["Tracker announces only — payloads and peer traffic remain untouched."] = "Somente anúncios de trackers — payloads e tráfego entre pares permanecem intactos.",
            ["Appearance"] = "Aparência",
            ["Updates"] = "Atualizações",
            ["Check the official GitHub release without changing files automatically."] = "Verifique a versão oficial do GitHub sem alterar arquivos automaticamente.",
            ["Current version"] = "Versão atual",
            ["Check for updates"] = "Verificar atualizações",
            ["Not checked yet"] = "Ainda não verificado",
            ["Checking for updates…"] = "Verificando atualizações…",
            ["You are up to date"] = "Você está usando a versão mais recente",
            ["Unable to check for updates"] = "Não foi possível verificar atualizações",
            ["Update available: {0}"] = "Atualização disponível: {0}",
            ["Theme"] = "Tema",
            ["Light"] = "Claro",
            ["Dim"] = "Suave",
            ["Soft Dark"] = "Escuro suave",
            ["Dark"] = "Escuro",
            ["Changes the visual theme without changing proxy behavior."] = "Altera o tema visual sem mudar o comportamento do proxy.",
            ["Changes the interface accent color without changing proxy behavior."] = "Altera a cor de destaque da interface sem mudar o comportamento do proxy.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Escolhe se o ícone da área de notificação usa estados coloridos ou monocromático.",
            ["Changes the language used by the XRatio interface."] = "Altera o idioma usado pela interface do XRatio.",
            ["Accent color"] = "Cor de destaque",
            ["Blue"] = "Azul",
            ["Teal"] = "Turquesa",
            ["Violet"] = "Violeta",
            ["Amber"] = "Âmbar",
            ["Rose"] = "Rosa",
            ["Green"] = "Verde",
            ["Language"] = "Idioma",
            ["Choose the language used by the XRatio interface."] = "Escolha o idioma da interface XRatio.",
            ["Connection"] = "Conexão",
            ["HTTP proxy port"] = "Porta do proxy HTTP",
            ["Minimum leechers"] = "Leechers mínimos",
            ["Accept tracker traffic only"] = "Aceitar apenas tráfego de trackers",
            ["Listen on localhost only (required)"] = "Escutar apenas no localhost (obrigatório)",
            ["Write redacted proxy debug log"] = "Escrever log de depuração anonimizado",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "A porta localhost usada pelo proxy HTTP do XRatio. Mantenha-a livre e use a mesma porta no qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Número mínimo de pares incompletos necessário antes que o ajuste de ratio adicione upload calculado.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Bloqueia tráfego que não é de trackers para que o XRatio se concentre nas solicitações announce dos trackers.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Mantém o proxy vinculado ao localhost. Este limite de segurança obrigatório não pode ser desativado.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Escreve diagnósticos anonimizados do proxy em %APPDATA%\\XRatio\\proxy_debug.log. Os arquivos de log são mantidos por até 7 dias e alternados a cada 1 MiB. Ative apenas durante a solução de problemas.",
            ["Configuration"] = "Configuração",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Limite inferior do upload creditado por cada download real durante o ajuste de announces.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Limite superior do upload creditado por cada download real durante o ajuste de announces.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Limite inferior do multiplicador de upload aplicado ao upload real.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Limite superior do multiplicador de upload aplicado ao upload real.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Boost máximo de upload adicional usado durante um announce ajustado, em KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Probabilidade, de 0 a 100, de aplicar o boost de upload adicional.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Sempre ativado: informa zero bytes baixados. Use Pausar ou Parar para suspender a reescrita.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "Não aumenta o seu ratio. Quando ativada, anuncia os torrents concluídos com left=0 para que o tracker os veja como seeds; os downloads ativos mantêm os bytes restantes.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Restaura as configurações ajustáveis para os valores padrão. Torrents monitorados, estatísticas, progresso da integração e sessões de simulação são preservados.",
            ["Starts XRatio automatically with your Windows session."] = "Inicia o XRatio automaticamente com a sessão do Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Mantém um ícone do XRatio na área de notificação do Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Inicia o XRatio oculto na área de notificação em vez de abrir a janela principal.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Confirma que o XRatio pode adicionar sua CA local ao repositório de confiança do usuário atual do Windows para interceptação HTTPS.",
            ["Configuration saved."] = "Configuração salva.",
            ["Configuration reset to defaults."] = "Configuração restaurada para os valores padrão.",
            ["Active"] = "Ativo",
            ["Paused"] = "Pausado",
            ["Proxy stopped"] = "Proxy parado",
            ["Ready"] = "Pronto"
        };

    private static readonly IReadOnlyDictionary<string, string> JapaneseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "概要",
            ["Interception"] = "インターセプト",
            ["Simulation"] = "シミュレーション",
            ["Activity"] = "アクティビティ",
            ["Settings"] = "設定",
            ["Platform"] = "プラットフォーム",
            ["Monitoring"] = "監視",
            ["Control"] = "制御",
            ["System"] = "システム",
            ["Support"] = "サポート",
            ["Guide"] = "ガイド",
            ["Appearance"] = "外観",
            ["Updates"] = "更新",
            ["Check the official GitHub release without changing files automatically."] = "ファイルを自動変更せず、GitHub の公式リリースを確認します。",
            ["Current version"] = "現在のバージョン",
            ["Check for updates"] = "更新を確認",
            ["Not checked yet"] = "未確認",
            ["Checking for updates…"] = "更新を確認中…",
            ["You are up to date"] = "最新バージョンです",
            ["Unable to check for updates"] = "更新を確認できません",
            ["Update available: {0}"] = "更新があります: {0}",
            ["Theme"] = "テーマ",
            ["Light"] = "ライト",
            ["Dim"] = "控えめ",
            ["Soft Dark"] = "ソフトダーク",
            ["Dark"] = "ダーク",
            ["Changes the visual theme without changing proxy behavior."] = "プロキシの動作を変えずに表示テーマを変更します。",
            ["Changes the interface accent color without changing proxy behavior."] = "プロキシの動作を変えずにインターフェースのアクセント色を変更します。",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "通知領域のアイコンをカラー表示にするかモノクロにするかを選びます。",
            ["Changes the language used by the XRatio interface."] = "XRatio インターフェースの言語を変更します。",
            ["Accent color"] = "アクセントカラー",
            ["Blue"] = "ブルー",
            ["Teal"] = "ティール",
            ["Violet"] = "バイオレット",
            ["Amber"] = "アンバー",
            ["Rose"] = "ローズ",
            ["Green"] = "グリーン",
            ["Language"] = "言語",
            ["Connection"] = "接続",
            ["Configuration"] = "設定",
            ["Start"] = "開始",
            ["Stop"] = "停止",
            ["Pause"] = "一時停止",
            ["Resume"] = "再開",
            ["Save changes"] = "変更を保存",
            ["Reset to defaults"] = "デフォルトに戻す",
            ["Reset settings"] = "設定をリセット",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "すべての設定可能な項目をデフォルトに戻しますか？追跡中の torrent 統計、オンボーディングの進行状況、シミュレーション セッションは保持されます。",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "XRatio の HTTP プロキシが使用する localhost ポートです。空いているポートにし、qBittorrent でも同じポートを使います。",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "レシオ調整で計算上のアップロードを追加する前に必要な、未完了ピアの最小数です。",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "トラッカー以外の通信を遮断し、XRatio がトラッカーの announce 要求だけを処理するようにします。",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "プロキシを localhost に限定します。この必須のセキュリティ境界は無効にできません。",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "%APPDATA%\\XRatio\\proxy_debug.log に匿名化したプロキシ診断情報を書き込みます。ログは最長 7 日間保持し、1 MiB でローテーションします。トラブルシューティング時だけ有効にしてください。",
            ["Lower bound for upload credited per actual download during announce shaping."] = "announce のレシオ調整で、実際のダウンロードごとに加算するアップロードの下限です。",
            ["Upper bound for upload credited per actual download during announce shaping."] = "announce のレシオ調整で、実際のダウンロードごとに加算するアップロードの上限です。",
            ["Lower bound for the upload multiplier applied to actual upload."] = "実際のアップロードに適用するアップロード倍率の下限です。",
            ["Upper bound for the upload multiplier applied to actual upload."] = "実際のアップロードに適用するアップロード倍率の上限です。",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "レシオ調整した announce で使用する追加アップロードブーストの最大値（KiB/s）です。",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "追加アップロードブーストを適用する確率（0～100%）です。",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "常に有効：ダウンロード量を 0 バイトとして通知します。書き換えを一時停止するには一時停止または停止を使います。",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "比率は増えません。有効にすると、完了した torrent を left=0 で通知してトラッカーにシード中と認識させます。アクティブなダウンロードは残りのバイト数を保持します。",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "設定可能な項目をデフォルトに戻します。追跡中の torrent、統計、オンボーディングの進行状況、シミュレーション セッションは保持されます。",
            ["Starts XRatio automatically with your Windows session."] = "Windows セッション開始時に XRatio を自動起動します。",
            ["Keeps an XRatio icon in the Windows notification area."] = "Windows の通知領域に XRatio アイコンを表示します。",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "メインウィンドウを開かず、通知領域に隠れた状態で XRatio を起動します。",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "HTTPS インターセプトのため、現在の Windows ユーザーの信頼ストアにローカル CA を追加することを XRatio に許可します。",
            ["Configuration reset to defaults."] = "設定をデフォルトに戻しました。"
        };

    private static readonly IReadOnlyDictionary<string, string> ChineseMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "概览",
            ["Interception"] = "拦截",
            ["Simulation"] = "模拟",
            ["Activity"] = "活动",
            ["Settings"] = "设置",
            ["Platform"] = "平台",
            ["Monitoring"] = "监控",
            ["Control"] = "控制",
            ["System"] = "系统",
            ["Support"] = "支持",
            ["Guide"] = "指南",
            ["Appearance"] = "外观",
            ["Updates"] = "更新",
            ["Check the official GitHub release without changing files automatically."] = "检查 GitHub 官方版本，不会自动修改文件。",
            ["Current version"] = "当前版本",
            ["Check for updates"] = "检查更新",
            ["Not checked yet"] = "尚未检查",
            ["Checking for updates…"] = "正在检查更新…",
            ["You are up to date"] = "已是最新版本",
            ["Unable to check for updates"] = "无法检查更新",
            ["Update available: {0}"] = "有可用更新：{0}",
            ["Theme"] = "主题",
            ["Light"] = "浅色",
            ["Dim"] = "柔和",
            ["Soft Dark"] = "柔和深色",
            ["Dark"] = "深色",
            ["Changes the visual theme without changing proxy behavior."] = "更改界面主题，不改变代理行为。",
            ["Changes the interface accent color without changing proxy behavior."] = "更改界面强调色，不改变代理行为。",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "选择通知区域图标使用彩色状态还是单色。",
            ["Changes the language used by the XRatio interface."] = "更改 XRatio 界面使用的语言。",
            ["Accent color"] = "强调色",
            ["Blue"] = "蓝色",
            ["Teal"] = "青绿色",
            ["Violet"] = "紫色",
            ["Amber"] = "琥珀色",
            ["Rose"] = "玫瑰色",
            ["Green"] = "绿色",
            ["Language"] = "语言",
            ["Connection"] = "连接",
            ["Configuration"] = "配置",
            ["Start"] = "启动",
            ["Stop"] = "停止",
            ["Pause"] = "暂停",
            ["Resume"] = "继续",
            ["Save changes"] = "保存更改",
            ["Reset to defaults"] = "恢复默认设置",
            ["Reset settings"] = "重置设置",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "要将所有可配置设置恢复为默认值吗？已跟踪种子的统计信息、入门引导进度和模拟会话将予以保留。",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "XRatio HTTP 代理使用的 localhost 端口。请保持端口空闲，并在 qBittorrent 中使用相同端口。",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "在比率调整添加计算上传量之前所需的最少未完成节点数。",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "阻止非 Tracker 流量，使 XRatio 只处理 Tracker announce 请求。",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "将代理限制绑定到 localhost。此必需的安全边界无法禁用。",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "将匿名化的代理诊断写入 %APPDATA%\\XRatio\\proxy_debug.log。日志最多保留 7 天，并在达到 1 MiB 时轮换。仅在排查问题时启用。",
            ["Lower bound for upload credited per actual download during announce shaping."] = "比率调整期间，按实际下载量计入上传量的下限。",
            ["Upper bound for upload credited per actual download during announce shaping."] = "比率调整期间，按实际下载量计入上传量的上限。",
            ["Lower bound for the upload multiplier applied to actual upload."] = "应用于实际上传量的上传倍率下限。",
            ["Upper bound for the upload multiplier applied to actual upload."] = "应用于实际上传量的上传倍率上限。",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "比率调整 announce 中使用的额外上传加速上限（KiB/s）。",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "应用额外上传加速的概率（0 到 100%）。",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "始终启用：报告已下载字节数为 0。需要暂停重写时，请使用暂停或停止。",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "不会增加你的分享率。启用后，会使用 left=0 报告已完成的种子，让 tracker 将其视为做种；正在下载的任务会保留剩余字节数。",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "将可配置设置恢复为默认值。已跟踪种子、统计信息、入门引导进度和模拟会话会保留。",
            ["Starts XRatio automatically with your Windows session."] = "随 Windows 会话自动启动 XRatio。",
            ["Keeps an XRatio icon in the Windows notification area."] = "在 Windows 通知区域保留 XRatio 图标。",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "启动 XRatio 时隐藏在通知区域，而不是打开主窗口。",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "确认 XRatio 可以将本地 CA 添加到当前 Windows 用户的信任存储区，以进行 HTTPS 拦截。",
            ["Configuration reset to defaults."] = "设置已恢复为默认值。"
        };

    private static readonly IReadOnlyDictionary<string, string> ArabicMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "نظرة عامة",
            ["Interception"] = "الاعتراض",
            ["Simulation"] = "المحاكاة",
            ["Activity"] = "النشاط",
            ["Settings"] = "الإعدادات",
            ["Platform"] = "المنصة",
            ["Monitoring"] = "المراقبة",
            ["Control"] = "التحكم",
            ["System"] = "النظام",
            ["Support"] = "الدعم",
            ["Guide"] = "الدليل",
            ["Appearance"] = "المظهر",
            ["Updates"] = "التحديثات",
            ["Check the official GitHub release without changing files automatically."] = "تحقق من إصدار GitHub الرسمي دون تغيير الملفات تلقائياً.",
            ["Current version"] = "الإصدار الحالي",
            ["Check for updates"] = "البحث عن تحديثات",
            ["Not checked yet"] = "لم يتم التحقق بعد",
            ["Checking for updates…"] = "جارٍ البحث عن تحديثات…",
            ["You are up to date"] = "لديك أحدث إصدار",
            ["Unable to check for updates"] = "تعذر البحث عن تحديثات",
            ["Update available: {0}"] = "يتوفر تحديث: {0}",
            ["Theme"] = "السمة",
            ["Light"] = "فاتح",
            ["Dim"] = "خافت",
            ["Soft Dark"] = "داكن ناعم",
            ["Dark"] = "داكن",
            ["Changes the visual theme without changing proxy behavior."] = "يغيّر المظهر المرئي دون تغيير سلوك الوكيل.",
            ["Changes the interface accent color without changing proxy behavior."] = "يغيّر لون إبراز الواجهة دون تغيير سلوك الوكيل.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "يختار ما إذا كان رمز منطقة الإعلام يستخدم حالات ملونة أو أحادية اللون.",
            ["Changes the language used by the XRatio interface."] = "يغيّر اللغة المستخدمة في واجهة XRatio.",
            ["Accent color"] = "لون التمييز",
            ["Blue"] = "أزرق",
            ["Teal"] = "تركوازي",
            ["Violet"] = "بنفسجي",
            ["Amber"] = "كهرماني",
            ["Rose"] = "وردي",
            ["Green"] = "أخضر",
            ["Language"] = "اللغة",
            ["Connection"] = "الاتصال",
            ["Configuration"] = "الإعدادات",
            ["Start"] = "بدء",
            ["Stop"] = "إيقاف",
            ["Pause"] = "إيقاف مؤقت",
            ["Resume"] = "استئناف",
            ["Save changes"] = "حفظ التغييرات",
            ["Reset to defaults"] = "إعادة التعيين إلى الإعدادات الافتراضية",
            ["Reset settings"] = "إعادة تعيين الإعدادات",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "هل تريد إعادة تعيين جميع الإعدادات القابلة للتهيئة إلى قيمها الافتراضية؟ ستبقى إحصاءات التورنت المتتبعة وتقدم الإعداد الأولي وجلسات المحاكاة محفوظة.",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "منفذ localhost الذي يستخدمه وكيل HTTP في XRatio. اتركه متاحًا واستخدم المنفذ نفسه في qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "الحد الأدنى من النظراء غير المكتملين المطلوب قبل أن يضيف تشكيل النسبة رفعًا محسوبًا.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "يحظر حركة المرور غير الخاصة بالمتعقبات لكي يركّز XRatio على طلبات announce الخاصة بالمتعقبات.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "يبقي الوكيل مرتبطًا بـ localhost. لا يمكن تعطيل حد الأمان الإلزامي هذا.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "يكتب تشخيصات الوكيل المنقّحة إلى %APPDATA%\\XRatio\\proxy_debug.log. يتم الاحتفاظ بملفات السجل لمدة تصل إلى 7 أيام وتدويرها عند 1 MiB. فعّله فقط أثناء استكشاف الأخطاء.",
            ["Lower bound for upload credited per actual download during announce shaping."] = "الحد الأدنى للرفع المحتسب لكل تنزيل فعلي أثناء تشكيل announce.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "الحد الأقصى للرفع المحتسب لكل تنزيل فعلي أثناء تشكيل announce.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "الحد الأدنى لمضاعف الرفع المطبق على الرفع الفعلي.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "الحد الأقصى لمضاعف الرفع المطبق على الرفع الفعلي.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "الحد الأقصى لزيادة الرفع الإضافية المستخدمة أثناء announce مُشكّل، بوحدة KiB/s.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "احتمال تطبيق زيادة الرفع الإضافية، من 0 إلى 100٪.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "مفعّل دائمًا: يبلّغ عن صفر بايت من البيانات المنزّلة. استخدم الإيقاف المؤقت أو الإيقاف لتعليق إعادة الكتابة.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "لا يزيد النسبة لديك. عند تفعيله، يتم الإبلاغ عن التورنت المكتمل باستخدام left=0 لكي يراه المتعقب كتورنت يقوم بالبذر؛ وتحافظ التنزيلات النشطة على وحدات البايت المتبقية.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "يعيد الإعدادات القابلة للتهيئة إلى قيمها الافتراضية. تبقى التورنت المتتبعة والإحصاءات وتقدم الإعداد الأولي وجلسات المحاكاة محفوظة.",
            ["Starts XRatio automatically with your Windows session."] = "يشغّل XRatio تلقائيًا مع جلسة Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "يبقي رمز XRatio في منطقة إعلام Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "يشغّل XRatio مخفيًا في منطقة الإعلام بدلًا من فتح النافذة الرئيسية.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "يؤكد أن XRatio يمكنه إضافة CA المحلية إلى مخزن الثقة لمستخدم Windows الحالي لاعتراض HTTPS.",
            ["Configuration reset to defaults."] = "تمت إعادة الإعدادات إلى القيم الافتراضية."
        };

    private static readonly IReadOnlyDictionary<string, string> RussianMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Overview"] = "Обзор",
            ["Interception"] = "Перехват",
            ["Simulation"] = "Симуляция",
            ["Activity"] = "Активность",
            ["Settings"] = "Настройки",
            ["Platform"] = "Платформа",
            ["Monitoring"] = "Мониторинг",
            ["Control"] = "Управление",
            ["System"] = "Система",
            ["Support"] = "Поддержка",
            ["Guide"] = "Справка",
            ["Appearance"] = "Внешний вид",
            ["Updates"] = "Обновления",
            ["Check the official GitHub release without changing files automatically."] = "Проверяйте официальный выпуск GitHub без автоматического изменения файлов.",
            ["Current version"] = "Текущая версия",
            ["Check for updates"] = "Проверить обновления",
            ["Not checked yet"] = "Ещё не проверено",
            ["Checking for updates…"] = "Проверка обновлений…",
            ["You are up to date"] = "Установлена последняя версия",
            ["Unable to check for updates"] = "Не удалось проверить обновления",
            ["Update available: {0}"] = "Доступно обновление: {0}",
            ["Theme"] = "Тема",
            ["Light"] = "Светлая",
            ["Dim"] = "Приглушённая",
            ["Soft Dark"] = "Мягкая тёмная",
            ["Dark"] = "Тёмная",
            ["Changes the visual theme without changing proxy behavior."] = "Меняет визуальную тему, не изменяя поведение прокси.",
            ["Changes the interface accent color without changing proxy behavior."] = "Меняет цвет акцента интерфейса, не изменяя поведение прокси.",
            ["Chooses whether the notification-area icon uses color states or monochrome."] = "Выбирает цветной или монохромный режим значка в области уведомлений.",
            ["Changes the language used by the XRatio interface."] = "Меняет язык интерфейса XRatio.",
            ["Accent color"] = "Цвет акцента",
            ["Blue"] = "Синий",
            ["Teal"] = "Бирюзовый",
            ["Violet"] = "Фиолетовый",
            ["Amber"] = "Янтарный",
            ["Rose"] = "Розовый",
            ["Green"] = "Зелёный",
            ["Language"] = "Язык",
            ["Connection"] = "Подключение",
            ["Configuration"] = "Конфигурация",
            ["Start"] = "Запустить",
            ["Stop"] = "Остановить",
            ["Pause"] = "Пауза",
            ["Resume"] = "Продолжить",
            ["Save changes"] = "Сохранить изменения",
            ["Reset to defaults"] = "Сбросить по умолчанию",
            ["Reset settings"] = "Сбросить настройки",
            ["Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved."] = "Сбросить все настраиваемые параметры до значений по умолчанию? Статистика отслеживаемых торрентов, прогресс ознакомления и сеансы симуляции будут сохранены.",
            ["The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent."] = "Порт localhost, который использует HTTP-прокси XRatio. Оставьте его свободным и укажите тот же порт в qBittorrent.",
            ["Minimum incomplete peers required before ratio shaping adds calculated upload."] = "Минимальное число незавершённых пиров, необходимое для добавления расчётной отдачи при настройке соотношения.",
            ["Blocks non-tracker traffic so XRatio stays focused on tracker announce requests."] = "Блокирует трафик, не относящийся к трекерам, чтобы XRatio обрабатывал только announce-запросы трекеров.",
            ["Keeps the proxy bound to localhost. This required security boundary cannot be disabled."] = "Ограничивает прокси localhost. Эту обязательную границу безопасности нельзя отключить.",
            ["Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting."] = "Записывает обезличенные диагностические данные прокси в %APPDATA%\\XRatio\\proxy_debug.log. Файлы журнала хранятся до 7 дней и ротируются при достижении 1 МиБ. Включайте только для поиска неисправностей.",
            ["Lower bound for upload credited per actual download during announce shaping."] = "Нижняя граница отдачи, начисляемой за каждую фактическую загрузку во время настройки announce.",
            ["Upper bound for upload credited per actual download during announce shaping."] = "Верхняя граница отдачи, начисляемой за каждую фактическую загрузку во время настройки announce.",
            ["Lower bound for the upload multiplier applied to actual upload."] = "Нижняя граница множителя отдачи, применяемого к фактической отдаче.",
            ["Upper bound for the upload multiplier applied to actual upload."] = "Верхняя граница множителя отдачи, применяемого к фактической отдаче.",
            ["Maximum extra upload boost used during a shaped announce, in KiB/s."] = "Максимальная дополнительная отдача во время настроенного announce, в КиБ/с.",
            ["Percentage chance, from 0 to 100, that the extra upload boost is applied."] = "Вероятность применения дополнительной отдачи от 0 до 100%.",
            ["Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting."] = "Всегда включено: сообщает о нулевом объёме загруженных данных. Используйте паузу или остановку, чтобы приостановить переписывание.",
            ["Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes."] = "Не увеличивает ваш рейтинг. При включении завершённые торренты сообщаются с left=0, чтобы трекер видел их как сиды; активные загрузки сохраняют оставшиеся байты.",
            ["Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."] = "Возвращает настраиваемые параметры к значениям по умолчанию. Отслеживаемые торренты, статистика, прогресс ознакомления и сеансы симуляции сохраняются.",
            ["Starts XRatio automatically with your Windows session."] = "Автоматически запускает XRatio вместе с сеансом Windows.",
            ["Keeps an XRatio icon in the Windows notification area."] = "Оставляет значок XRatio в области уведомлений Windows.",
            ["Starts XRatio hidden in the notification area instead of opening the main window."] = "Запускает XRatio скрытым в области уведомлений вместо открытия главного окна.",
            ["Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception."] = "Подтверждает, что XRatio может добавить локальный центр сертификации в хранилище доверия текущего пользователя Windows для перехвата HTTPS.",
            ["Configuration reset to defaults."] = "Настройки сброшены к значениям по умолчанию."
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

    // These prefix maps are read on every activity update. Keep them as
    // process-lifetime arrays so translating a burst of proxy events does not
    // allocate a new tuple array for every message.
    private static readonly (string From, string To)[] EnglishToFrenchMessagePrefixes =
    [
        ("HTTP/HTTPS active on", "HTTP/HTTPS actif sur"),
        ("HTTP active on", "HTTP actif sur"),
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
        ("Restored ", "Restauré "),
        ("Onboarding could not be closed: ", "Impossible de fermer l’onboarding : "),
        ("Onboarding could not be restored: ", "Impossible de restaurer l’onboarding : "),
        ("Onboarding progress could not be saved: ", "Impossible d’enregistrer la progression de l’onboarding : "),
        ("Proxy listening on ", "Proxy à l’écoute sur "),
        ("Unexpected connection failure: ", "Échec inattendu de la connexion : "),
        ("Connection failed: ", "Échec de la connexion : "),
        ("Tracker connection failed for ", "Échec de la connexion au tracker pour "),
        ("Tracker response failed for ", "Échec de la réponse du tracker pour ")
    ];

    private static readonly (string From, string To)[] FrenchToEnglishMessagePrefixes =
    [
        ("HTTP/HTTPS actif sur", "HTTP/HTTPS active on"),
        ("HTTP actif sur", "HTTP active on"),
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
        ("Restauré ", "Restored "),
        ("Impossible de fermer l’onboarding : ", "Onboarding could not be closed: "),
        ("Impossible de restaurer l’onboarding : ", "Onboarding could not be restored: "),
        ("Impossible d’enregistrer la progression de l’onboarding : ", "Onboarding progress could not be saved: "),
        ("Proxy à l’écoute sur ", "Proxy listening on "),
        ("Échec inattendu de la connexion : ", "Unexpected connection failure: "),
        ("Échec de la connexion : ", "Connection failed: "),
        ("Tracker connection failed for ", "Tracker connection failed for "),
        ("Tracker response failed for ", "Tracker response failed for ")
    ];

    private static readonly (string From, string Key)[] EnglishDynamicMessagePrefixes =
    [
        ("HTTP/HTTPS active on ", "HTTP/HTTPS active on"),
        ("HTTP active on ", "HTTP active on"),
        ("Paused on ", "Paused on"),
        ("Interception is stopped. ", "Interception is stopped."),
        ("Simulation added: ", "Simulation added:"),
        ("Simulation start failed: ", "Simulation start failed:"),
        ("Simulation update failed: ", "Simulation update failed:"),
        ("Could not add simulation: ", "Could not add simulation:"),
        ("Could not restore simulation form: ", "Could not restore simulation form:"),
        ("Removed simulation: ", "Removed simulation:"),
        ("Loaded torrent: ", "Loaded torrent:"),
        ("Torrent import failed: ", "Torrent import failed:"),
        ("Could not enable HTTPS: ", "Could not enable HTTPS:"),
        ("Could not remove CA trust: ", "Could not remove CA trust:"),
        ("Configuration error: ", "Configuration error:"),
        ("Proxy cleanup error: ", "Proxy cleanup error:"),
        ("Simulation settings persistence error: ", "Simulation settings persistence error:"),
        ("State persistence error: ", "State persistence error:"),
        ("Skipped saved simulation: ", "Skipped saved simulation:"),
        ("Copied info hash to clipboard: ", "Copied info hash to clipboard:"),
        ("Reset stats for torrent hash: ", "Reset stats for torrent hash:"),
        ("Reset all tracked statistics for ", "Reset all tracked statistics for"),
        ("Startup error: ", "Startup error:"),
        ("Removed ", "Removed"),
        ("Restored ", "Restored"),
        ("Onboarding close error: ", "Onboarding close error:"),
        ("Onboarding restore error: ", "Onboarding restore error:"),
        ("Onboarding progress error: ", "Onboarding progress error:"),
        ("Could not open qBittorrent: ", "Could not open qBittorrent:"),
        ("Ratio shaping warning could not be shown: ", "Ratio shaping warning could not be shown:"),
        ("Onboarding could not be closed: ", "Onboarding could not be closed:"),
        ("Onboarding could not be restored: ", "Onboarding could not be restored:"),
        ("Onboarding progress could not be saved: ", "Onboarding progress could not be saved:"),
        ("Proxy listening on ", "Proxy listening on"),
        ("Unexpected connection failure: ", "Unexpected connection failure:"),
        ("Connection failed: ", "Connection failed:"),
        ("Tracker connection failed for ", "Tracker connection failed for"),
        ("Tracker response failed for ", "Tracker response failed for"),
        ("Start failed: ", "Start failed:"),
        ("Stop announce failed: ", "Stop announce failed:"),
        ("Simulation failed: ", "Simulation failed:")
    ];

    private static readonly (string From, string To)[] SimulationStatusToFrench =
    [
        ("●  Running", "●  En cours"),
        ("▶  Starting", "▶  Démarrage…"),
        ("■  Stopping", "■  Arrêt en cours"),
        ("!  Error", "!  Erreur"),
        ("■  Stopped", "■  Arrêté")
    ];

    private static readonly (string From, string To)[] SimulationStatusToEnglish =
    [
        ("●  En cours", "●  Running"),
        ("▶  Démarrage…", "▶  Starting"),
        ("■  Arrêt en cours", "■  Stopping"),
        ("!  Erreur", "!  Error"),
        ("■  Arrêté", "■  Stopped")
    ];

    private static IReadOnlyDictionary<string, string> BuildEnglishMap()
    {
        var reverse = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var map in TranslationMaps.Values.Concat(UiTextGenerated.Maps.Values))
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
        var normalized = Normalize(language);
        var map = TranslationMaps.TryGetValue(normalized, out var selected)
            ? selected
            : null;
        if (map is not null && map.TryGetValue(key, out var translation))
            return NormalizePlaceholderSpacing(key, translation);
        if (UiTextGenerated.Maps.TryGetValue(normalized, out var generated) &&
            generated.TryGetValue(key, out var generatedTranslation))
            return NormalizePlaceholderSpacing(key, generatedTranslation);
        return key;
    }

    private static string NormalizePlaceholderSpacing(string key, string translation)
    {
        if (translation.IndexOf('{', StringComparison.Ordinal) < 0)
            return translation;

        foreach (Match match in Regex.Matches(key, @"\{\d+\}", RegexOptions.CultureInvariant))
        {
            var token = match.Value;
            var translatedIndex = translation.IndexOf(token, StringComparison.Ordinal);
            if (translatedIndex < 0)
                continue;

            var needsLeadingSpace = match.Index > 0 && char.IsWhiteSpace(key[match.Index - 1]);
            var needsTrailingSpace = match.Index + match.Length < key.Length &&
                                     char.IsWhiteSpace(key[match.Index + match.Length]);
            if (needsLeadingSpace && translatedIndex > 0 &&
                !char.IsWhiteSpace(translation[translatedIndex - 1]))
            {
                translation = translation.Insert(translatedIndex, " ");
                translatedIndex++;
            }

            var trailingIndex = translatedIndex + token.Length;
            if (needsTrailingSpace && trailingIndex < translation.Length &&
                !char.IsWhiteSpace(translation[trailingIndex]))
                translation = translation.Insert(trailingIndex, " ");
        }

        return translation;
    }

    public static bool HasTranslation(string key, string language)
    {
        var normalized = Normalize(language);
        if (normalized == English)
            return true;
        return (TranslationMaps.TryGetValue(normalized, out var selected) && selected.ContainsKey(key)) ||
               (UiTextGenerated.Maps.TryGetValue(normalized, out var generated) && generated.ContainsKey(key));
    }

    // Keep the hover action name stable across locales. The surrounding
    // tooltip and confirmation remain localized, while the compact rail
    // action always reads "Update" as requested.
    public static string UpdateIndicatorLabel(string language) => "Update";

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
        if (normalized != English && message.StartsWith(englishGuidePrefix, StringComparison.Ordinal))
        {
            var tab = message[englishGuidePrefix.Length..];
            var guideLabel = Translate("Guide", normalized);
            return $"{guideLabel} XRatio · {TranslateAny(tab, normalized)}";
        }
        if (normalized != English && message.StartsWith(frenchGuidePrefix, StringComparison.Ordinal))
        {
            var tab = message[frenchGuidePrefix.Length..];
            var guideLabel = Translate("Guide", normalized);
            return $"{guideLabel} XRatio · {TranslateAny(tab, normalized)}";
        }
        if (normalized == English && message.StartsWith(frenchGuidePrefix, StringComparison.Ordinal))
        {
            var tab = message[frenchGuidePrefix.Length..];
            return $"XRatio Guide · {TranslateAny(tab, normalized)}";
        }

        if (normalized != English)
        {
            var localizedDynamic = TranslateDynamicMessage(message, normalized);
            if (!string.Equals(localizedDynamic, message, StringComparison.Ordinal))
                return localizedDynamic;
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

            return ReplacePrefix(message, EnglishToFrenchMessagePrefixes, out var translated)
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

        return ReplacePrefix(message, FrenchToEnglishMessagePrefixes, out var english)
            ? english
            : TranslateProgressMessage(message, toFrench: false);
    }

    private static string TranslateDynamicMessage(string message, string language)
    {
        const string simulationAddedPrefix = "Simulation added: ";
        const string simulationAddedSuffix = ". Press Start to contact the tracker.";
        if (message.StartsWith(simulationAddedPrefix, StringComparison.Ordinal) &&
            message.EndsWith(simulationAddedSuffix, StringComparison.Ordinal))
        {
            var name = message[simulationAddedPrefix.Length..^simulationAddedSuffix.Length];
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Simulation added: {0}. Press Start to contact the tracker.", language),
                name);
        }

        if (message.StartsWith("Loaded torrent: ", StringComparison.Ordinal) &&
            message.EndsWith(" tracker(s).", StringComparison.Ordinal))
        {
            var body = message["Loaded torrent: ".Length..^" tracker(s).".Length];
            var parts = body.Split(" · ", 3, StringSplitOptions.None);
            if (parts.Length == 3)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    Translate("Loaded torrent: {0} · {1} · {2} tracker(s).", language),
                    parts[0],
                    parts[1],
                    parts[2]);
            }
        }

        if (message.StartsWith("Removed ", StringComparison.Ordinal) &&
            message.EndsWith(" duplicate saved simulation(s).", StringComparison.Ordinal) &&
            int.TryParse(
                message["Removed ".Length..^" duplicate saved simulation(s).".Length],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var removedCount))
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Removed {0} duplicate saved simulation(s).", language),
                removedCount);
        }

        if (message.StartsWith("Restored ", StringComparison.Ordinal) &&
            message.EndsWith(" stopped simulation session(s).", StringComparison.Ordinal) &&
            int.TryParse(
                message["Restored ".Length..^" stopped simulation session(s).".Length],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var restoredCount))
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Restored {0} stopped simulation session(s).", language),
                restoredCount);
        }

        if (message.StartsWith("Update available: ", StringComparison.Ordinal))
        {
            var version = message["Update available: ".Length..];
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Update available: {0}", language),
                version);
        }

        foreach (var (from, key) in EnglishDynamicMessagePrefixes)
        {
            if (!message.StartsWith(from, StringComparison.Ordinal))
                continue;

            var translatedPrefix = Translate(key, language);
            var separator = from.EndsWith(' ') ? " " : string.Empty;
            var remainder = message[from.Length..];
            var translatedRemainder = TranslateDynamicMessage(remainder, language);
            if (string.Equals(translatedRemainder, remainder, StringComparison.Ordinal))
            {
                var directRemainder = TranslateAny(remainder, language);
                translatedRemainder = string.Equals(directRemainder, remainder, StringComparison.Ordinal)
                    ? remainder
                    : directRemainder;
            }
            return translatedPrefix + separator + translatedRemainder;
        }

        const string removePrefix = "Remove the stopped simulation “";
        const string removeSuffix = "”? This does not delete the .torrent file.";
        if (message.StartsWith(removePrefix, StringComparison.Ordinal) &&
            message.EndsWith(removeSuffix, StringComparison.Ordinal))
        {
            var name = message[removePrefix.Length..^removeSuffix.Length];
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Remove the stopped simulation “{0}”? This does not delete the .torrent file.", language),
                name);
        }

        const string portSuffix = " is already in use. Interception is stopped until you choose a free port or close the other listener.";
        if (message.StartsWith("Port ", StringComparison.Ordinal) &&
            message.EndsWith(portSuffix, StringComparison.Ordinal))
        {
            var port = message["Port ".Length..^portSuffix.Length];
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate("Port {0} is already in use. Interception is stopped until you choose a free port or close the other listener.", language),
                port);
        }

        foreach (var (suffix, template) in new[]
                 {
                     (" must be an integer.", "{0} must be an integer."),
                     (" must be a finite number using '.' as decimal separator.", "{0} must be a finite number using '.' as decimal separator."),
                     (" must be greater than zero.", "{0} must be greater than zero."),
                     (" must be an absolute URI.", "{0} must be an absolute URI.")
                 })
        {
            if (!message.EndsWith(suffix, StringComparison.Ordinal) || message.Length <= suffix.Length)
                continue;

            var name = message[..^suffix.Length];
            var translatedName = TranslateAny(name, language);
            return string.Format(
                CultureInfo.InvariantCulture,
                Translate(template, language),
                translatedName);
        }

        var translatedTimerMessage = message switch
        {
            "Timer duration must be greater than zero." => Translate(message, language),
            "Timer duration is too small." => Translate(message, language),
            _ => message
        };
        if (!string.Equals(translatedTimerMessage, message, StringComparison.Ordinal))
            return translatedTimerMessage;

        if (message.StartsWith("Downloaded ", StringComparison.Ordinal) &&
            message.EndsWith(".", StringComparison.Ordinal))
        {
            var body = message["Downloaded ".Length..^1];
            var atIndex = body.IndexOf(" at ", StringComparison.Ordinal);
            if (atIndex > 0)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    Translate("Downloaded {0} at {1}/s.", language),
                    body[..atIndex],
                    body[(atIndex + 4)..]);
            }
        }

        if (message.StartsWith("Uploaded ", StringComparison.Ordinal) &&
            message.EndsWith(".", StringComparison.Ordinal))
        {
            var body = message["Uploaded ".Length..^1];
            var atIndex = body.IndexOf(" at ", StringComparison.Ordinal);
            if (atIndex > 0)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    Translate("Uploaded {0} at {1}/s.", language),
                    body[..atIndex],
                    body[(atIndex + 4)..]);
            }
        }

        if (message.StartsWith("Peers: ", StringComparison.Ordinal) &&
            message.EndsWith(".", StringComparison.Ordinal))
        {
            var body = message["Peers: ".Length..^1];
            const string seedersSeparator = " seeders, ";
            const string leechersSeparator = " leechers. Next announce: ";
            var seedersIndex = body.IndexOf(seedersSeparator, StringComparison.Ordinal);
            var leechersIndex = body.IndexOf(leechersSeparator, StringComparison.Ordinal);
            if (seedersIndex > 0 && leechersIndex > seedersIndex + seedersSeparator.Length)
            {
                var seeders = body[..seedersIndex];
                var leechers = body[(seedersIndex + seedersSeparator.Length)..leechersIndex];
                var announceIndex = leechersIndex + leechersSeparator.Length;
                return string.Format(
                    CultureInfo.InvariantCulture,
                    Translate("Peers: {0} seeders, {1} leechers. Next announce: {2}.", language),
                    seeders,
                    leechers,
                    body[announceIndex..]);
            }
        }

        // Simulation log entries are prefixed with the torrent name. Translate
        // the known status/error suffix while preserving that user-provided name.
        var separatorIndex = message.IndexOf(": ", StringComparison.Ordinal);
        if (separatorIndex > 0 && separatorIndex + 2 < message.Length)
        {
            var prefix = message[..(separatorIndex + 2)];
            var prefixLabel = message[..separatorIndex];
            var suffix = message[(separatorIndex + 2)..];
            var translatedSuffix = TranslateDynamicMessage(suffix, language);
            if (string.Equals(translatedSuffix, suffix, StringComparison.Ordinal))
                translatedSuffix = TranslateAny(suffix, language);

            var translatedPrefix = TranslateAny(prefixLabel, language);
            var prefixChanged = !string.Equals(translatedPrefix, prefixLabel, StringComparison.Ordinal);
            var suffixChanged = !string.Equals(translatedSuffix, suffix, StringComparison.Ordinal);
            if (prefixChanged || suffixChanged)
                return (prefixChanged ? translatedPrefix : prefixLabel) + ": " + translatedSuffix;
        }

        return message;
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
        var statuses = toFrench ? SimulationStatusToFrench : SimulationStatusToEnglish;

        return ReplacePrefix(message, statuses, out var translated)
            ? translated
            : message;
    }
}
