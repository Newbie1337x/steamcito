using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table( "game")]
public class Game
{
    [Key]
    public string Id { get; set; }
    public required string Name { get; set; }
    
}