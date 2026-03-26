using steamcito.Core.Interfaces;
using steamcito.Models;

namespace steamcito.Integrations.EightBitFiesta;

public class EightBitFiestaService : IEightBitFiestaService
{
    // private readonly IDownloadService _downloadService;
    // private readonly IPathService _pathService;

    public async Task SetupAsync(Game game)
    {
        // 1. Descargar 8BitFiesta
        // 2. Copiar archivos al juego
        // 3. Configurar
        await Task.CompletedTask;
    }
}
