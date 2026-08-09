namespace CS2ConfigSaver.Helpers
{
    /// <summary>
    /// Языковая локализация интерфейса.
    /// </summary>
    public class Locale
    {
        public string Title { get; set; } = "";
        public string SaveBtn { get; set; } = "";
        public string LocateBtn { get; set; } = "";
        public string FindSteamBtn { get; set; } = "";
        public string AddToGameBtn { get; set; } = "";
        public string ExitBtn { get; set; } = "";
        public string SteamNotFound { get; set; } = "";
        public string SteamPathLabel { get; set; } = "";
        public string BackupFolderLabel { get; set; } = "";
        public string HelpTitle { get; set; } = "";
        public string Step1 { get; set; } = "";
        public string Step2 { get; set; } = "";
        public string Step3_1 { get; set; } = "";
        public string Step3_Cmd { get; set; } = "";
        public string Step3_2 { get; set; } = "";
        public string Step4 { get; set; } = "";
        public string Step5 { get; set; } = "";
        public string Step6 { get; set; } = "";
        public string Step7 { get; set; } = "";
        public string Step8 { get; set; } = "";
        public string Step9 { get; set; } = "";
        public string Step10Part1 { get; set; } = "";
        public string Step10Cmd { get; set; } = "";
        public string Step10Part2 { get; set; } = "";
        public string Run { get; set; } = "";
        public string Copied { get; set; } = "";
        public string SelectFile { get; set; } = "";
        public string CopySuccess { get; set; } = "";
        public string SelectSteamId { get; set; } = "";
        public string Back { get; set; } = "";
        public string EnterConfigName { get; set; } = "";
        public string OnlyLetters { get; set; } = "";
        public string Save { get; set; } = "";
        public string GamePathNotFound { get; set; } = "";
        public string NotificationFormat { get; set; } = "";
        public string HelpBtn { get; set; } = "";

        /// <summary>
        /// Словари локализации
        /// </summary>
        public static readonly Locale[] Locales = new Locale[]
        {
            // 0: EN
            new Locale {
                Title = "Config Saver by DIWEST",
                SaveBtn = "Save current config",
                LocateBtn = "Locate saved configs",
                FindSteamBtn = "Find Steam Folders",
                AddToGameBtn = "Add config to CS2",
                ExitBtn = "Exit",
                SteamNotFound = "Steam not found. Click 'Find Steam Folders'",
                SteamPathLabel = "Steam path: ",
                BackupFolderLabel = "Backup folder:",
                HelpTitle = "How it works:",
                Step1 = "1. Launch CS2.",
                Step2 = "2. Type host_writeconfig in console.",
                Step3_1 = "3. Detect Steam and choose profile.",
                Step3_Cmd = "host_writeconfig YourConfigName",
                Step4 = "4. If multiple accounts found, choose active.",
                Step5 = "5. Select backup folder destination.",
                Step6 = "6. Input config name and save.",
                Step7 = "7. Add saved config back to game folder for exec.",
                Step8 = "8. Thank DIWEST!",
                Step10Part1 = "7. Add config and type ",
                Step10Cmd = "exec YourConfigName",
                Run = "▶ Run",
                Copied = "Copied!",
                SelectFile = "Select a config (.cfg) file to add to game",
                CopySuccess = "Config successfully added to CS2 directory!",
                SelectSteamId = "Select Steam ID:",
                Back = "Back",
                EnterConfigName = "Enter config name:",
                OnlyLetters = "Only RU/EN letters",
                Save = "Save",
                GamePathNotFound = "Counter-Strike 2 is not running.",
                NotificationFormat = "Command \"{0}\" copied to clipboard!",
                HelpBtn = "Config saver helper!"
            },
            // 1: RU
            new Locale {
                Title = "Сохранение конфига от DIWEST",
                SaveBtn = "Сохранить текущий конфиг",
                LocateBtn = "Открыть папку бэкапов",
                FindSteamBtn = "Найти папки Steam",
                AddToGameBtn = "Добавить конфиг в CS2",
                ExitBtn = "Выход",
                SteamNotFound = "Steam не найден. Нажмите 'Найти папки Steam'",
                SteamPathLabel = "Путь к Steam: ",
                BackupFolderLabel = "Папка бэкапов:",
                HelpTitle = "Как это работает:",
                Step1 = "1. Запустить CS2.",
                Step2 = "2. Прописать host_writeconfig в консоли.",
                Step3_1 = "3. Найти папки Steam и выбрать профиль.",
                Step3_Cmd = "host_writeconfig YourConfigName",
                Step4 = "4. Если аккаунтов несколько, выберите активный.",
                Step5 = "5. Указать папку для сохранения бэкапов.",
                Step6 = "6. Ввести имя конфига и сохранить.",
                Step7 = "7. Добавить сохраненный конфиг в игру для exec.",
                Step8 = "8. Поблагодарить DIWEST!",
                Step10Part1 = "7. Добавить конфиг и ввести ",
                Step10Cmd = "exec YourConfigName",
                Run = "▶ Пуск",
                Copied = "Скопировано!",
                SelectFile = "Выберите файл конфигурации (.cfg) для добавления в игру",
                CopySuccess = "Конфиг успешно добавлен в папку CS2!",
                SelectSteamId = "Выберите Steam ID:",
                Back = "Назад",
                EnterConfigName = "Введите имя конфига:",
                OnlyLetters = "Только буквы RU/EN",
                Save = "Сохранить",
                GamePathNotFound = "Counter-Strike 2 не запущена.",
                NotificationFormat = "Команда \"{0}\" скопирована в буфер!",
                HelpBtn = "Помощник сохранения конфига!"
            },
            // 2: DE
            new Locale {
                Title = "Config Saver von DIWEST",
                SaveBtn = "Aktuelle Config speichern",
                LocateBtn = "Backups-Ordner öffnen",
                FindSteamBtn = "Steam-Ordner finden",
                AddToGameBtn = "Config zu CS2 hinzufügen",
                ExitBtn = "Beenden",
                SteamNotFound = "Steam nicht gefunden. Klicken Sie auf 'Steam-Ordner finden'",
                SteamPathLabel = "Steam-Pfad: ",
                BackupFolderLabel = "Backup-Ordner:",
                HelpTitle = "Wie es funktioniert:",
                Step1 = "1. CS2 starten.",
                Step2 = "2. host_writeconfig in die Konsole eingeben.",
                Step3_1 = "3. Steam-Ordner erkennen und Ihr Profil auswählen.",
                Step3_Cmd = "host_writeconfig YourConfigName",
                Step4 = "4. Wenn mehrere Konten gefunden werden, wählen Sie das aktive aus.",
                Step5 = "5. Speicherort für Backups auswählen.",
                Step6 = "6. Config-Namen eingeben und speichern.",
                Step7 = "7. Config in den CS2-Ordner kopieren, um sie mit exec zu laden.",
                Step8 = "8. Danke DIWEST!",
                Step10Part1 = "7. Config hinzufügen und eingeben ",
                Step10Cmd = "exec YourConfigName",
                Run = "▶ Start",
                Copied = "Kopiert!",
                SelectFile = "Wählen Sie eine Config (.cfg) aus, die Sie dem Spiel hinzufügen möchten",
                CopySuccess = "Config erfolgreich zum CS2-Verzeichnis hinzugefügt!",
                SelectSteamId = "Steam-ID auswählen:",
                Back = "Zurück",
                EnterConfigName = "Config-Namen eingeben:",
                OnlyLetters = "Nur RU/EN Buchstaben",
                Save = "Speichern",
                GamePathNotFound = "Counter-Strike 2 läuft nicht.",
                NotificationFormat = "Befehl \"{0}\" in die Zwischenablage kopiert!", 
                HelpBtn = "Konfigurations saver helfer!"
            },
            // 3: FR
            new Locale {
                Title = "Config Saver par DIWEST",
                SaveBtn = "Sauvegarder config actuelle",
                LocateBtn = "Ouvrir dossier sauvegardes",
                FindSteamBtn = "Trouver dossiers Steam",
                AddToGameBtn = "Ajouter config à CS2",
                ExitBtn = "Quitter",
                SteamNotFound = "Steam introuvable. Cliquez sur 'Trouver dossiers Steam'",
                SteamPathLabel = "Chemin Steam : ",
                BackupFolderLabel = "Dossier de sauvegarde :",
                HelpTitle = "Comment ça marche :",
                Step1 = "1. Lancez CS2.",
                Step2 = "2. Tapez host_writeconfig dans la console.",
                Step3_1 = "3. Détectez Steam et choisissez votre profil.",
                Step3_Cmd = "host_writeconfig YourConfigName",
                Step4 = "4. Si plusieurs comptes sont trouvés, choisissez celui qui est actif.",
                Step5 = "5. Sélectionnez le dossier de sauvegarde.",
                Step6 = "6. Entrez le nom de la config et sauvegardez.",
                Step7 = "7. Copiez la config dans le dossier CS2 pour la charger via exec.",
                Step8 = "8. Merci DIWEST !",
                Step10Part1 = "7. Ajouter config et taper ",
                Step10Cmd = "exec YourConfigName",
                Run = "▶ Lancer",
                Copied = "Copié !",
                SelectFile = "Sélectionnez un fichier config (.cfg) à ajouter au jeu",
                CopySuccess = "Configuration ajoutée avec succès au répertoire CS2 !",
                SelectSteamId = "Sélectionnez l'identifiant Steam :",
                Back = "Retour",
                EnterConfigName = "Entrez le nom de la config :",
                OnlyLetters = "Lettres RU/EN uniquement",
                Save = "Sauvegarder",
                GamePathNotFound = "Counter-Strike 2 n'est pas lancé.",
                NotificationFormat = "Commande \"{0}\" copiée dans le presse-papiers !",
                HelpBtn = "Assistant de sauvegarde de configuration!"
            },
            // 4: ES
            new Locale {
                Title = "Config Saver por DIWEST",
                SaveBtn = "Guardar config actual",
                LocateBtn = "Abrir carpeta de respaldos",
                FindSteamBtn = "Buscar carpetas de Steam",
                AddToGameBtn = "Agregar config a CS2",
                ExitBtn = "Salir",
                SteamNotFound = "Steam no encontrado. Haga clic en 'Buscar carpetas de Steam'",
                SteamPathLabel = "Ruta de Steam: ",
                BackupFolderLabel = "Carpeta de respaldo:",
                HelpTitle = "Cómo funciona:",
                Step1 = "1. Iniciar CS2.",
                Step2 = "2. Escriba host_writeconfig en la consola.",
                Step3_1 = "3. Detecte Steam y elija su perfil.",
                Step3_Cmd = "host_writeconfig YourConfigName",
                Step4 = "4. Si se encuentran varias cuentas, elija la activa.",
                Step5 = "5. Seleccione la carpeta de destino del respaldo.",
                Step6 = "6. Ingrese el nombre de la config y guarde.",
                Step7 = "7. Copie la config en la carpeta de CS2 para cargarla vía exec.",
                Step8 = "8. ¡Gracias DIWEST!",
                Step10Part1 = "7. Agregar config y escribir ",
                Step10Cmd = "exec YourConfigName",
                Run = "▶ Ejecutar",
                Copied = "¡Copiado!",
                SelectFile = "Seleccione un archivo de config (.cfg) para agregar al juego",
                CopySuccess = "¡Configuración agregada con éxito al directorio de CS2!",
                SelectSteamId = "Seleccionar ID de Steam:",
                Back = "Atrás",
                EnterConfigName = "Ingrese el nombre de la config:",
                OnlyLetters = "Solo letras RU/EN",
                Save = "Guardar",
                GamePathNotFound = "Counter-Strike 2 no se está ejecutando.",
                NotificationFormat = "¡Comando \"{0}\" copiado al portapapeles!",
                HelpBtn = "Ayudante de guardado de configuración!"
            }
        };
    }
}