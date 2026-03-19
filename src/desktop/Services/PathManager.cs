using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using steamcito.Models;
using steamcito.Models.Dtos;
using File = System.IO.File;
using AuthenticodeExaminer;
namespace steamcito.Services;

public class PathManager
{
    
    
    
    

    public void CreateShortcut(string exePath, string exeName)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return;

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // name of shortcut
        string shortcutName = exeName + ".lnk";
        string shortcutPath = Path.Combine(desktopPath, shortcutName);

        //Create shortcut
        var shell = new WshShell();
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

        shortcut.TargetPath = exePath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
        shortcut.Description = "Launch game";
        shortcut.IconLocation = exePath;
        shortcut.Save();
    }

    public void OpenFolder(string path)
    {
        if (path == null)
            return;

        if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }

    public DllDetectionResults FindStoreDll(string path)
    {
        var storeDlls = new Dictionary<StoreType, string[]>
        {
            [StoreType.Steam] = new[]
            {
                "steam_api*"
            },
            [StoreType.EpicGames] = new[]
            {
                "EOSSDK-Win32-Shipping.dll",
                "EOSSDK-Win64-Shipping.dll"
            },
            [StoreType.Origin] = new[]
            {
                "uplay_r1_loader.dll",
                "uplay_r1_loader64.dll",
                "Origin.dll"
            }
        };

        foreach (var store in storeDlls)
        {
            var candidates = new List<string>();

            foreach (var dll in store.Value)
            {
                var found = Directory
                    .EnumerateFiles(path, dll, SearchOption.AllDirectories);

                candidates.AddRange(found);
            }

            foreach (var file in candidates)
            {
                if (!IsPortableExecutable(file))
                    continue;

                if (IsValidSignature(file))
                {
                    return new DllDetectionResults()
                    {
                        StoreType = store.Key,
                        FilePath = file,
                        IsSigned = true
                    };
                }
            }
            
            var fallback = candidates.FirstOrDefault();
            if (fallback != null)
            {
                return new DllDetectionResults()
                {
                    StoreType = store.Key,
                    FilePath = fallback,
                    IsSigned = false
                };
            }
        }
        return new DllDetectionResults();
    }

    private string? SearchFile(string path, string fileName)
    {
        if (!Directory.Exists(path))
            return null;

        return Directory
            .EnumerateFiles(path, fileName, SearchOption.AllDirectories)
            .FirstOrDefault();
    }


    private bool IsValidSignature(string filePath)
    {
        try
        {
            var inspector = new FileInspector(filePath);
            var result = inspector.Validate();

            if (result != SignatureCheckResult.Valid)
                return false;

            var signatures = inspector.GetSignatures();
            var signature = signatures.FirstOrDefault();

            if (signature == null)
                return false;

            var cert = signature.SigningCertificate;

            if (cert == null)
                return false;
        }
        catch
        {
            return false;
        }
        return true;
    }
    
    private bool IsPortableExecutable(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            return reader.ReadUInt16() == 0x5A4D; // "MZ"
        }
        catch
        {
            return false;
        }
    }
}