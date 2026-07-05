using CommunityToolkit.Mvvm.ComponentModel;

namespace GameManagerPro.Models
{
    public partial class GameModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string executablePath;

        [ObservableProperty]
        private string imageUrl;
    }
}
