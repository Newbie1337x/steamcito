namespace steamcito.Models;

public class GameDetails
{

    public string SteamId { get; set; }
    public string IgdbId { get; set; }
    public List<Genre> Genres { get; set; } = new List<Genre>();
    public bool Favorite { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public TimeSpan PlayedTime { get; set; }
    public DateTime? LastTime { get; set; }
    public StoreType Store { get; set; }
    public string Platform { get; set; }
    public bool IsInstalled { get; set; }
    public DateTime AddedAt { get; set; }
    
    public GameDetails(string steamId, string igdbId, bool favorite, string title, string description, DateTime? releaseDate, TimeSpan playedTime, DateTime? lastTime, StoreType store, string platform, bool isInstalled, DateTime addedAt)
    {
        SteamId = steamId;
        IgdbId = igdbId;
        Favorite = favorite;
        Title = title;
        Description = description;
        ReleaseDate = releaseDate;
        PlayedTime = playedTime;
        LastTime = lastTime;
        Store = store;
        Platform = platform;
        IsInstalled = isInstalled;
        AddedAt = addedAt;
    }
    
    public GameDetails()
    {
        
    }
}