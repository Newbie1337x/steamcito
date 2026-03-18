using System.Diagnostics;
using System.IO;
using steamcito.Models;
using steamcito.Models.Enum;
using Timer = System.Timers.Timer;


namespace steamcito.Services;

public class GameSessionManager
{
    private Process? _trackedProcess;
    private Timer? _detectionTimer;
    private DateTime _startTime;
    private string _processName = "";
    private Game? _currentGame;

    public event Action<Game>? OnGameLaunching;
    public event Action<Game>? OnGameStarted;
    public event Action<Game, TimeSpan>? OnGameClosed;

    public void LaunchGame(Game game)
    {
        if (game.IsRunning) return;

        _currentGame = game;
        _currentGame.IsRunning = true;
        _processName = Path.GetFileNameWithoutExtension(game.GamePaths.ExePath);
        
       //capturing time before start the timer asserting race conditions.
        var launchTime = DateTime.Now.AddSeconds(-1);
        _startTime = DateTime.Now;

        OnGameLaunching?.Invoke(_currentGame);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = game.GamePaths.ExePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(game.GamePaths.ExePath)
            });

            if (process != null)
            {
                //if the process is a game attach directly 
                AttachToProcess(process);
            }
            else
            {
                //if not return a process search by name
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
            if (_trackedProcess != null && !_trackedProcess.HasExited)
            {
                _trackedProcess.Kill();
            }
            
            _detectionTimer?.Stop();
            _detectionTimer?.Dispose();
            _detectionTimer = null;

            if (_currentGame != null)
            {
                _currentGame.IsRunning = false;
                OnGameClosed?.Invoke(_currentGame, TimeSpan.Zero);
                _currentGame = null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping game: {ex.Message}");
        }
    }

    private void CancelLaunching()
    {
        StopGame();
    }

    private void StartDetection(DateTime launchTime)
    {
        _detectionTimer = new Timer(2000); 
        _detectionTimer.Elapsed += (s, e) =>
        {
            try
            {
                var processes = Process.GetProcessesByName(_processName)
                    .Where(p =>
                    {
                        try
                        {
                            // searching for the process right after o later.
                            return p.StartTime >= launchTime;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .OrderByDescending(p => p.StartTime)
                    .ToList();

                if (processes.Any())
                {
                    AttachToProcess(processes.First());
                }
            }
            catch
            {
                //ignore access violations 
                //TODO impl
            }
        };

        _detectionTimer.Start();
    }

    private void AttachToProcess(Process process)
    {
        if (_currentGame == null) return;

        _trackedProcess = process;
        _currentGame.IsRunning = true;

        _trackedProcess.EnableRaisingEvents = true;
        _trackedProcess.Exited += OnProcessExited;

       //if ended before the event
        if (_trackedProcess.HasExited)
        {
            OnProcessExited(_trackedProcess, EventArgs.Empty);
            return;
        }

        _detectionTimer?.Stop();
        _detectionTimer?.Dispose();
        _detectionTimer = null;

        OnGameStarted?.Invoke(_currentGame);
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        var playTime = DateTime.Now - _startTime;

        if (_currentGame != null)
        {
            _currentGame.IsRunning = false;
            OnGameClosed?.Invoke(_currentGame, playTime);
            _currentGame = null;
        }

        _trackedProcess?.Dispose();
        _trackedProcess = null;
    }
}