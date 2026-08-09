using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using CS2ConfigSaver.Helpers;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace CS2ConfigSaver
{
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public partial class MainWindow : Window
    {
        #region ПОЛЯ И ИНИЦИАЛИЗАЦИЯ
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const int SW_RESTORE = 9;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private string? _detectedSteamPath;
        private string? _selectedSteamIdPath;
        private string _backupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ConfigSaverBackups");
        private readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CS2ConfigSaver.log");

        private HelpWindow? _helpWindow;

        // Флаг, предотвращающий закрытие окон при открытии диалоговых окон выбора файлов/папок
        public bool IsDialogOpen { get; set; } = false;

        private int _currentLanguageIndex = 0;
        private readonly string _langConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang.txt");

        public Locale CurrentLocale => Locale.Locales[_currentLanguageIndex];

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LogOperation("Application started.", NotificationType.Info);
                LoadSavedLanguage();
                ApplyLocalization();
                BackupPathInput.Text = _backupFolderPath;
                AutoDetectSteam();
            }
            catch (Exception ex)
            {
                LogOperation("Initialization failed.", NotificationType.Error, ex);
                MessageBox.Show($"Window Init Error:\n{ex}", "Crash Protection", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region СИСТЕМА ЛОКАЛИЗАЦИИ И СЕРВИСЫ УВЕДОМЛЕНИЙ
        private void LogOperation(string message, NotificationType type, Exception? ex = null)
        {
            try
            {
                string status = type.ToString().ToUpper();
                string errDetails = ex != null ? $" | Exception: {ex.Message}\n{ex.StackTrace}" : "";
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{status}] {message}{errDetails}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logLine, Encoding.UTF8);
            }
            catch { }
        }

        public void TriggerToast(string message, NotificationType type)
        {
            try
            {
                NotificationWindow toast = new NotificationWindow(message, type, this);
                toast.Show();
            }
            catch (Exception ex)
            {
                LogOperation("Failed to trigger toast notification.", NotificationType.Error, ex);
            }
        }

        public void LoadSavedLanguage()
        {
            try
            {
                if (File.Exists(_langConfigPath))
                {
                    string content = File.ReadAllText(_langConfigPath).Trim();
                    if (int.TryParse(content, out int index) && index >= 0 && index < Locale.Locales.Length)
                    {
                        _currentLanguageIndex = index;
                    }
                }
            }
            catch { }
        }

        private void SaveLanguageSettings()
        {
            try
            {
                File.WriteAllText(_langConfigPath, _currentLanguageIndex.ToString());
            }
            catch { }
        }

        public void ApplyLocalization()
        {
            var loc = CurrentLocale;
            AppTitleText.Text = loc.Title;

            try
            {
                LangFlagImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(Flags.FlagPaths[_currentLanguageIndex]));
                LangText.Text = Flags.LangNames[_currentLanguageIndex];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language icon: {ex.Message}");
            }

            SaveConfigBtn.Content = loc.SaveBtn;
            LocateConfigsBtn.Content = loc.LocateBtn;
            FindSteamBtn.Content = loc.FindSteamBtn;
            AddConfigToGameBtn.Content = loc.AddToGameBtn;
            ExitBtn.Content = loc.ExitBtn;

            SelectSteamIdTitle.Text = loc.SelectSteamId;
            BackBtn1.Content = loc.Back;

            EnterConfigNameTitle.Text = loc.EnterConfigName;
            OnlyLettersHint.Text = loc.OnlyLetters;
            ConfirmSaveButton.Content = loc.Save;
            BackBtn2.Content = loc.Back;

            BackupFolderTitle.Text = loc.BackupFolderLabel;
            HelpBtn.Content = loc.HelpBtn;

            UpdateSteamStatusText();

            if (_helpWindow != null && _helpWindow.IsLoaded)
            {
                _helpWindow.ApplyLocalization();
            }
        }

        private void LangButton_Click(object sender, RoutedEventArgs e)
        {
            _currentLanguageIndex = (_currentLanguageIndex + 1) % Locale.Locales.Length;
            SaveLanguageSettings();
            ApplyLocalization();
            LogOperation($"Language changed to Index {_currentLanguageIndex} ({CurrentLocale.Title})", NotificationType.Info);
        }

        private void UpdateSteamStatusText()
        {
            if (!string.IsNullOrEmpty(_selectedSteamIdPath))
            {
                string id = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(_selectedSteamIdPath)))) ?? "";
                SteamStatusText.Text = $"{CurrentLocale.SteamPathLabel} ID: {id}";
            }
            else
            {
                SteamStatusText.Text = CurrentLocale.SteamNotFound;
            }
        }
        #endregion

        #region ПЕРЕКЛЮЧЕНИЕ ЭКРАНОВ
        private void ShowScreen(Grid screenToShow)
        {
            MainMenuGrid.Visibility = Visibility.Collapsed;
            SelectAccountGrid.Visibility = Visibility.Collapsed;
            SaveNameGrid.Visibility = Visibility.Collapsed;

            screenToShow.Visibility = Visibility.Visible;
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            ShowScreen(MainMenuGrid);
        }
        #endregion

        #region ПОИСК И НАСТРОЙКА STEAM ПАПОК И ДЕТЕКТ КЛИЕНТОВ
        public void FindSteamFolders_Click(object sender, RoutedEventArgs e)
        {
            LogOperation("Manual Steam folder search triggered.", NotificationType.Info);
            AutoDetectSteam(manualSearchIfFailed: true);
        }

        private void AutoDetectSteam(bool manualSearchIfFailed = false)
        {
            string? steamPath = null;
            using (var key = Registry.CurrentUser.OpenSubKey(AppPaths.FindSteamUserSubKey))
            {
                if (key != null)
                {
                    object? o = key.GetValue("SteamPath");
                    if (o != null)
                    {
                        steamPath = o.ToString()?.Replace('/', '\\');
                    }
                }
            }

            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                if (Directory.Exists(AppPaths.DefaultSteamPath))
                {
                    steamPath = AppPaths.DefaultSteamPath;
                }
            }

            if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
            {
                _detectedSteamPath = steamPath;
                LogOperation($"Steam path resolved: {_detectedSteamPath}", NotificationType.Info);
                AnalyzeUserdata();
            }
            else
            {
                LogOperation("Steam path auto-detection failed.", NotificationType.Warning);
                if (manualSearchIfFailed)
                {
                    ManualSelectSteamFolder();
                }
                else
                {
                    SteamStatusText.Text = CurrentLocale.SteamNotFound;
                }
            }
        }

        private uint GetActiveSteamRegistryUserId()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam\ActiveProcess"))
                {
                    if (key != null)
                    {
                        object? activeUserValue = key.GetValue("ActiveUser");
                        if (activeUserValue != null && activeUserValue is int integerValue)
                        {
                            return (uint)integerValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation("Could not read Active Registry Steam User ID.", NotificationType.Warning, ex);
            }
            return 0;
        }

        private Dictionary<string, (string PersonaName, string AccountName)> ParseSteamLoginUsers()
        {
            var dict = new Dictionary<string, (string PersonaName, string AccountName)>();
            if (string.IsNullOrEmpty(_detectedSteamPath)) return dict;

            string loginUsersFile = Path.Combine(_detectedSteamPath, "config", "loginusers.vdf");
            if (!File.Exists(loginUsersFile)) return dict;

            try
            {
                string content = File.ReadAllText(loginUsersFile);
                var blocks = Regex.Matches(content, @"""(7656119\d{10})""\s*\{([^}]+)\}", RegexOptions.Singleline);
                foreach (Match match in blocks)
                {
                    string steamId64 = match.Groups[1].Value;
                    string blockBody = match.Groups[2].Value;

                    string personaName = "";
                    string accountName = "";

                    var personaMatch = Regex.Match(blockBody, @"""PersonaName""\s+""([^""]+)""", RegexOptions.IgnoreCase);
                    if (personaMatch.Success) personaName = personaMatch.Groups[1].Value;

                    var accountMatch = Regex.Match(blockBody, @"""AccountName""\s+""([^""]+)""", RegexOptions.IgnoreCase);
                    if (accountMatch.Success) accountName = accountMatch.Groups[1].Value;

                    dict[steamId64] = (personaName, accountName);
                }
            }
            catch (Exception ex)
            {
                LogOperation("Error parsing Steam config loginusers.vdf", NotificationType.Warning, ex);
            }
            return dict;
        }

        private void AnalyzeUserdata()
        {
            if (string.IsNullOrEmpty(_detectedSteamPath)) return;

            string userdataPath = Path.Combine(_detectedSteamPath, "userdata");
            if (!Directory.Exists(userdataPath))
            {
                string msg = "Folder 'userdata' not found inside Steam.";
                LogOperation(msg, NotificationType.Error);
                SteamStatusText.Text = msg;
                TriggerToast(msg, NotificationType.Error);
                return;
            }

            var userDirs = Directory.GetDirectories(userdataPath)
                .Select(Path.GetFileName)
                .Where(name => name != null && Regex.IsMatch(name, @"^\d+$"))
                .ToList();

            if (userDirs.Count == 0)
            {
                string msg = "No active steam profiles found in userdata.";
                LogOperation(msg, NotificationType.Warning);
                SteamStatusText.Text = msg;
                TriggerToast(msg, NotificationType.Info);
            }
            else if (userDirs.Count == 1)
            {
                SetSteamId(userDirs[0]);
                LogOperation($"Single Steam profile detected and selected automatically: {userDirs[0]}", NotificationType.Success);
            }
            else
            {
                LogOperation($"Multiple steam accounts found: {userDirs.Count} directories.", NotificationType.Info);
                SteamIdsContainer.Children.Clear();

                uint registryActiveUser32 = GetActiveSteamRegistryUserId();
                var resolvedAccountData = ParseSteamLoginUsers();

                foreach (var id32Str in userDirs)
                {
                    bool isActiveAccount = false;
                    string displayAccountLabel = $"ID: {id32Str}";

                    if (uint.TryParse(id32Str, out uint numericId32))
                    {
                        if (registryActiveUser32 != 0 && numericId32 == registryActiveUser32)
                        {
                            isActiveAccount = true;
                        }

                        // Convert 32-bit ID to 64-bit ID
                        long steamId64 = 76561197960265728L + numericId32;
                        string key64 = steamId64.ToString();

                        if (resolvedAccountData.ContainsKey(key64))
                        {
                            var details = resolvedAccountData[key64];
                            displayAccountLabel = $"{details.PersonaName} ({details.AccountName}) - ID: {id32Str}";
                        }
                    }

                    if (isActiveAccount)
                    {
                        displayAccountLabel += " [Current active account]";
                    }

                    Button btn = new Button
                    {
                        Content = displayAccountLabel,
                        Style = (Style)FindResource("MenuButtonStyle"),
                        Tag = id32Str
                    };

                    if (isActiveAccount)
                    {
                        btn.BorderBrush = System.Windows.Media.Brushes.LimeGreen;
                        btn.BorderThickness = new Thickness(1.5);
                        btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 60, 25));
                    }

                    btn.Click += SteamIdSelect_Click;
                    SteamIdsContainer.Children.Add(btn);
                }

                Button manualBtn = new Button
                {
                    Content = "Select folder manually...",
                    Style = (Style)FindResource("MenuButtonStyle"),
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 45))
                };
                manualBtn.Click += (s, e) => ManualSelectSteamFolder();
                SteamIdsContainer.Children.Add(manualBtn);

                ShowScreen(SelectAccountGrid);
            }
        }

        private void SteamIdSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                SetSteamId(id);
                LogOperation($"Steam account manually selected: {id}", NotificationType.Success);
                ShowScreen(MainMenuGrid);
            }
        }

        private void SetSteamId(string steamId)
        {
            if (string.IsNullOrEmpty(_detectedSteamPath)) return;

            _selectedSteamIdPath = Path.Combine(_detectedSteamPath, "userdata", steamId, "730", "local", "cfg");
            UpdateSteamStatusText();
        }

        private void ManualSelectSteamFolder()
        {
            IsDialogOpen = true;
            try
            {
                using (var dialog = new Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Select your Steam folder (or direct 'cfg' folder)";
                    Forms.DialogResult result = dialog.ShowDialog();

                    if (result == Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    {
                        if (dialog.SelectedPath.EndsWith("cfg", StringComparison.OrdinalIgnoreCase))
                        {
                            _selectedSteamIdPath = dialog.SelectedPath;
                            SteamStatusText.Text = "Manual path: ...\\730\\local\\cfg";
                            LogOperation($"Steam manual path directly bound to cfg: {_selectedSteamIdPath}", NotificationType.Success);
                        }
                        else
                        {
                            _detectedSteamPath = dialog.SelectedPath;
                            LogOperation($"Steam manual root path specified: {_detectedSteamPath}", NotificationType.Info);
                            AnalyzeUserdata();
                        }
                        ShowScreen(MainMenuGrid);
                    }
                }
            }
            finally
            {
                IsDialogOpen = false;
            }
        }
        #endregion

        #region СОХРАНЕНИЕ И ДОБАВЛЕНИЕ КОНФИГА В ИГРУ
        public void SaveConfigStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedSteamIdPath) || !Directory.Exists(_selectedSteamIdPath))
            {
                string errMsg = CurrentLocale.SteamNotFound;
                LogOperation("Attempted to initiate save config without loaded Steam profile folder.", NotificationType.Warning);
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TriggerToast(errMsg, NotificationType.Error);
                return;
            }

            ConfigNameInput.Clear();
            ShowScreen(SaveNameGrid);
        }

        private void ConfigNameInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-ЯёЁ\s]+$");
        }

        private void ConfigNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ConfigNameInput.Text.Trim();
            ConfirmSaveButton.Visibility = (!string.IsNullOrEmpty(text) && Regex.IsMatch(text, @"^[a-zA-Zа-яА-ЯёЁ\s]+$"))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ConfirmSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedSteamIdPath) || !Directory.Exists(_selectedSteamIdPath)) return;

            string configName = ConfigNameInput.Text.Trim();
            string targetDirectory = Path.Combine(_backupFolderPath, configName);
            string rawDirectory = Path.Combine(targetDirectory, "raw");

            try
            {
                string machineConvarsPath = Path.Combine(_selectedSteamIdPath, AppPaths.MachineConvarsFile);

                string userConvarsPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserConvarsFile);
                if (!File.Exists(userConvarsPath))
                    userConvarsPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserConvarsFileAlt);

                string userKeysPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserKeysFile);
                if (!File.Exists(userKeysPath))
                    userKeysPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserKeysFileAlt);

                if (!File.Exists(machineConvarsPath) && !File.Exists(userConvarsPath) && !File.Exists(userKeysPath))
                {
                    string vcfgMissingText = "Required .vcfg config files were not found.";
                    LogOperation($"Failed to backup configurations: .vcfg missing in {_selectedSteamIdPath}", NotificationType.Error);
                    MessageBox.Show(vcfgMissingText, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TriggerToast(vcfgMissingText, NotificationType.Error);
                    return;
                }

                if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                if (!Directory.Exists(rawDirectory)) Directory.CreateDirectory(rawDirectory);

                if (File.Exists(machineConvarsPath))
                    File.Copy(machineConvarsPath, Path.Combine(rawDirectory, Path.GetFileName(machineConvarsPath)), true);
                if (File.Exists(userConvarsPath))
                    File.Copy(userConvarsPath, Path.Combine(rawDirectory, Path.GetFileName(userConvarsPath)), true);
                if (File.Exists(userKeysPath))
                    File.Copy(userKeysPath, Path.Combine(rawDirectory, Path.GetFileName(userKeysPath)), true);

                StringBuilder mergedContent = new StringBuilder();

                if (File.Exists(machineConvarsPath))
                {
                    mergedContent.AppendLine("// === Machine Convars ===");
                    mergedContent.AppendLine(ParseVcfg(machineConvarsPath));
                    mergedContent.AppendLine();
                }

                if (File.Exists(userConvarsPath))
                {
                    mergedContent.AppendLine("// === User Convars ===");
                    mergedContent.AppendLine(ParseVcfg(userConvarsPath));
                    mergedContent.AppendLine();
                }

                if (File.Exists(userKeysPath))
                {
                    mergedContent.AppendLine("// === User Keys ===");
                    mergedContent.AppendLine(ParseVcfg(userKeysPath));
                    mergedContent.AppendLine();
                }

                string mergedConfigPath = Path.Combine(targetDirectory, $"{configName}.cfg");
                File.WriteAllText(mergedConfigPath, mergedContent.ToString(), Encoding.UTF8);

                string successMessage = $"{CurrentLocale.CopySuccess}\n\nPath: {mergedConfigPath}";
                LogOperation($"Config saved successfully: {mergedConfigPath}", NotificationType.Success);
                MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                TriggerToast("Config Saved Successfully!", NotificationType.Success);

                ShowScreen(MainMenuGrid);
            }
            catch (Exception ex)
            {
                LogOperation("Error saving configuration.", NotificationType.Error, ex);
                MessageBox.Show($"An error occurred during save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TriggerToast("Error saving configuration!", NotificationType.Error);
            }
        }

        private string ParseVcfg(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            var lines = File.ReadAllLines(filePath);
            StringBuilder cleanLines = new StringBuilder();

            string currentSection = "";
            int braceDepth = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (trimmed == "{")
                {
                    braceDepth++;
                    continue;
                }
                if (trimmed == "}")
                {
                    braceDepth--;
                    if (braceDepth <= 1)
                    {
                        currentSection = "";
                    }
                    continue;
                }

                if (trimmed.Equals("\"bindings\"", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "bindings";
                    continue;
                }
                if (trimmed.Equals("\"analogbindings\"", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "analogbindings";
                    continue;
                }
                if (trimmed.Equals("\"convars\"", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "convars";
                    continue;
                }

                var match = Regex.Match(trimmed, @"^""([^""]+)""\s+""(.*)""$");
                if (match.Success)
                {
                    string key = match.Groups[1].Value;
                    string val = match.Groups[2].Value;

                    if (currentSection == "bindings" || currentSection == "analogbindings")
                    {
                        cleanLines.AppendLine($"bind \"{key}\" \"{val}\"");
                    }
                    else if (currentSection == "convars")
                    {
                        cleanLines.AppendLine($"{key} \"{val}\"");
                    }
                }
            }

            return cleanLines.ToString();
        }

        public void AddConfigToGame_Click(object sender, RoutedEventArgs e)
        {
            _ = AddConfigAndAutomateAsync(null);
        }

        public async Task<bool> AddConfigAndAutomateAsync(Button? runBtn)
        {
            if (string.IsNullOrEmpty(_detectedSteamPath) || !Directory.Exists(_detectedSteamPath))
            {
                string noSteamText = CurrentLocale.SteamNotFound;
                LogOperation("Attempted importing configuration without detected Steam folder path.", NotificationType.Warning);
                MessageBox.Show(noSteamText, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TriggerToast(noSteamText, NotificationType.Error);
                return false;
            }

            string cs2ConfigFolder = Path.Combine(_detectedSteamPath, AppPaths.SteamConfigInjectionFolder);

            if (!Directory.Exists(cs2ConfigFolder))
            {
                string pathNotFoundMsg = CurrentLocale.GamePathNotFound + $"\nExpected path:\n{cs2ConfigFolder}";
                LogOperation($"CS2 CFG folder path not found: {cs2ConfigFolder}", NotificationType.Error);
                MessageBox.Show(pathNotFoundMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TriggerToast("CS2 Folder Not Found!", NotificationType.Error);
                return false;
            }

            bool? dialogResult = false;
            string sourceFile = "";

            IsDialogOpen = true; // Блокируем закрытие окон на время выбора файла
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    InitialDirectory = _backupFolderPath,
                    Filter = "Config files (*.cfg)|*.cfg",
                    Title = CurrentLocale.SelectFile
                };
                dialogResult = openDialog.ShowDialog();
                sourceFile = openDialog.FileName;
            }
            finally
            {
                IsDialogOpen = false; // Возвращаем обычную логику потери фокуса
            }

            if (dialogResult == true)
            {
                try
                {
                    string destFile = Path.Combine(cs2ConfigFolder, Path.GetFileName(sourceFile));

                    File.Copy(sourceFile, destFile, overwrite: true);

                    string configNameOnly = Path.GetFileNameWithoutExtension(sourceFile);
                    string execCommand = $"exec {configNameOnly}";

                    Clipboard.SetText(execCommand);

                    string notificationMsg = string.Format(CurrentLocale.NotificationFormat, execCommand);
                    TriggerToast(notificationMsg, NotificationType.Success);
                    LogOperation($"Successfully copied config file {sourceFile} into {destFile}. Copied command: '{execCommand}'", NotificationType.Success);

                    await SendCommandToCS2Async(execCommand, runBtn);
                    return true;
                }
                catch (Exception ex)
                {
                    LogOperation("Error copying config file to CS2 folder.", NotificationType.Error, ex);
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    TriggerToast("Failed copying config to CS2!", NotificationType.Error);
                }
            }
            return false;
        }
        #endregion

        #region СЕРВИСНЫЕ ИНСТРУМЕНТЫ ВЗАИМОДЕЙСТВИЯ С CS2
        public async Task<bool> SendCommandToCS2Async(string command, Button? feedbackBtn)
        {
            Clipboard.SetText(command);

            var processes = System.Diagnostics.Process.GetProcessesByName("cs2");
            if (processes.Length == 0)
            {
                string notRunningMsg = CurrentLocale.GamePathNotFound;
                LogOperation("CS2 is not running. Could not inject commands automatically.", NotificationType.Warning);
                MessageBox.Show(notRunningMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TriggerToast("Counter-Strike 2 is offline!", NotificationType.Info);
                return false;
            }

            IntPtr handle = processes[0].MainWindowHandle;
            string originalContent = feedbackBtn != null ? feedbackBtn.Content.ToString() ?? "Run" : "Run";

            for (int i = 7; i > 0; i--)
            {
                if (feedbackBtn != null) feedbackBtn.Content = $"{i}s...";
                await Task.Delay(1000);
            }

            if (feedbackBtn != null) feedbackBtn.Content = originalContent;

            try
            {
                ShowWindow(handle, SW_RESTORE);
                SetForegroundWindow(handle);
                await Task.Delay(350); // Увеличили паузу фокуса

                string consoleKey = GetConsoleKey();

                // 1. Открываем консоль (зажатие на 100 мс)
                SendConsoleHardwareKey(consoleKey);
                await Task.Delay(400); // Даем консоли в CS2 время гарантированно отрисоваться

                // 2. Вставляем текст и нажимаем Enter
                SendPasteAndEnterHardware();
                LogOperation($"Hardware console key emulation sent to CS2 process.", NotificationType.Success);
            }
            catch (Exception ex)
            {
                LogOperation("Failed hardware input simulation.", NotificationType.Error, ex);
                System.Diagnostics.Debug.WriteLine($"Hardware input error: {ex.Message}");
            }

            return true;
        }

        private void SendConsoleHardwareKey(string key)
        {
            byte vk = 0xC0; // tilde `

            switch (key.ToLower())
            {
                case "{esc}": vk = 0x1B; break;
                case "{f1}": vk = 0x70; break;
                case "{f2}": vk = 0x71; break;
                case "{f3}": vk = 0x72; break;
                case "{f4}": vk = 0x73; break;
                case "{f5}": vk = 0x74; break;
                case "{f6}": vk = 0x75; break;
                case "{f7}": vk = 0x76; break;
                case "{f8}": vk = 0x77; break;
                case "{f9}": vk = 0x78; break;
                case "{f10}": vk = 0x79; break;
                case "{f11}": vk = 0x7A; break;
                case "{f12}": vk = 0x7B; break;
                case "`":
                case "~":
                    vk = 0xC0; break;
                default:
                    if (key.Length == 1)
                    {
                        char c = char.ToUpper(key[0]);
                        if (c >= 'A' && c <= 'Z') vk = (byte)c;
                        else if (c >= '0' && c <= '9') vk = (byte)c;
                    }
                    break;
            }

            keybd_event(vk, 0, 0, 0);
            System.Threading.Thread.Sleep(100);
            keybd_event(vk, 0, KEYEVENTF_KEYUP, 0);
        }

        private void SendPasteAndEnterHardware()
        {
            // Нажимаем Ctrl (0x11)
            keybd_event(0x11, 0, 0, 0);
            System.Threading.Thread.Sleep(30);

            // Нажимаем V (0x56)
            keybd_event(0x56, 0, 0, 0);
            System.Threading.Thread.Sleep(50);

            // Отпускаем V
            keybd_event(0x56, 0, KEYEVENTF_KEYUP, 0);
            System.Threading.Thread.Sleep(30);

            // Отпускаем Ctrl
            keybd_event(0x11, 0, KEYEVENTF_KEYUP, 0);
            System.Threading.Thread.Sleep(150); // Пауза для обработки буфера

            // Нажимаем Enter (0x0D)
            keybd_event(0x0D, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            keybd_event(0x0D, 0, KEYEVENTF_KEYUP, 0);
        }

        private string GetConsoleKey()
        {
            if (string.IsNullOrEmpty(_selectedSteamIdPath)) return "`";

            string userKeysPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserKeysFile);
            if (!File.Exists(userKeysPath))
                userKeysPath = Path.Combine(_selectedSteamIdPath, AppPaths.UserKeysFileAlt);

            if (File.Exists(userKeysPath))
            {
                try
                {
                    var lines = File.ReadAllLines(userKeysPath);
                    foreach (var line in lines)
                    {
                        if (line.Contains("toggleconsole"))
                        {
                            var match = Regex.Match(line.Trim(), @"^""([^""]+)""\s+""toggleconsole""");
                            if (match.Success)
                            {
                                string key = match.Groups[1].Value.ToLower();
                                return TranslateVcfgKey(key);
                            }
                        }
                    }
                }
                catch { }
            }
            return "`";
        }

        private string TranslateVcfgKey(string rawKey)
        {
            switch (rawKey)
            {
                case "escape": return "{ESC}";
                case "f1": return "{F1}";
                case "f2": return "{F2}";
                case "f3": return "{F3}";
                case "f4": return "{F4}";
                case "f5": return "{F5}";
                case "f6": return "{F6}";
                case "f7": return "{F7}";
                case "f8": return "{F8}";
                case "f9": return "{F9}";
                case "f10": return "{F10}";
                case "f11": return "{F11}";
                case "f12": return "{F12}";
                default: return rawKey;
            }
        }
        #endregion

        #region ДРУГИЕ ДЕЙСТВИЯ
        private void LocateConfigs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(_backupFolderPath))
                {
                    Directory.CreateDirectory(_backupFolderPath);
                }
                System.Diagnostics.Process.Start("explorer.exe", _backupFolderPath);
                LogOperation($"Opened backups folder directory in Explorer: {_backupFolderPath}", NotificationType.Info);
            }
            catch (Exception ex)
            {
                LogOperation("Could not open backups folder in Explorer.", NotificationType.Warning, ex);
            }
        }

        public void BackupPathInput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IsDialogOpen = true; // Блокируем деактивацию
            try
            {
                using (var dialog = new Forms.FolderBrowserDialog())
                {
                    dialog.Description = "Select folder to store your backups";
                    if (dialog.ShowDialog() == Forms.DialogResult.OK)
                    {
                        _backupFolderPath = dialog.SelectedPath;
                        BackupPathInput.Text = _backupFolderPath;
                        LogOperation($"Backup folder location successfully changed manually: {_backupFolderPath}", NotificationType.Success);
                    }
                }
            }
            finally
            {
                IsDialogOpen = false; // Возвращаем проверку фокуса
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_helpWindow == null)
                {
                    _helpWindow = new HelpWindow(this);
                    _helpWindow.Closed += (s, args) => _helpWindow = null;

                    _helpWindow.Show();

                    double helpWidth = _helpWindow.ActualWidth > 0 ? _helpWindow.ActualWidth : 430;
                    _helpWindow.Left = this.Left - helpWidth - 10;
                    _helpWindow.Top = this.Top;
                    LogOperation("Help window overlay opened.", NotificationType.Info);
                }
                else
                {
                    _helpWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                LogOperation("Error opening Help window.", NotificationType.Error, ex);
                MessageBox.Show($"Error opening Help Window:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LogOperation("Application exit sequence requested.", NotificationType.Info);
            Application.Current.Shutdown();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Если сейчас открыто системное диалоговое окно (выбор файла/папки), игнорируем уход фокуса
                if (IsDialogOpen)
                {
                    return;
                }

                if (_helpWindow != null && _helpWindow.IsActive)
                {
                    return;
                }

                if (Application.Current.Windows.Cast<Window>().Any(w => w.IsActive))
                {
                    return;
                }

                if (_helpWindow != null)
                {
                    _helpWindow.Close();
                }
                this.Hide();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        #endregion
    }
}