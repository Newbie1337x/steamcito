using steamcito.Models;
using steamcito.Data;
using System.Collections.ObjectModel;
namespace steamcito.Services;

public class GameService
{
    public void runGame(string exePath)
    {
        // TODO implement run game
        System.Diagnostics.Process.Start(exePath);
    }

    public ObservableCollection<Game> GetAllGames()
    {
        using var db = new AppDBContext();
        var games = db.Games.ToList();
        return new ObservableCollection<Game>(games);
    }

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
        
        var gamePaths = new GamePaths
        {
            GameId = nuevoJuego.Id,
            Game = nuevoJuego,
            FolderPath = folderPath,
            ExePath = exePath
        };
        
        var artwork = new Artwork
        {
            GameId = nuevoJuego.Id,
            Game = nuevoJuego
        };
        
        nuevoJuego.GamePaths = gamePaths;
        nuevoJuego.Artworks = artwork;
        
        db.GamePaths.Add(gamePaths);
        db.Artworks.Add(artwork);
        db.SaveChanges();
        return nuevoJuego;
    }
}