using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using steamcito.Models;
using steamcito.Models.Enum;
using steamcito.Services;
using steamcito.Core.Interfaces;

namespace steamcito.Infrastructure.Services;

public class SteamService
{
    private readonly string? _steamPath;
    private readonly GameService _gameService;
    private ISteamApiService _steamApiService;

    public SteamService(GameService gameService, ISteamApiService steamApiService)
    {
        _steamPath = GetSteamPathFromRegistry();
        _gameService = gameService;
        _steamApiService = steamApiService;
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

    public async Task ScanAndSaveSteamGamesAsync()
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
                    var game = await GetGameFromManifestAsync(manifest, library);

                    if (game != null)
                    {
                        _gameService.Upsert(game);
                    }
                }
                catch { }
            }
        }
    }

    public async Task UpdateSteamGameDetailsAsync(Game game)
    {
        if (string.IsNullOrEmpty(game.Details.SteamId)) return;

        var apiData = await _steamApiService.GetAppDetailsAsync(game.Details.SteamId);
        if (apiData != null)
        {
            game.Details.Title = apiData.Name ?? game.Details.Title;
            game.Details.Description = apiData.ShortDescription ?? apiData.DetailedDescription;
            game.Details.RemotePlayTogether = apiData.Categories?.Any(c => c.Id is 44 or 24 or 39 or 37) != null;

            if (apiData.ReleaseDate != null && !string.IsNullOrEmpty(apiData.ReleaseDate.Date))
            {
                if (DateTime.TryParse((string)apiData.ReleaseDate.Date, out var releaseDate))
                {
                    game.Details.ReleaseDate = releaseDate;
                }
            }

            if (apiData.Genres != null)
            {
                game.Details.Genres = apiData.Genres.Select(g => new Genre(g.Description ?? "Unknown")).ToList();
            }
            // Artworks caché logic
            bool hasCachedArtworks = CheckArtworksCache(game.Details.SteamId);

            if (!hasCachedArtworks)
            {
                await DownloadAndCacheArtworks(game.Details.SteamId, game.Artworks);
            }
            else
            {
                UpdateArtworksFromCache(game.Details.SteamId, game.Artworks);
            }

            _gameService.Update(game);
        }
    }

    private bool CheckArtworksCache(string appId)
    {
        var cachePath = GetArtworksCachePath(appId);
        return Directory.Exists(cachePath) && Directory.GetFiles(cachePath).Length > 0;
    }

    private string GetArtworksCachePath(string appId)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Steamcito",
            "Cache",
            "Artworks",
            appId
        );
    }

    private async Task DownloadAndCacheArtworks(string appId, Artwork artworks)
    {
        var cachePath = GetArtworksCachePath(appId);
        if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);

        using var client = new HttpClient();

        var files = new Dictionary<string, string>
        {
            { "grid", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_600x900_2x.jpg" },
            { "hero", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/library_hero.jpg" },
            { "logo", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/logo.png" },
            { "icon", $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg" }
        };

        foreach (var file in files)
        {
            try
            {
                var extension = Path.GetExtension(file.Value).Split('?')[0];
                if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                var localPath = Path.Combine(cachePath, $"{file.Key}{extension}");

                var data = await client.GetByteArrayAsync(file.Value);
                await File.WriteAllBytesAsync(localPath, data);

                switch (file.Key)
                {
                    case "grid": artworks.Grid = localPath; break;
                    case "hero": artworks.Hero = localPath; break;
                    case "logo": artworks.Logo = localPath; break;
                    case "icon": artworks.Icon = localPath; break;
                }
            }
            catch { /* Ignorar errores de descarga individual */ }
        }
    }

    private void UpdateArtworksFromCache(string appId, Artwork artworks)
    {
        var cachePath = GetArtworksCachePath(appId);
        if (!Directory.Exists(cachePath)) return;

        var files = Directory.GetFiles(cachePath);
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file).ToLower();
            switch (fileName)
            {
                case "grid": artworks.Grid = file; break;
                case "hero": artworks.Hero = file; break;
                case "logo": artworks.Logo = file; break;
                case "icon": artworks.Icon = file; break;
            }
        }
    }

    private async Task<Game?> GetGameFromManifestAsync(string manifestPath, string libraryPath)
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