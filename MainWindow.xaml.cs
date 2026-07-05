using System.Windows;
using GameManagerPro.ViewModels;

namespace GameManagerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Views.SettingsView();
            if (settingsWindow.ShowDialog() == true)
            {
                ((MainViewModel)DataContext).LoadGames();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.SettingsService.Settings.EnableTray)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}