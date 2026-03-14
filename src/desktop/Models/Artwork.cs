using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table("artworks")]
public class Artwork
{
    [Key]
    public int Id { get; set; }
    
    public int GameId { get; set; }
    
    [ForeignKey("GameId")]
    public Game? Game { get; set; }
    
    public string? Hero { get; set; }
    public string? Grid { get; set; }
    public string? Logo { get; set; }
    public string? Icon { get; set; }
}