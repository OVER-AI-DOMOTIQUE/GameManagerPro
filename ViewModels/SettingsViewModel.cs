using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms;

namespace GameManagerPro.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string gameDirectory;

        [ObservableProperty]
        private bool launchWithWindows;

        [ObservableProperty]
        private string steamGridDbApiKey;

        [ObservableProperty]
        private bool enableTray;

        [ObservableProperty]
        private string startupMode;

        public MaterialDesignThemes.Wpf.ISnackbarMessageQueue SettingsSnackbarMessageQueue { get; } = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(System.TimeSpan.FromSeconds(5));

        public SettingsViewModel()
        {
            GameDirectory = App.SettingsService.Settings.GameDirectory;
            LaunchWithWindows = App.SettingsService.Settings.LaunchWithWindows;
            SteamGridDbApiKey = App.SettingsService.Settings.SteamGridDbApiKey;
            EnableTray = App.SettingsService.Settings.EnableTray;
            StartupMode = App.SettingsService.Settings.StartupMode;
        }

        [RelayCommand]
        private void BrowseDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "SÃ©lectionnez le rÃ©pertoire de jeux";
                dialog.SelectedPath = GameDirectory;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    GameDirectory = dialog.SelectedPath;
                }
            }
        }

        [RelayCommand]
        private void Save(Window window)
        {
            if (LaunchWithWindows && StartupMode == "Tray" && !EnableTray)
            {
                SettingsSnackbarMessageQueue.Enqueue("Vous devez activer le System Tray pour démarrer réduit.");
                EnableTray = true;
                return;
            }

            App.SettingsService.Settings.GameDirectory = GameDirectory;
            App.SettingsService.Settings.LaunchWithWindows = LaunchWithWindows;
            App.SettingsService.Settings.SteamGridDbApiKey = SteamGridDbApiKey;
            App.SettingsService.Settings.EnableTray = EnableTray;
            App.SettingsService.Settings.StartupMode = StartupMode;
            App.SettingsService.SaveSettings();
            
            UpdateStartupKey(LaunchWithWindows, StartupMode == "Tray");

            App.TrayService.UpdateTrayState();
            App.ScannerService.ScanDirectory();

            window.DialogResult = true;
            window.Close();
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task TestApi()
        {
            if (string.IsNullOrWhiteSpace(SteamGridDbApiKey))
            {
                SettingsSnackbarMessageQueue.Enqueue("Veuillez d'abord entrer une clé API.");
                return;
            }

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {SteamGridDbApiKey}");
                    var response = await client.GetAsync("https://www.steamgriddb.com/api/v2/search/autocomplete/ReadyOrNot");
                    if (response.IsSuccessStatusCode)
                    {
                        SettingsSnackbarMessageQueue.Enqueue("✅ L'API SteamGridDB fonctionne correctement !");
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        SettingsSnackbarMessageQueue.Enqueue($"❌ Erreur API ({response.StatusCode})");
                    }
                }
            }
            catch (System.Exception ex)
            {
                SettingsSnackbarMessageQueue.Enqueue($"❌ Erreur de connexion: {ex.Message}");
            }
        }

        private void UpdateStartupKey(bool enable, bool startInTray)
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                var appName = "GameManagerPro";
                if (enable)
                {
                    var args = startInTray ? " --tray" : "";
                    key?.SetValue(appName, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"{args}");
                }
                else
                {
                    key?.DeleteValue(appName, false);
                }
            }
            catch { }
        }
    }
}
