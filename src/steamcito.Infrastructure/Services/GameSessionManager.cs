using System.Diagnostics;
using System.IO;
using steamcito.Models;
using steamcito.Models.Enum;
using Timer = System.Timers.Timer;

namespace steamcito.Services;

public class GameSessionManager
{
    private List<Process> _trackedProcesses = new();
    private Timer? _detectionTimer;
    private DateTime _startTime;
    private Game? _currentGame;
    private string? _gameFolder;

    public event Action<Game>? OnGameLaunching;
    public event Action<Game>? OnGameStarted;
    public event Action<Game, TimeSpan>? OnGameClosed;

    public void LaunchGame(Game game)
    {
        if (game.IsRunning) return;

        _currentGame = game;
        _gameFolder = game.GamePaths.FolderPath;

        var launchTime = DateTime.Now.AddSeconds(-1);
        _startTime = DateTime.Now;

        OnGameLaunching?.Invoke(_currentGame);

        try
        {
            if (game.Details.Store == StoreType.Steam && !string.IsNullOrEmpty(game.Details.SteamId))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"steam://rungameid/{game.Details.SteamId}",
                    UseShellExecute = true
                });

                StartDetection(launchTime);
            }
            else if (!string.IsNullOrEmpty(game.GamePaths.ExeRelativePath))
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = game.GamePaths.ExeFullPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(game.GamePaths.ExeFullPath)
                });

                if (process != null)
                {
                    AttachToProcesses(new List<Process> { process });
                }
                else
                {
                    StartDetection(launchTime);
                }
            }
            else
            {
                StartDetection(launchTime);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error launching game: {ex.Message}");
            StopGame();
        }
    }

    public void StopGame()
    {
        if (_currentGame == null) return;

        try
        {
            if (_currentGame.Details.Store != StoreType.Steam)
            {
                foreach (var p in _trackedProcesses)
                {
                    try
                    {
                        if (!p.HasExited)
                            p.Kill();
                    }
                    catch { }
                }
            }

            Cleanup();

            _currentGame.IsRunning = false;
            OnGameClosed?.Invoke(_currentGame, TimeSpan.Zero);
            _currentGame = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping game: {ex.Message}");
        }
    }

    private void StartDetection(DateTime launchTime)
    {
        _detectionTimer = new Timer(1500);
        int attempts = 0;

        _detectionTimer.Elapsed += (s, e) =>
        {
            try
            {
                attempts++;

                var all = Process.GetProcesses();
                var byFolder = all.Where(p =>
                {
                    try
                    {
                        if (p.HasExited) return false;
                        if (p.StartTime < launchTime) return false;

                        var path = p.MainModule?.FileName;
                        if (path == null) return false;

                        return _gameFolder != null &&
                               path.IndexOf(_gameFolder, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();

                if (byFolder.Any())
                {
                    Debug.WriteLine($"[DETECT] Folder match: {byFolder.Count}");
                    AttachToProcesses(byFolder);
                    return;
                }
                
                var byTime = all.Where(p =>
                {
                    try
                    {
                        if (p.HasExited) return false;
                        return p.StartTime >= launchTime;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .OrderByDescending(p => p.StartTime)
                .Take(5)
                .ToList();

                if (byTime.Any())
                {
                    Debug.WriteLine($"[DETECT] Time fallback: {byTime.Count}");
                    AttachToProcesses(byTime);
                    return;
                }

                if (attempts == 5)
                    _detectionTimer.Interval = 3000;

                if (attempts > 20)
                {
                    Debug.WriteLine("[DETECT] Timeout");
                    CancelLaunching();
                }
            }
            catch { }
        };

        _detectionTimer.Start();
    }

    private void AttachToProcesses(List<Process> processes)
    {
        if (_currentGame == null) return;

        foreach (var process in processes)
        {
            try
            {
                if (_trackedProcesses.Any(p => p.Id == process.Id))
                    continue;

                process.EnableRaisingEvents = true;
                process.Exited += OnProcessExited;

                _trackedProcesses.Add(process);
            }
            catch { }
        }
        
        if (string.IsNullOrEmpty(_currentGame.GamePaths.ExeRelativePath))
        {
            try
            {
                var main = processes
                    .OrderByDescending(p => p.StartTime)
                    .FirstOrDefault();

                var path = main?.MainModule?.FileName;

                if (!string.IsNullOrEmpty(path))
                {
                    _currentGame.GamePaths.ExeRelativePath = path;
                    Debug.WriteLine($"[AUTO] Exe detected: {path}");
                }
            }
            catch { }
        }

        if (!_currentGame.IsRunning)
        {
            _currentGame.IsRunning = true;
            OnGameStarted?.Invoke(_currentGame);
        }

        CleanupDetection();
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (sender is Process p)
        {
            _trackedProcesses.RemoveAll(x => x.Id == p.Id);
            try { p.Dispose(); } catch { }
        }

        if (_trackedProcesses.Count == 0 && _currentGame != null)
        {
            var playTime = DateTime.Now - _startTime;

            _currentGame.IsRunning = false;
            OnGameClosed?.Invoke(_currentGame, playTime);
            _currentGame = null;
        }
    }

    private void CleanupDetection()
    {
        _detectionTimer?.Stop();
        _detectionTimer?.Dispose();
        _detectionTimer = null;
    }

    private void Cleanup()
    {
        CleanupDetection();

        foreach (var p in _trackedProcesses)
        {
            try { p.Dispose(); } catch { }
        }

        _trackedProcesses.Clear();
    }

    private void CancelLaunching()
    {
        StopGame();
    }
}