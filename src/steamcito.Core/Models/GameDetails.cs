using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table("gamedetails")]
public partial class GameDetails : ObservableObject
{
    [Key]
    public int Id { get; set; }

    public int GameId { get; set; }
    
    [ForeignKey("GameId")]
    public Game? Game { get; set; }
    public string? SteamId { get; set; }
    public string? IgdbId { get; set; }
    public List<Genre> Genres { get; set; } = new();
    
    [ObservableProperty]
    private bool _remotePlayTogether = false;
    
    [ObservableProperty]
    private bool _favorite = false;
    
    [ObservableProperty]
    private string? _title;
    
    [ObservableProperty]
    private string? _description;
    
    [ObservableProperty]
    private DateTime? _releaseDate;
    [ObservableProperty] 
    private TimeSpan _playTime;
    [ObservableProperty]
    private DateTime _lastTime;
    public StoreType? Store { get; set; }
    public string? Platform { get; set; }
    public bool IsInstalled { get; set; }
    public DateTime AddedAt { get; set; }
}