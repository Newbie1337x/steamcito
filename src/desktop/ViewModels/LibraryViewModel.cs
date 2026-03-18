using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Services;
using steamcito.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using steamcito.Views;
using Application = System.Windows.Application;

namespace steamcito.ViewModels
{
    public partial class LibraryViewModel : ObservableObject, IRecipient<GameAddedMessage>
    {
        private readonly GameService _gameService;
        private readonly GameSessionManager _gameSessionManager = new();

        [ObservableProperty] private ObservableCollection<Game> games = new();

        [ObservableProperty] private Game? _selectedGame;

        public LibraryViewModel(GameService gameService)
        {
            _gameService = gameService;
            LoadGames();
            WeakReferenceMessenger.Default.Register(this);

            _gameSessionManager.OnGameClosed += (game, playTime) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    game.Details.PlayTime += playTime;
                    game.Details.LastTime = DateTime.Now;

                    _gameService.Update(game);

                    Debug.WriteLine($"Game {game.Details.Title} closed after {playTime}");
                });
            };
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
        private void PlayGame(Game game)
        {
            if (game?.GamePaths?.ExePath == null)
                return;

            if (!game.IsRunning)
            {
                _gameSessionManager.LaunchGame(game);
                Debug.WriteLine($"Running game: {game.Details.Title}");
            }
            else
            {
                _gameSessionManager.StopGame();
                Debug.WriteLine($"Stopping/Cancelling game: {game.Details.Title}");
            }
        }
        
        [RelayCommand]
        private void CreateShortcut(Game game)
        {
            if (game.GamePaths?.ExePath == null )
                return;
            
           _gameService.CreateShortcut(game.GamePaths, game.Details.Title);
        }

        [RelayCommand]
        private void ToggleFavorite(Game game)
        {
            game.Details.Favorite = !game.Details.Favorite;
            _gameService.Update(game);
        }



        [RelayCommand]
        private void RemoveGame(Game? game)
        {
            if (game == null) return;

            _gameService.RemoveGame(game);
            Games.Remove(game);

            if (SelectedGame == game)
                SelectedGame = Games.FirstOrDefault();
        }
        
        [RelayCommand]
        private  void OpenFolder(Game game)
        {
            if (game?.GamePaths?.FolderPath == null)
                return;
            
            if (Directory.Exists(game.GamePaths.FolderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = game.GamePaths.FolderPath,
                    UseShellExecute = true
                });
            }
        }
        
        [RelayCommand]
        private void OpenProperties(Game game)
        {
            if (game == null) return;

            var viewModel = new GamePropertiesViewModel(game, _gameService);
            var view = new GamePropertiesView
            {
                DataContext = viewModel
            };
            view.ShowDialog();
        }
        
    }
}
