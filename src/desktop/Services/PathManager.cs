using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using steamcito.Models;
using File = System.IO.File;
using AuthenticodeExaminer;
using steamcito.Models.Dtos;
using steamcito.Models.Enum;
namespace steamcito.Services;

public class PathManager
{
    public void CreateShortcut(string exePath, string exeName)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return;

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string shortcutName = exeName + ".lnk";
        string shortcutPath = Path.Combine(desktopPath, shortcutName);

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
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }
    
    public static (StoreType? Store,List<GameDll> Dlls) FindDlls(string path)
    {
        var results = new List<GameDll>();
        StoreType? detectedStore = null;

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return (detectedStore, results);
        
        var allFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
        DllAnalizerConfig config= new DllAnalizerConfig() { CheckStore = true, CheckSignature = true };

        foreach (var filePath in allFiles)
        {
            var analysis = DllAnalizerService.AnalizeDll(filePath, config);
            if (analysis != null)
            {
                if (analysis.StoreType != StoreType.Other && detectedStore == null)
                {
                    detectedStore = analysis.StoreType;
                    config.CheckStore = false;
                }

                if (analysis.Role == DllRole.Original)
                {
                    config.CheckSignature = false;
                }

                results.Add(new GameDll()
                {
                    RelativePath = Path.GetRelativePath(path, filePath),
                    Role = analysis.Role ?? DllRole.GenericFix,
                });
            }
        }

        return (detectedStore,results);
    }

    public string? SearchFile(string path, string fileName)
    {
        if (!ExistsPath(path))
            return null;

        return Directory
            .EnumerateFiles(path, fileName, SearchOption.AllDirectories)
            .FirstOrDefault();
    }

 

    private bool ExistsPath(string path)
    {
        return Directory.Exists(path) || File.Exists(path);
    }
}