using steamcito.Models.Enum;

namespace steamcito.Models.Dtos;

public class DllAnalizerResult
{
    public StoreType? StoreType { get; set; }
    public string? RelativePath { get; set; }
    public DllRole? Role { get; set; }
    
    
    
    
}