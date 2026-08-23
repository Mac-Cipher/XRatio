namespace XRatio.Desktop;

internal static class UiText
{
    public const string English = "English";
    public const string French = "French";

    private static readonly IReadOnlyDictionary<string, string> FrenchMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["English"] = "Anglais",
            ["French"] = "Français",
            ["Overview"] = "Vue d’ensemble",
            ["Interception"] = "Interception",
            ["Simulation"] = "Simulation",
            ["Activity"] = "Activité",
            ["Settings"] = "Réglages",
            ["Platform"] = "Plateforme",
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
            ["Theme"] = "Thème",
            ["Light"] = "Clair",
            ["Dim"] = "Sombre doux",
            ["Dark"] = "Sombre",
            ["Accent color"] = "Couleur d’accentuation",
            ["Blue"] = "Bleu",
            ["Teal"] = "Turquoise",
            ["Language"] = "Langue",
            ["Choose the visual mode and signal color for the XRatio control plane. Blue is the default; the hierarchy stays the same in all themes."] = "Choisissez le mode visuel et la couleur de signal du plan de contrôle XRatio. Le bleu est utilisé par défaut ; la hiérarchie reste identique dans les trois thèmes.",
            ["Choose the language used by the XRatio interface."] = "Choisissez la langue de l’interface XRatio.",
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
            ["Keep the scope clear"] = "Garder le périmètre clair",
            ["Keep Listen on localhost only enabled unless you have a specific, authorized reason to change the deployment boundary."] = "Laissez Écouter uniquement sur localhost activé, sauf raison précise et autorisée de modifier la limite d’exposition.",
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

    private static readonly IReadOnlyDictionary<string, string> EnglishMap =
        FrenchMap.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.Ordinal);

    public static string Normalize(string? language) =>
        string.Equals(language, French, StringComparison.OrdinalIgnoreCase)
            ? French
            : English;

    public static string Translate(string key, string language)
    {
        if (Normalize(language) == French && FrenchMap.TryGetValue(key, out var translation))
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
