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
        if (game.GamePaths != null) game.GamePaths.Game = game;
        if (game.Details != null) game.Details.Game = game;
        if (game.Artworks != null) game.Artworks.Game = game;

        _context.Games.Add(game);
        _context.SaveChanges();
    }

    public Game? GetBySteamId(string steamId) => _context.Games
        .Include(g => g.Details)
        .Include(g => g.GamePaths)
        .ThenInclude(gp => gp.Dlls)
        .Include(g => g.Artworks)
        .FirstOrDefault(g => g.Details.SteamId == steamId);
    
    public void Upsert(Game game)
    {
        if (game.Details?.SteamId == null) return;
        
        var existing = GetBySteamId(game.Details.SteamId);

        if (existing != null)
        {
            existing.Details.Title = game.Details.Title;
            if (existing.GamePaths != null && game.GamePaths != null)
            {
                existing.GamePaths.FolderPath = game.GamePaths.FolderPath;

                // Clear existing Dlls to avoid duplication on rescan
                existing.GamePaths.Dlls ??= new List<GameDll>();
                existing.GamePaths.Dlls.Clear();
                if (game.GamePaths.Dlls != null)
                {
                    foreach (var dll in game.GamePaths.Dlls)
                    {
                        existing.GamePaths.Dlls.Add(new GameDll
                        {
                            RelativePath = dll.RelativePath,
                            Role = dll.Role,
                            GamePathsId = existing.GamePaths.Id
                        });
                    }
                }
            }

            if (existing.Artworks != null && game.Artworks != null)
            {
                existing.Artworks.Grid = game.Artworks.Grid;
                existing.Artworks.Hero = game.Artworks.Hero;
                existing.Artworks.Logo = game.Artworks.Logo;
                existing.Artworks.Icon = game.Artworks.Icon;
            }

            _context.Games.Update(existing);

            Console.WriteLine($"[DB] Updated: {existing.Details.Title}");
        }
        else
        {
            _context.Games.Add(game);

            Console.WriteLine($"[DB] Inserted: {game.Details.Title}");
        }

        _context.SaveChanges();
    }

    public List<Game> GetAll() => _context.Games
        .Include(g => g.Details)
        .Include(g => g.GamePaths)
        .Include(g => g.Artworks)
        .ToList();
    public Game? GetById(string id) => _context.Games.Find(id);
    
    public bool existBySteamId(string steamId) => _context.Games.Any(g => g.Details.SteamId == steamId);
    
    
    public Game SaveNewGame(string title, string folderPath, string exePath)
    {
        var dllResults = PathManager.FindDlls(folderPath);
       
        var game = new Game
        {
            Details = new GameDetails
            {
                Title = title,
                AddedAt = DateTime.Now,
                IsInstalled = true,
                Store = dllResults.Store ?? StoreType.Other,
                
            }
        };
        
        var gamePaths = new GamePaths
        {
            GameId = game.Id,
            Game = game,
            FolderPath = folderPath,
            ExeRelativePath = exePath,
            Dlls = dllResults.Dlls,
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