using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace steamcito.Models;

[Table( "gamepaths")]
public class GamePaths
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game? Game { get; set; }
    public string? ExeRelativePath { get; set; }
    public string? LauncherRelativePath { get; set; }
    public required string FolderPath { get; set; }

    public List<GameDll>? Dlls { get; set; } = new();
    
    public string? Arguments { get; set; }
    
    [NotMapped]
    public string? ExeFullPath => 
        ExeRelativePath == null ? null : Path.Combine(FolderPath, ExeRelativePath);
    
    [NotMapped]
    public string? LauncherFullPath => 
        LauncherRelativePath == null ? null : Path.Combine(FolderPath, LauncherRelativePath);
}