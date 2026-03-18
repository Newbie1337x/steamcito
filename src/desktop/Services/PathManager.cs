using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using IWshRuntimeLibrary;
using steamcito.Models;
using steamcito.Models.Dtos;
using File = System.IO.File;
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
                "steam_api.dll",
                "steam_api64.dll"
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
            foreach (var dll in store.Value)
            {
                var result = SearchFile(path, dll);
                if (result != null)
                {
                    return new DllDetectionResults()
                    {
                        StoreType = store.Key,
                        FilePath = result,
                        IsSigned = IsValidSignature(result),
                    };
                }
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
            var cert = new X509Certificate2(X509Certificate.CreateFromCertFile(filePath));

            return cert.Verify();
        }
        catch
        {
            return false;
        }
    }
}