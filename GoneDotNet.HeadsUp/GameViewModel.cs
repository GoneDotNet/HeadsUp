namespace GoneDotNet.HeadsUp;


[ShellMap<GamePage>]
public partial class GameViewModel(
    ILogger<GameViewModel> logger,
    INavigator navigator,
    IBeepService beeper,
    IGameService gameService,
    IVideoRecorder videoRecorder,
    IFileSystem fileSystem,
    IEnumerable<IAnswerDetector> answerDetectors
) : ObservableObject, IPageLifecycleAware
{
    readonly System.Timers.Timer gameTimer = new();
    readonly CancellationTokenSource gameTokenSource = new();
    
    [ObservableProperty] string answerText = "";
    [ObservableProperty] int countdown = 60;
    [ObservableProperty] string stateText = "";
    [ObservableProperty] Color stateColor = Colors.Blue;
    
    public async void OnAppearing()
    {
        beeper.SetThemeVolume(0.5f);
        foreach (var detector in answerDetectors)
            await detector.Start();
        
        try
        {
            var path = Path.Combine(fileSystem.AppDataDirectory, gameService.Id + ".mp4");
            await videoRecorder.StartRecording(path, true, false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start video recording");
        }
        
        _ = this.DoAnswer(this.gameTokenSource.Token);

        gameTimer.Interval = 1000;
        gameTimer.Elapsed += OnGameTimerElapsed;
        gameTimer.Start();
    }

    public void OnDisappearing()
    {
        StopGame();
    }

    async Task StopGame()
    {
        gameTimer.Stop();
        gameTimer.Elapsed -= OnGameTimerElapsed;
        
        this.gameTokenSource.Cancel(); // cancel the answer loop
        videoRecorder.StopRecording();
        foreach (var detector in answerDetectors)
            await detector.Stop();
        
        gameService.EndGame();
    }
    
    
    async void OnGameTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        this.Countdown--;

        if (this.Countdown <= 0)
        {
            // beeper.GameOver();
            await this.StopGame();
            SetState(ScreenState.GameOver);
            await Task.Delay(2000);

            await navigator.NavigateToScore(gameService.Id, true);
        }        
        else if (this.Countdown <= 10)
        {
            beeper.Countdown();
        }
    }
    

    async Task DoAnswer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            SetState(ScreenState.InAnswer);
            this.AnswerText = gameService.CurrentAnswer;
            
            var answerType = await this.WaitForAnswer(cancellationToken);
            gameService.MarkAnswer(answerType);

            var state = answerType == AnswerType.Pass ? ScreenState.Pass : ScreenState.Success;
            SetState(state);
            await Task.Delay(2000, cancellationToken);
        }
    }


    async Task<AnswerType> WaitForAnswer(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<AnswerType>();
        var handler = new EventHandler<AnswerType>((_, args) => tcs.TrySetResult(args));

        try
        {
            foreach (var detector in answerDetectors)
                detector.AnswerDetected += handler;
            
            await using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
            return await tcs.Task;
        }
        finally
        {
            foreach (var detector in answerDetectors)
                detector.AnswerDetected -= handler;
        }
    }

    void SetState(ScreenState state)
    {
        switch (state)
        {
            case ScreenState.InAnswer:
                StateColor = Colors.Blue;
                StateText = String.Empty;
                break;
            
            case ScreenState.Success:
                StateColor = Colors.Green;
                StateText = "Success!";
                beeper.Success();
                break;
            
            case ScreenState.Pass:
                StateColor = Colors.Orange;
                StateText = "Pass";
                beeper.Pass();
                break;
            
            case ScreenState.GameOver:
                StateText = "Game Over";
                StateColor = Colors.BlueViolet;
                break;
        }
    }
}

public enum ScreenState
{
    Success,
    Pass,
    InAnswer,
    GameOver
}