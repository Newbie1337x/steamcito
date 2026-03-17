using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using steamcito.Services;
using System.Windows.Forms;
namespace steamcito.ViewModels;

public partial class MainWindowModel : ObservableObject
{
    private readonly GameService _gameService;

    public MainWindowModel()
    {
        _gameService = new GameService();
    }

    [RelayCommand]
    private void AddGame()
    {
        string folderPath = "";
        string exePath = "";

        
        using (var folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            folderPath = folderDialog.SelectedPath;
        }

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

        var nuevoJuego = _gameService.SaveNewGame("Nuevo Juego", folderPath, exePath);
    }
}