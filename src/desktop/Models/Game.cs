using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using steamcito.Models.Enum;

namespace steamcito.Models;
[Table ("games")]
public partial class Game : ObservableObject
{

    [Key]
    public int Id { get; set; }
    
    [NotMapped]
    [ObservableProperty]
    private bool _isRunning;
    
    public GameDetails Details { get; set; } = new();

    public GamePaths? GamePaths { get; set; }
    public Artwork Artworks { get; set; } = new();
}