using steamcito.Models;
using steamcito.Data;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
namespace steamcito.Services;

public class GameService
{
    private readonly AppDBContext _context;
    
    public GameService(AppDBContext context)
    {
        _context = context;
    }
    public void runGame(string exePath)
    {
        // TODO implement run game
        System.Diagnostics.Process.Start(exePath);
    }

    public List<Game> GetAll() => _context.Games
        .Include(g => g.Details)
        .Include(g => g.GamePaths)
        .Include(g => g.Artworks)
        .ToList();
    public Game? GetById(string id) => _context.Games.Find(id);
    
    public Game SaveNewGame(string title, string folderPath, string exePath)
    {
       
        var nuevoJuego = new Game
        {
            Details = new GameDetails
            {
                Title = title,
                AddedAt = DateTime.Now,
                IsInstalled = true
            }
        };
        
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
        
        _context.Games.Add(nuevoJuego);
        _context.SaveChanges();
        return nuevoJuego;
    }
}