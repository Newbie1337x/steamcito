using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Services;
using steamcito.Models;
using steamcito.Models.Dtos;

namespace steamcito.ViewModels;

public partial class MainWindowModel : ObservableObject
{
    private readonly GameService _gameService;
    private readonly PathManager _pathManager;
    private readonly SteamService _steamService;

    public MainWindowModel(GameService gameService, PathManager pathManager, SteamService steamService)
    {
        _gameService = gameService;
        _pathManager = pathManager;
        _steamService = steamService;
    }

    [RelayCommand]
    private void AddGame()
    {
        string folderPath;
        string exePath = "";
        DllDetectionResults? dllResults;

        
        using (var folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            folderPath = folderDialog.SelectedPath;
        }

        dllResults = _pathManager.FindStoreDll(folderPath);
        

        Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog()
        {
            Title = "Select game .exe",
            Filter = "(*.exe)|*.exe",
            InitialDirectory = folderPath
        };

        if (fileDialog.ShowDialog() != true)
        {
            return;
        }
        exePath = fileDialog.FileName;

        if (string.IsNullOrEmpty(exePath) || string.IsNullOrEmpty(folderPath))
        {
            return;
        }

        string gameTitle = System.IO.Path.GetFileNameWithoutExtension(exePath);
        
        var nuevoJuego = _gameService.SaveNewGame(gameTitle, folderPath, exePath,dllResults);
        
        // Notificar que se agregó un juego
        WeakReferenceMessenger.Default.Send(new GameAddedMessage(nuevoJuego));
    }

    [RelayCommand]
    private void ScanSteamGames()
    {
        var steamGames = _steamService.GetAllSteamGames();

        foreach (var steamGame in steamGames)
        {
            Debug.WriteLine($"Steam gameS found:"+ steamGames.Length);
            // Guardar el juego
            _gameService.SaveGame(steamGame);
            
            // Notificar que se agregó un juego
            WeakReferenceMessenger.Default.Send(new GameAddedMessage(steamGame));
        }
    }
}

public record GameAddedMessage(Game Game);