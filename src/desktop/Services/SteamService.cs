using steamcito.Models.Dtos;
using steamcito.Models;

namespace steamcito.Services;

public class SteamService
{
    private readonly PathManager _pathManager;

    public SteamService(PathManager pathManager)
    {
        _pathManager = pathManager;
    }

    public bool IsSteamGameInstalled(string path)
    {
        var result = _pathManager.FindStoreDll(path);
        return result.StoreType == StoreType.Steam && result.IsSigned;
    }
}