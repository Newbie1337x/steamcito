using steamcito.Models;
using steamcito.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using Microsoft.EntityFrameworkCore;
using steamcito.Models.Dtos;
using File = System.IO.File;

namespace steamcito.Services;

public class GameService
{
    private readonly AppDBContext _context;
    
    public GameService(AppDBContext context)
    {
        _context = context;
    }

    public void RemoveGame(Game game)
    {
        _context.Games.Remove(game);
        _context.SaveChanges();
    }

    public void SaveGame(Game game)
    {
        _context.Games.Add(game);
        _context.SaveChanges();
    }


    public List<Game> GetAll() => _context.Games
        .Include(g => g.Details)
        .Include(g => g.GamePaths)
        .Include(g => g.Artworks)
        .ToList();
    public Game? GetById(string id) => _context.Games.Find(id);
    
    public Game SaveNewGame(string title, string folderPath, string exePath, DllDetectionResults dllResults)
    {
       
        var game = new Game
        {
            Details = new GameDetails
            {
                Title = title,
                AddedAt = DateTime.Now,
                IsInstalled = true,
                Store = dllResults.StoreType,
                IsSigned = dllResults.IsSigned,
                
            }
        };
        
        var gamePaths = new GamePaths
        {
            GameId = game.Id,
            Game = game,
            FolderPath = folderPath,
            ExePath = exePath,
            DllPath = dllResults.FilePath,
        };
        
        var artwork = new Artwork
        {
            GameId = game.Id,
            Game = game
        };
        
        game.GamePaths = gamePaths;
        game.Artworks = artwork;
        
        _context.Games.Add(game);
        _context.SaveChanges();
        return game;
    }
    
    public void Update(Game game)
    {
        _context.Games.Update(game);
        _context.SaveChanges();
    }
}