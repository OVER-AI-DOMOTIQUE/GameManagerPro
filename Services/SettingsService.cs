using Newtonsoft.Json;
using System;
using System.IO;
using GameManagerPro.Models;

namespace GameManagerPro.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private SettingsModel _currentSettings;

        public SettingsModel Settings => _currentSettings;

        public SettingsService()
        {
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                _currentSettings = JsonConvert.DeserializeObject<SettingsModel>(json) ?? new SettingsModel();
            }
            else
            {
                _currentSettings = new SettingsModel();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(_currentSettings, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
