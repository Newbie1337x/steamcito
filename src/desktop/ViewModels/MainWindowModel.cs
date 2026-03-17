using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using steamcito.Services;
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
            folderDialog.Description = "Selecciona la carpeta del juego";
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderDialog.SelectedPath;
            }
            else
            {
                return;
            }
        }
        
        using (var fileDialog = new OpenFileDialog())
        {
            fileDialog.Title = "Selecciona el ejecutable del juego";
            fileDialog.Filter = "Archivos ejecutables (*.exe)|*.exe";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                exePath = fileDialog.FileName;
            }
            else
            {
                return;
            }
        }
        
        var nuevoJuego = _gameService.SaveNewGame("Nuevo Juego", folderPath, exePath);
    }
}