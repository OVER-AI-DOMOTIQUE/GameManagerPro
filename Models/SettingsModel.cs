using System.Collections.Generic;

namespace GameManagerPro.Models
{
    public class SettingsModel
    {
        public string GameDirectory { get; set; } = string.Empty;
        public bool LaunchWithWindows { get; set; } = false;
        public bool EnableTray { get; set; } = true;
        public string StartupMode { get; set; } = "Window";
        public string SortOrder { get; set; } = "A-Z";
        public List<string> ExcludedExecutables { get; set; } = new List<string>();
        public List<string> KnownExecutables { get; set; } = new List<string>();
        public string SteamGridDbApiKey { get; set; } = string.Empty;
    }
}
