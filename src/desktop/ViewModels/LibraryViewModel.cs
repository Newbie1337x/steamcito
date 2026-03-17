using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Services;
using steamcito.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace steamcito.ViewModels
{
    public partial class LibraryViewModel : ObservableObject, IRecipient<GameAddedMessage>
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
            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(GameAddedMessage message)
        {
            Games.Add(message.Game);
            if (SelectedGame == null)
            {
                SelectedGame = message.Game;
            }
        }

        private void LoadGames()
        {
            var allGames = _gameService.GetAll();
            Games = new ObservableCollection<Game>(allGames);
            if (Games.Count > 0 && SelectedGame == null)
            {
                SelectedGame = Games[0];
            }
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
