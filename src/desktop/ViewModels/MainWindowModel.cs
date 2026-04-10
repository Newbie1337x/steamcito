using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using steamcito.Infrastructure.Services;
using steamcito.Services;
using steamcito.Models;
using steamcito.Models.Dtos;

namespace steamcito.ViewModels;

public partial class MainWindowModel : ObservableObject
{
    private readonly GameService _gameService;
    private readonly SteamService _steamService;

    public MainWindowModel(GameService gameService, PathManager pathManager, SteamService steamService)
    {
        _gameService = gameService;
        _steamService = steamService;
    }

    [RelayCommand]
    private void AddGame()
    {
        string folderPath;
        string exePath = "";

        
        using (var folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            folderPath = folderDialog.SelectedPath;
        }

        var dllResults = PathManager.FindDlls(folderPath);
        

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

        string gameTitle = Path.GetFileName(folderPath);
        
        var nuevoJuego = _gameService.SaveNewGame(gameTitle, folderPath, exePath);
        
        // Notificar que se agregó un juego
        WeakReferenceMessenger.Default.Send(new GameAddedMessage(nuevoJuego)); }

    [RelayCommand]
    private async Task ScanSteamGames()
    {
        await _steamService.ScanAndSaveSteamGamesAsync();
        WeakReferenceMessenger.Default.Send(new GamesReloadedMessage());
    }
}

public record GameAddedMessage(Game Game);
public record GamesReloadedMessage();