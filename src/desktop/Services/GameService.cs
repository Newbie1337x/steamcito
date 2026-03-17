using steamcito.Models;
using steamcito.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using Microsoft.EntityFrameworkCore;
using File = System.IO.File;

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
        // TODO implement detection of game running
        
        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(exePath)
        });
        //Aplicar para juegos de steam
       /* Process.Start(new ProcessStartInfo
        {
            FileName = "steam://rungameid/480",
            UseShellExecute = true
        });
    */
    }

    public void RemoveGame(Game game)
    {
        _context.Games.Remove(game);
        _context.SaveChanges();
    }

    public void CreateShortcut(GamePaths gamePaths, string name)
    {
        var exePath = gamePaths.ExePath;
        
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return;
        
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // name of shortcut
        string shortcutName = name + ".lnk";
        string shortcutPath = Path.Combine(desktopPath, shortcutName);
        
        //Create shortcut
        var shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

        shortcut.TargetPath = exePath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
        shortcut.Description = "Launch game";
        shortcut.IconLocation = exePath;

        shortcut.Save();
    }

    public List<Game> GetAll() => _context.Games
        .Include(g => g.Details)
        .Include(g => g.GamePaths)
        .Include(g => g.Artworks)
        .ToList();
    public Game? GetById(string id) => _context.Games.Find(id);
    
    public Game SaveNewGame(string title, string folderPath, string exePath)
    {
       
        var game = new Game
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
            GameId = game.Id,
            Game = game,
            FolderPath = folderPath,
            ExePath = exePath
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