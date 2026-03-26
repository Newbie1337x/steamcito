using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using steamcito.Models;
using steamcito.Models.Enum;

namespace steamcito.Services;

public class SteamService
{
    private readonly string? _steamPath;
    private readonly GameService _gameService;

    public SteamService(GameService gameService)
    {
        _steamPath = GetSteamPathFromRegistry();
        _gameService = gameService;
    }

    public string? GetSteamPathFromRegistry()
    {
        using var keyCU = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        var path = keyCU?.GetValue("SteamPath") as string;

        if (!string.IsNullOrWhiteSpace(path))
            return path;

        using var keyLM = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
        return keyLM?.GetValue("InstallPath") as string;
    }

    public bool IsSteamInstalled()
    {
        if (string.IsNullOrWhiteSpace(_steamPath))
            return false;

        return File.Exists(Path.Combine(_steamPath, "steam.exe"));
    }

    public List<string> GetSteamLibraries()
    {
        var libraries = new List<string>();

        if (string.IsNullOrEmpty(_steamPath))
            return libraries;

        libraries.Add(_steamPath);

        var vdfPath = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(vdfPath))
            return libraries;

        var lines = File.ReadAllLines(vdfPath);

        foreach (var line in lines)
        {
            try
            {
                if (line.Contains("\"path\"") || line.Contains(":\\")) 
                {
                    var path = ExtractValue(line);

                    if (Directory.Exists(path) && !libraries.Contains(path))
                        libraries.Add(path);
                }
            }
            catch { }
        }

        return libraries;
    }

    public void ScanAndSaveSteamGames()
    {
        if (!IsSteamInstalled())
            return;

        var libraries = GetSteamLibraries();

        foreach (var library in libraries)
        {
            var steamAppsPath = Path.Combine(library, "steamapps");

            if (!Directory.Exists(steamAppsPath))
                continue;

            var manifests = Directory.EnumerateFiles(
                steamAppsPath,
                "appmanifest_*.acf",
                SearchOption.TopDirectoryOnly
            );

            foreach (var manifest in manifests)
            {
                try
                {
                    var game = GetGameFromManifest(manifest, library);

                    if (game != null)
                    {
                        _gameService.Upsert(game);
                    }
                }
                catch { }
            }
        }
    }

    private Game? GetGameFromManifest(string manifestPath, string libraryPath)
    {
        var lines = File.ReadAllLines(manifestPath);

        string? name = null;
        string? appId = null;
        string? installDir = null;

        foreach (var line in lines)
        {
            if (line.Contains("\"appid\""))
            {
                appId = ExtractValue(line);

                if (appId == "228980")
                    return null;
            }
            else if (line.Contains("\"name\""))
            {
                name = ExtractValue(line);
            }
            else if (line.Contains("\"installdir\""))
            {
                installDir = ExtractValue(line);
            }
        }

        if (string.IsNullOrEmpty(appId))
            return null;

        var game = new Game
        {
            LaunchMode = LaunchMode.Store,

            Details = new GameDetails
            {
                Title = name ?? "Unknown",
                SteamId = appId,
                AddedAt = DateTime.Now,
                IsInstalled = true,
                Store = StoreType.Steam,
            }
        };

        string gamePath = Path.Combine(
            libraryPath,
            "steamapps",
            "common",
            installDir ?? ""
        );
        
        var gamePaths = new GamePaths
        {
            Game = game,
            FolderPath = gamePath,
            ExeRelativePath = null,
            Dlls = PathManager.FindDlls(gamePath).Dlls
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