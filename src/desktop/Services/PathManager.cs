using System.IO;
using IWshRuntimeLibrary;
using steamcito.Models;
using File = System.IO.File;

namespace steamcito.Services;

public class PathManager
{
    public PathManager()
    {
    }
    public void CreateShortcut(GamePaths gamePaths, string name)
    {
        var exePath = gamePaths.ExePath;
        
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return;
        
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // name of shortcut
        string shortcutName = name + ".lnk";
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
}