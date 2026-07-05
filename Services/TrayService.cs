using H.NotifyIcon;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace GameManagerPro.Services
{
    public class TrayService
    {
        private TaskbarIcon _taskbarIcon;

        public void Initialize()
        {
            UpdateTrayState();
        }

        public void UpdateTrayState()
        {
            var enableTray = App.SettingsService.Settings.EnableTray;

            if (enableTray)
            {
                if (_taskbarIcon == null)
                {
                    _taskbarIcon = new TaskbarIcon
                    {
                        ToolTipText = "Game Manager Pro",
                        Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
                    };

                    _taskbarIcon.ForceCreate();

                    _taskbarIcon.TrayPopup = new Views.TrayPopupView();
                    
                }
            }
            else
            {
                if (_taskbarIcon != null)
                {
                    _taskbarIcon.Dispose();
                    _taskbarIcon = null;
                }
            }
        }

        public void ShowNotification(string title, string message)
        {
            if (App.SettingsService.Settings.EnableTray)
            {
                _taskbarIcon?.ShowNotification(title, message);
            }
        }
        
        public void Dispose()
        {
            _taskbarIcon?.Dispose();
        }
    }
}
