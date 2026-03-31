using steamcito.Models.Enum;

namespace steamcito.Models.Dtos;

public class DllAnalizerResult
{
    public string?      FilePath     { get; set; }
    public StoreType?   StoreType    { get; set; }
    public string?      RelativePath { get; set; }
    public DllRole?     Role         { get; set; }
    public bool         IsSigned     { get; set; }
    public List<string> ConfigFiles  { get; set; } = [];
}