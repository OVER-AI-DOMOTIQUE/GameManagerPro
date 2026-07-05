using System.Collections.ObjectModel;
using System.Linq;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using GameManagerPro.Models;

namespace GameManagerPro.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        [NotifyPropertyChangedFor(nameof(IsNotEmpty))]
        private ObservableCollection<GameModel> games = new();

        [ObservableProperty]
        private string gameDirectory;

        public ISnackbarMessageQueue MainSnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        public bool IsEmpty => Games.Count == 0;
        public bool IsNotEmpty => Games.Count > 0;

        public System.ComponentModel.ICollectionView GamesView { get; }

        [ObservableProperty]
        private string selectedSort;

        public MainViewModel()
        {
            GamesView = System.Windows.Data.CollectionViewSource.GetDefaultView(Games);
            SelectedSort = App.SettingsService.Settings.SortOrder;
            
            GameDirectory = App.SettingsService.Settings.GameDirectory;
            LoadGames();
        }

        partial void OnSelectedSortChanged(string value)
        {
            ApplySort(value);
            App.SettingsService.Settings.SortOrder = value;
            App.SettingsService.SaveSettings();
        }

        private void ApplySort(string sort)
        {
            if (GamesView == null) return;
            GamesView.SortDescriptions.Clear();
            if (sort == "A-Z")
            {
                GamesView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
            }
            else if (sort == "Z-A")
            {
                GamesView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Descending));
            }
        }

        public async void LoadGames()
        {
            Games.Clear();
            var executables = App.SettingsService.Settings.KnownExecutables.ToList();
            foreach (var exe in executables)
            {
                var gameName = System.IO.Path.GetFileNameWithoutExtension(exe);
                var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(exe));
                var game = new GameModel
                {
                    Name = gameName,
                    ExecutablePath = exe
                };
                Games.Add(game);

                var imageUrl = await App.MetadataService.GetGameImageUrlAsync(gameName, folderName);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    game.ImageUrl = imageUrl;
                }
            }
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }

        [RelayCommand]
        private void LaunchGame(GameModel game)
        {
            if (System.IO.File.Exists(game.ExecutablePath))
            {
                var processName = System.IO.Path.GetFileNameWithoutExtension(game.ExecutablePath);
                var existing = System.Diagnostics.Process.GetProcessesByName(processName);
                if (existing.Length > 0)
                {
                    // Le jeu est déjà en cours d'exécution
                    return;
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(game.ExecutablePath) { UseShellExecute = true });
            }
        }

        [RelayCommand]
        private async System.Threading.Tasks.Task FetchGrid(GameModel game)
        {
            var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(game.ExecutablePath));
            var imageUrl = await App.MetadataService.GetGameImageUrlAsync(game.Name, folderName);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                game.ImageUrl = imageUrl;
                MainSnackbarMessageQueue.Enqueue($"Image rÃ©cupÃ©rÃ©e pour {game.Name} !");
            }
            else
            {
                MainSnackbarMessageQueue.Enqueue($"Impossible de trouver l'image pour {game.Name}.");
            }
        }

        [RelayCommand]
        private void RemoveGame(GameModel game)
        {
            App.SettingsService.Settings.KnownExecutables.Remove(game.ExecutablePath);
            App.SettingsService.SaveSettings();
            Games.Remove(game);
            MainSnackbarMessageQueue.Enqueue($"{game.Name} a été supprimé.");
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }

        [RelayCommand]
        private void ExcludeGame(GameModel game)
        {
            App.SettingsService.Settings.KnownExecutables.Remove(game.ExecutablePath);
            if (!App.SettingsService.Settings.ExcludedExecutables.Contains(game.ExecutablePath, StringComparer.OrdinalIgnoreCase))
            {
                App.SettingsService.Settings.ExcludedExecutables.Add(game.ExecutablePath);
            }
            App.SettingsService.SaveSettings();
            Games.Remove(game);
            MainSnackbarMessageQueue.Enqueue($"{game.Name} a été exclu.");
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(IsNotEmpty));
        }

        [RelayCommand]
        private void ScanDirectory()
        {
            if (string.IsNullOrWhiteSpace(App.SettingsService.Settings.GameDirectory))
            {
                MainSnackbarMessageQueue.Enqueue("Veuillez d'abord configurer un répertoire dans les paramètres.");
                return;
            }

            MainSnackbarMessageQueue.Enqueue($"Début du scan dans {App.SettingsService.Settings.GameDirectory}...");
            App.ScannerService.ScanDirectory();
        }
    }
}
