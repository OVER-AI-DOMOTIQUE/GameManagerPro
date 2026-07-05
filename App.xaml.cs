using System.Windows;
using GameManagerPro.Services;

namespace GameManagerPro
{
    public partial class App : Application
    {
        public static SettingsService SettingsService { get; private set; }
        public static ScannerService ScannerService { get; private set; }
        public static TrayService TrayService { get; private set; }
        public static MetadataService MetadataService { get; private set; }

        private static System.Threading.Mutex _mutex = null;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _mutex = new System.Threading.Mutex(true, "GameManagerPro_SingleInstance_Mutex", out bool createdNew);
            if (!createdNew)
            {
                Application.Current.Shutdown();
                return;
            }

            SettingsService = new SettingsService();
            MetadataService = new MetadataService();
            TrayService = new TrayService();
            TrayService.Initialize();

            ScannerService = new ScannerService(SettingsService);
            ScannerService.NewExecutableFound += ScannerService_NewExecutableFound;
            ScannerService.Start();

            var startInTray = false;
            foreach (var arg in e.Args)
            {
                if (arg.ToLower() == "--tray") startInTray = true;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            
            if (startInTray && SettingsService.Settings.EnableTray)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.ShowInTaskbar = false;
                mainWindow.Show();
                mainWindow.Hide();
            }
            else
            {
                mainWindow.Show();
            }
            
            if (string.IsNullOrWhiteSpace(SettingsService.Settings.GameDirectory))
            {
                var settingsView = new Views.SettingsView();
                settingsView.Owner = mainWindow;
                settingsView.ShowDialog();
                if (mainWindow.DataContext is ViewModels.MainViewModel vm)
                {
                    vm.LoadGames();
                }
            }
        }

        private void ScannerService_NewExecutableFound(object sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    TrayService.ShowNotification("Nouveau jeu trouvé", $"Exécutable détecté : {System.IO.Path.GetFileName(e)}");
                    var prompt = new Views.NewExePromptView(e);
                    if (prompt.ShowDialog() == true)
                    {
                        if (Application.Current.MainWindow?.DataContext is ViewModels.MainViewModel vm)
                        {
                            vm.LoadGames();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'affichage du prompt: {ex.Message}");
                }
            });
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            TrayService?.Dispose();
            ScannerService?.Stop();
        }
    }
}
