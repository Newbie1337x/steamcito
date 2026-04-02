using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using steamcito.Models.Dtos;

namespace steamcito.Services;

public interface ISteamApiService
{
    Task<SteamAppData?> GetAppDetailsAsync(string appId);
}

public class SteamApiService : ISteamApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _cachePath;

    public SteamApiService()
    {
        _httpClient = new HttpClient();
        //User agent if required for steam
        //_httpClient.DefaultRequestHeaders.Add("User-Agent", "Steamcito/1.0");
        
        _cachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Steamcito",
            "Cache",
            "SteamApi"
        );

        if (!Directory.Exists(_cachePath))
        {
            Directory.CreateDirectory(_cachePath);
        }
    }

    public async Task<SteamAppData?> GetAppDetailsAsync(string appId)
    {
        if (string.IsNullOrEmpty(appId)) return null;

        var cacheFile = Path.Combine(_cachePath, $"{appId}.json");
        
        if (File.Exists(cacheFile))
        {
            try
            {
                var cachedJson = await File.ReadAllTextAsync(cacheFile);
                return JsonSerializer.Deserialize<SteamAppData>(cachedJson);
            }
            catch
            {
                File.Delete(cacheFile);
                //TODO llamar de nuevo a la api o mostrar error en pantalla
            }
        }
        
        try
        {
            var s = $"https://store.steampowered.com/api/appdetails?appids={appId}";
            var url = s;
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, SteamAppDetailsResponse>>(url);

            if (response != null && response.TryGetValue(appId, out var details) && details.Success && details.Data != null)
            {
                // Save to Cache (only the Data part to save space)
                var jsonToCache = JsonSerializer.Serialize(details.Data);
                await File.WriteAllTextAsync(cacheFile, jsonToCache);

                return details.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Steam AppDetails for {appId}: {ex.Message}");
            //TODO mostrar detalle en pantalla
        }

        return null;
    }
}
