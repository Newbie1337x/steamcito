using System.ComponentModel.DataAnnotations.Schema;
using steamcito.Models.Enum;

namespace steamcito.Models;
[Table( "settings")]
public class Setting
{
    public string Id { get; set; }
    public required string UserId { get; set; }
    [ForeignKey("UserId")]
    public required User User { get; set; }
    public required Language Language { get; set; }
    
}