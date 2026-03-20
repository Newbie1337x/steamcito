using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using steamcito.Models;
using steamcito.Models.Enum;

namespace steamcito.Services;

public class SteamService
{
    private readonly string? _steamPath;

    public SteamService()
    {
        _steamPath = GetSteamPathFromRegistry();
    }

    public string? GetSteamPathFromRegistry()
    {
        // HKCU 
        using var keyCU = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        var path = keyCU?.GetValue("SteamPath") as string;

        if (!string.IsNullOrWhiteSpace(path))
            return path;
        
       // HKLM
        using var keyLM = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
        return keyLM?.GetValue("InstallPath") as string;
    }

    public bool IsSteamInstalled()
    {
        if (string.IsNullOrWhiteSpace(_steamPath))
            return false;

        return File.Exists(Path.Combine(_steamPath, "steam.exe"));
    }
    
    public Game[] GetAllSteamGames()
    {
        if (!IsSteamInstalled())
            return Array.Empty<Game>();

        var steamAppsPath = Path.Combine(_steamPath!, "steamapps");

        if (!Directory.Exists(steamAppsPath))
            return Array.Empty<Game>();

        var manifests = Directory.EnumerateFiles(
            steamAppsPath,
            "appmanifest_*.acf",
            SearchOption.TopDirectoryOnly
        );

        var games = new List<Game>();

        foreach (var manifest in manifests)
        {
            try
            {
                var game = GetGameFromManifest(manifest);
                
                games.Add(game);
                Debug.WriteLine($"Found game: {game.ToString()}");
            }
            catch
            {
            }
        }

        return games.ToArray();
    }
    
    private Game GetGameFromManifest(string manifestPath)
    {
        var lines = File.ReadAllLines(manifestPath);

        string? name = null;
        string? appId = null;
        string? installDir = null;

        foreach (var line in lines)
        {
            if (line.Contains("\"appid\""))
                appId = ExtractValue(line);

            else if (line.Contains("\"name\""))
                name = ExtractValue(line);

            else if (line.Contains("\"installdir\""))
                installDir = ExtractValue(line);
        }

        var game = new Game
        {
            LaunchMode = LaunchMode.Launcher,
            
            Details = new GameDetails
            {
                Title = name ?? "Unknown",
                SteamId = appId,
                AddedAt = DateTime.Now,
                IsInstalled = true,
                Store = StoreType.Steam,
                IsSigned = true,
            }
        };

        var gamePaths = new GamePaths
        {
            Game = game,
            FolderPath = Path.Combine(
                _steamPath!,
                "steamapps",
                "common",
                installDir ?? ""
            ),
            ExePath = $"steam://rungameid/{appId}",
            DllPath = "" // opcional
        };

        var artwork = new Artwork
        {
            Game = game,
            Grid = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_600x900_2x.jpg",
            Hero = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_hero.jpg",
            Logo = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/logo.png",
            Icon = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg"
        };
        
        game.GamePaths = gamePaths;
        game.Artworks = artwork;

        return game;
    }
    
    private string ExtractValue(string line)
    {
        var parts = line.Split('"', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 4 ? parts[3] : string.Empty;
    }
}