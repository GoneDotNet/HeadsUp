namespace GoneDotNet.HeadsUp;

// TODO: navigate to results page with game summary when game ends
[ShellMap<GamePage>]
public partial class GameViewModel(
    IGameContext gameContext,
    IEnumerable<IAnswerDetector> answerDetectors
) : ObservableObject, IPageLifecycleAware
{
    readonly System.Timers.Timer gameTimer = new();
    readonly CancellationTokenSource cancellationTokenSource = new();
    
    [ObservableProperty] string answerText = "";
    [ObservableProperty] string timerText = "";
    [ObservableProperty] string stateText = "";
    [ObservableProperty] Color stateColor = Colors.Blue;
    
    bool isGameRunning;
    DateTime gameStartTime;
    TimeSpan gameDuration = TimeSpan.FromMinutes(2); // 2 minutes game duration
    
    public void OnAppearing()
    {
        _ = StartGame();
    }

    public void OnDisappearing()
    {
        StopGame();
    }

    async Task StartGame()
    {
        isGameRunning = true;
        gameStartTime = DateTime.Now;
        
        // Setup game timer to update every 100ms for smooth countdown
        gameTimer.Interval = 100;
        gameTimer.Elapsed += OnGameTimerElapsed;
        gameTimer.Start();
        
        await GameLoop();
    }

    void StopGame()
    {
        isGameRunning = false;
        gameTimer.Stop();
        gameTimer.Elapsed -= OnGameTimerElapsed;
        cancellationTokenSource.Cancel();
        gameContext.EndGame();
    }

    void OnGameTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!isGameRunning) return;
        
        var elapsed = DateTime.Now - gameStartTime;
        var remaining = gameDuration - elapsed;
        
        if (remaining <= TimeSpan.Zero)
        {
            // Game time is up
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerText = "Time's Up!";
                StateText = "Game Over";
                StateColor = Colors.Red;
            });
            
            StopGame();
            return;
        }
        
        // Update timer display
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TimerText = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        });
        
        // Vibrate during last 10 seconds
        if (remaining.TotalSeconds <= 10 && remaining.Milliseconds < 100)
        {
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
            }
            catch
            {
                // Vibration not supported on this platform
            }
        }
    }

    async Task GameLoop()
    {
        while (isGameRunning && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Loop();
                
                // Move to next answer after successful loop iteration
                if (isGameRunning)
                {
                    // Small delay before next question
                    await Task.Delay(100, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error and continue
                System.Diagnostics.Debug.WriteLine($"Error in game loop: {ex.Message}");
                await Task.Delay(1000, cancellationTokenSource.Token);
            }
        }
    }

    async Task Loop()
    {
        if (!isGameRunning) return;
        
        // Set up the new answer
        AnswerText = gameContext.CurrentAnswer;
        await SetState(ScreenState.InAnswer);
        
        // Create tasks for each answer detector
        var detectorTasksWithHandlers = answerDetectors.Select(detector =>
        {
            var tcs = new TaskCompletionSource<AnswerType>();
            
            void OnAnswerDetected(AnswerType answerType)
            {
                tcs.TrySetResult(answerType);
            }
            
            detector.AnswerDetected += OnAnswerDetected;
            return new { Task = tcs.Task, Detector = detector, Handler = (Action<AnswerType>)OnAnswerDetected };
        }).ToArray();
        
        try
        {
            // Create a timeout task for the current answer (in case no answer is detected)
            var timeoutTask = Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            var allTasks = detectorTasksWithHandlers.Select(x => x.Task).Append(timeoutTask);
            
            // Wait for the first detector to trigger or cancellation
            var completedTask = await Task.WhenAny(allTasks);
            
            // If it was the timeout task or cancellation, don't process further
            if (completedTask == timeoutTask || cancellationTokenSource.Token.IsCancellationRequested)
                return;
            
            // Get the result from the completed detector task
            var detectorTask = detectorTasksWithHandlers.FirstOrDefault(x => x.Task == completedTask);
            if (detectorTask == null) return;
            
            var result = await detectorTask.Task;
            
            // Handle the result based on the answer type
            switch (result)
            {
                case AnswerType.Pass:
                    await SetState(ScreenState.Pass);
                    break;
                
                case AnswerType.Success:
                    await SetState(ScreenState.Success);
                    break;
            }
            
            // Mark the answer in the game context
            gameContext.MarkAnswer(result);
            
            // Stay in Success/Pass state for 2 seconds (timer continues running in background)
            await Task.Delay(2000, cancellationTokenSource.Token);
        }
        finally
        {
            // Clean up all subscriptions
            foreach (var item in detectorTasksWithHandlers)
            {
                item.Detector.AnswerDetected -= item.Handler;
            }
        }
    }

    Task SetState(ScreenState state) => MainThread.InvokeOnMainThreadAsync(() =>
    {
        switch (state)
        {
            case ScreenState.InAnswer:
                StateColor = Colors.Blue;
                //StateText = "Guess the Answer!";
                break;
            
            case ScreenState.Success:
                StateColor = Colors.Green;
                StateText = "Success!";
                break;
            
            case ScreenState.Pass:
                StateColor = Colors.Orange;
                StateText = "Pass";
                break;
        }
    });
}

public enum ScreenState
{
    Success,
    Pass,
    InAnswer
}