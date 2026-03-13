using steamcito.Models;
using steamcito.Data;
namespace steamcito.Services;

public class GameService
{
    
    public Game SaveNewGame(string title, string folderPath, string exePath)
    {
        using var db = new AppDBContext();
        var nuevoJuego = new Game
        {
            Details = new GameDetails
            {
                Title = title,
                AddedAt = DateTime.Now,
                IsInstalled = true
            }
        };

        db.Games.Add(nuevoJuego);
        db.SaveChanges();
        
        var gamePaths = new GamePaths
        {
            GameId = nuevoJuego.Id,
            Game = nuevoJuego,
            FolderPath = folderPath,
            ExePath = exePath
        };
        
        nuevoJuego.GamePaths = gamePaths;
        db.GamePaths.Add(gamePaths);
        db.SaveChanges();
        return nuevoJuego;
    }
}