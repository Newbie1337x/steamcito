using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table( "gamepaths")]
public class GamePaths
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public required Game Game { get; set; }
    public required string ExePath { get; set; }
    public string? LauncherPath { get; set; }
    public required string FolderPath { get; set; }
    public string? DllPath { get; set; }
    public string? Arguments { get; set; }
    
}