using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Models;
using steamcito.Services;

namespace steamcito.ViewModels;

public partial class GamePropertiesViewModel : ObservableObject
{
    private readonly GameService _gameService;
    [ObservableProperty] private Game? _selectedGame;
 
    
    public GamePropertiesViewModel(Game game, GameService gameService)
    {
        _gameService = gameService;
        _selectedGame = game;
        
    }
}