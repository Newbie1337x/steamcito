using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;
[Table ("games")]
public class Game
{
    [Key]
    public int Id { get; set; }
    public GameDetails Details { get; set; } = new();

    public GamePaths? GamePaths { get; set; }
    public Artwork Artworks { get; set; } = new();
}