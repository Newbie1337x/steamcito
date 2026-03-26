using System.IO;
using AuthenticodeExaminer;
using steamcito.Helpers;
using steamcito.Models;
using steamcito.Models.Dtos;
using steamcito.Models.Enum;

namespace steamcito.Services;

public class DllAnalizerService
{

    public static DllAnalizerResult? AnalizeDll(string filePath, DllAnalizerConfig config)
    {
        string fileName = Path.GetFileName(filePath);
        if (!IsTargetDll(fileName))
            return null;

        if (!IsPortableExecutable(filePath))
            return null;

        bool isSigned = false;
        StoreType storeType = StoreType.Other;

        if (config.CheckStore)
        {
            storeType = GetStoreType(filePath);
        }

        if (config.CheckSignature || storeType != StoreType.Other)
        {
            isSigned = IsValidSignature(filePath);
        }

        return new DllAnalizerResult
        {
            StoreType = storeType,
            Role = isSigned ? DllRole.Original : DllRole.GenericFix
        };
    }

    private static bool IsPortableExecutable(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new BinaryReader(stream);

            return reader.ReadUInt16() == 0x5A4D; // "MZ"
        }
        catch
        {
            return false;
        }
    }


    private static bool IsTargetDll(string fileName)
    {
        return fileName.Contains("steam_api", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("EOSSDK", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("uplay_r1_loader", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("Origin.dll", StringComparison.OrdinalIgnoreCase);
    }

    private static StoreType GetStoreType(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        if (fileName.Contains("steam_api", StringComparison.OrdinalIgnoreCase))
            return StoreType.Steam;

        if (fileName.StartsWith("EOSSDK", StringComparison.OrdinalIgnoreCase))
            return StoreType.EpicGames;

        if (fileName.StartsWith("uplay_r1_loader", StringComparison.OrdinalIgnoreCase))
            return StoreType.Uplay;

        if (fileName.StartsWith("Origin.dll", StringComparison.OrdinalIgnoreCase))
            return StoreType.Origin;

        return StoreType.Other;
    }

    private static bool IsValidSignature(string filePath)
    {
        try
        {
            return SignatureHelper.IsWindowsSignatureValid(filePath);
        }
        catch
        {
            return false;
        }
    }
}