namespace steamcito.Models.Dtos;

public class DllDetectionResults
{
    public StoreType StoreType { get; set; }
    public bool IsSigned { get; set; }
    public string FilePath { get; set; }
    
    public bool IsRenamed { get; set; }
    
    public string injectDllPath { get; set; }
    
    
    
}