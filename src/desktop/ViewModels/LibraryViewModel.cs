using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Services;
using steamcito.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using steamcito.Views;
using steamcito.Core.Interfaces;
using Application = System.Windows.Application;

namespace steamcito.ViewModels
{
    public partial class LibraryViewModel : ObservableObject, IRecipient<GameAddedMessage>, IRecipient<GamesReloadedMessage>
    {
        private readonly GameService _gameService;
        private readonly GameSessionManager _gameSessionManager;
        private readonly PathManager _pathManager;
        private readonly IEightBitFiestaService _eightBitFiestaService;
        
        [ObservableProperty] private ObservableCollection<Game> _games = new();
        private Game? _selectedGame;
        public Game? SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    OnSelectedGameChanged(value);
                }
            }
        }

        private async void OnSelectedGameChanged(Game? game)
        {
            if (game?.Details?.SteamId != null)
            {
                if (string.IsNullOrEmpty(game.Details.Description))
                {
                    await UpdateSteamGameDetailsAsync(game);
                }
            }
        }

        private async Task UpdateSteamGameDetailsAsync(Game game)
        {
            if (_steamApiService == null || string.IsNullOrEmpty(game.Details.SteamId)) return;

            var apiData = await _steamApiService.GetAppDetailsAsync(game.Details.SteamId);
            if (apiData != null)
            {
                game.Details.Title = apiData.Name ?? game.Details.Title;
                game.Details.Description = apiData.ShortDescription?? apiData.DetailedDescription;
                game.Details.RemotePlayTogether = (apiData.Categories?.Find(c => c.Description == "Remote Play Together" || c.Description == "Pantalla partida/compartida") != null);
                
                if (apiData.ReleaseDate != null && !string.IsNullOrEmpty(apiData.ReleaseDate.Date))
                {
                    if (DateTime.TryParse(apiData.ReleaseDate.Date, out var releaseDate))
                    {
                        game.Details.ReleaseDate = releaseDate;
                    }
                }

                if (apiData.Genres != null)
                {
                    game.Details.Genres = apiData.Genres.Select(g => new Genre(g.Description ?? "Unknown")).ToList();
                }
                
                game.Artworks.Hero = apiData.Background ?? game.Artworks.Hero;

                _gameService.Update(game);
                
                OnPropertyChanged(nameof(SelectedGame));
            }
        }

        private readonly ISteamApiService? _steamApiService;
        
        public LibraryViewModel(GameService gameService, GameSessionManager gameSessionManager, PathManager pathManager, IEightBitFiestaService eightBitFiestaService, ISteamApiService steamApiService)
        {
            _gameService = gameService;
            _gameSessionManager = gameSessionManager;
            _pathManager = pathManager;
            _eightBitFiestaService = eightBitFiestaService;
            _steamApiService = steamApiService;
            LoadGames();
            WeakReferenceMessenger.Default.Register<GameAddedMessage>(this);
            WeakReferenceMessenger.Default.Register<GamesReloadedMessage>(this);

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

        public void Receive(GamesReloadedMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadGames();
            });
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
            if (game.GamePaths == null || (string.IsNullOrEmpty(game.GamePaths.ExeRelativePath) && string.IsNullOrEmpty(game.Details.SteamId)))
                return;

            if (!game.IsRunning)
            {
                _gameSessionManager.LaunchGame(game);
            }
            else
            {
                _gameSessionManager.StopGame();
            }
        }
        
        [RelayCommand]
        private void CreateShortcut(Game game)
        {
            if (game.GamePaths?.ExeRelativePath == null )
                return;
            
           _pathManager.CreateShortcut(game.GamePaths.ExeFullPath, game.Details.Title);
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
        private  void OpenFolder(string folderPath)
        {
          _pathManager.OpenFolder(folderPath);
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
        
        [RelayCommand]
        private async Task SetupEightBitFiesta(Game game)
        {
            if (game == null) return;
            await _eightBitFiestaService.SetupAsync(game);
        }
        
    }
}
