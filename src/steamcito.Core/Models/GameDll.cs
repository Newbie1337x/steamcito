using System.ComponentModel.DataAnnotations.Schema;
using steamcito.Models;
using steamcito.Models.Enum;

[Table("gamedlls")]
public class GameDll
{
    public int Id { get; set; }
    public string RelativePath { get; set; } = null!;
    public DllRole Role { get; set; }

    public int GamePathsId { get; set; }
    
    [ForeignKey("GamePathsId")]
    public GamePaths? GamePaths { get; set; }

    public List<GameDllConfig> Configs { get; set; } = [];
}