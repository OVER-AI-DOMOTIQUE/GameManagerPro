using System.Windows;
using System.Windows.Controls;
using GameManagerPro.ViewModels;
using H.NotifyIcon;

namespace GameManagerPro.Views
{
    public partial class TrayPopupView : UserControl
    {
        public TrayPopupView()
        {
            InitializeComponent();
            Loaded += TrayPopupView_Loaded;
        }

        private void TrayPopupView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow?.DataContext is MainViewModel vm)
            {
                DataContext = vm;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Popup closes automatically when focus is lost, or we can just hide the window if needed.
            // But since this is a TrayPopup, we can close it by simulating a click elsewhere, or hiding the popup.
            if (this.Parent is System.Windows.Controls.Primitives.Popup popup)
            {
                popup.IsOpen = false;
            }
        }

        private void OpenApp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            }
            
            if (this.Parent is System.Windows.Controls.Primitives.Popup popup)
            {
                popup.IsOpen = false;
            }
        }

        private void Exit_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
