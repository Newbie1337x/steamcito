using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table("gamedllconfigs")]
public class GameDllConfig
{
    public int    Id           { get; set; }
    public string FileName     { get; set; } = null!;
    public string RelativePath { get; set; } = null!;

    public int     GameDllId { get; set; }
    [ForeignKey("GameDllId")]
    public GameDll? GameDll  { get; set; }
}
