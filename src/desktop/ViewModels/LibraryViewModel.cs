using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using steamcito.Services;
using steamcito.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace steamcito.ViewModels
{
    public partial class LibraryViewModel : ObservableObject
    {
        private readonly GameService _gameService;

        [ObservableProperty]
        private ObservableCollection<Game> games = new();

        [ObservableProperty]
        private Game? selectedGame;

        public LibraryViewModel()
        {
            _gameService = new GameService(new steamcito.Data.AppDBContext());
            LoadGames();
        }

        private void LoadGames()
        {
            Games = new ObservableCollection<Game>(_gameService.GetAll());
            Debug.WriteLine($"Loaded {Games.Count} games from the database.");

        }

        [RelayCommand]
        private void RunGame()
        {
            if (SelectedGame?.GamePaths?.ExePath == null)
            {
                return;
            }
            _gameService.runGame(SelectedGame.GamePaths.ExePath);
            Debug.WriteLine($"Running game: {SelectedGame.Details.Title} at {SelectedGame.GamePaths.ExePath}");
        }
    }
}
