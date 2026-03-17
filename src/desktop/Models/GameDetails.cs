using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table("gamedetails")]
public class GameDetails
{
    [Key]
    public int Id { get; set; }

    public int GameId { get; set; }
    
    [ForeignKey("GameId")]
    public Game? Game { get; set; }
    public string? SteamId { get; set; }
    public string? IgdbId { get; set; }
    public List<Genre> Genres { get; set; } = new();
    public bool Favorite { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public TimeSpan PlayedTime { get; set; }
    public DateTime? LastTime { get; set; }
    public StoreType Store { get; set; }
    public string? Platform { get; set; }
    public bool IsInstalled { get; set; }
    public DateTime AddedAt { get; set; }
}