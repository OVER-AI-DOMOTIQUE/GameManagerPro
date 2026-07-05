using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace GameManagerPro.Services
{
    public class ScannerService
    {
        private readonly SettingsService _settingsService;
        private readonly Timer _timer;

        public event EventHandler<string> NewExecutableFound;

        public ScannerService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _timer = new Timer(35000); // 35 seconds
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            _timer.Start();
            // Trigger immediately
            ScanDirectory();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ScanDirectory();
        }

        public void ScanDirectory()
        {
            var dir = _settingsService.Settings.GameDirectory;
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;

            Task.Run(() => SafeScanDirectory(dir));
        }

        private void SafeScanDirectory(string dir)
        {
            try
            {
                var exeFiles = Directory.GetFiles(dir, "*.exe");

                var known = _settingsService.Settings.KnownExecutables;
                var excluded = _settingsService.Settings.ExcludedExecutables;

                foreach (var file in exeFiles)
                {
                    if (!known.Contains(file, StringComparer.OrdinalIgnoreCase) &&
                        !excluded.Contains(file, StringComparer.OrdinalIgnoreCase))
                    {
                        NewExecutableFound?.Invoke(this, file);
                    }
                }

                var subDirs = Directory.GetDirectories(dir);
                foreach (var subDir in subDirs)
                {
                    SafeScanDirectory(subDir);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore permissions issues for this specific folder
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning directory {dir}: {ex.Message}");
            }
        }
    }
}
