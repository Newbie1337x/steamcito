using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace steamcito.Models;

[Table( "users")]
public class User
{
    [Key]
    public string Id { get; set; }
    [MaxLength(50)]
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public string? SteamId { get; set;}
    public Setting? Setting { get; set; }
    
}