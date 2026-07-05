using System.Windows;
using GameManagerPro.ViewModels;

namespace GameManagerPro.Views
{
    public partial class NewExePromptView : Window
    {
        public string ExecutablePath { get; set; }

        public NewExePromptView(string exePath)
        {
            ExecutablePath = exePath;
            InitializeComponent();
            DataContext = this;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            App.SettingsService.Settings.KnownExecutables.Add(ExecutablePath);
            App.SettingsService.SaveSettings();
            DialogResult = true;
            Close();
        }

        private void Exclude_Click(object sender, RoutedEventArgs e)
        {
            App.SettingsService.Settings.ExcludedExecutables.Add(ExecutablePath);
            App.SettingsService.SaveSettings();
            DialogResult = false;
            Close();
        }
    }
}
