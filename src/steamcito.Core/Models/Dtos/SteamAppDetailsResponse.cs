using System.Text.Json.Serialization;

namespace steamcito.Models.Dtos;

public class SteamAppDetailsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public SteamAppData? Data { get; set; }
}

public class SteamAppData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("steam_appid")]
    public int SteamAppId { get; set; }

    [JsonPropertyName("required_age")]
    public string? RequiredAge { get; set; }

    [JsonPropertyName("is_free")]
    public bool IsFree { get; set; }

    [JsonPropertyName("detailed_description")]
    public string? DetailedDescription { get; set; }

    [JsonPropertyName("about_the_game")]
    public string? AboutTheGame { get; set; }

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("supported_languages")]
    public string? SupportedLanguages { get; set; }

    [JsonPropertyName("header_image")]
    public string? HeaderImage { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("developers")]
    public List<string>? Developers { get; set; }

    [JsonPropertyName("publishers")]
    public List<string>? Publishers { get; set; }

    [JsonPropertyName("categories")]
    public List<SteamAppCategory>? Categories { get; set; }

    [JsonPropertyName("genres")]
    public List<SteamAppGenre>? Genres { get; set; }

    [JsonPropertyName("screenshots")]
    public List<SteamAppScreenshot>? Screenshots { get; set; }

    [JsonPropertyName("release_date")]
    public SteamAppReleaseDate? ReleaseDate { get; set; }

    [JsonPropertyName("background")]
    public string? Background { get; set; }

    [JsonPropertyName("background_raw")]
    public string? BackgroundRaw { get; set; }
}

public class SteamAppCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class SteamAppGenre
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class SteamAppScreenshot
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("path_thumbnail")]
    public string? PathThumbnail { get; set; }

    [JsonPropertyName("path_full")]
    public string? PathFull { get; set; }
}

public class SteamAppReleaseDate
{
    [JsonPropertyName("coming_soon")]
    public bool ComingSoon { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }
}
