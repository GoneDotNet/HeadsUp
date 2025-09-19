namespace GoneDotNet.HeadsUp;

// TODO: navigate to results page with game summary when game ends
[ShellMap<GamePage>]
public partial class GameViewModel(
    INavigator navigator,
    //TimeProvider timeProvider, 
    IGameContext gameContext,
    IVideoRecorder videoRecorder,
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
        // TODO: start some music
        foreach (var detector in answerDetectors)
            await detector.Start();
        
        var path = Path.Combine(FileSystem.AppDataDirectory, gameContext.Id + ".mp4");
        await videoRecorder.StartRecording(path, true, false);
        
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
        
        gameContext.EndGame();
    }
    
    
    async void OnGameTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // TODO: worry about initial loop
        // TODO: watch main thread
        this.Countdown--;

        if (this.Countdown <= 0)
        {
            await this.StopGame();
            SetState(ScreenState.GameOver);
            await Task.Delay(2000);
            
            // TODO: navigate to summary page
            //         // TODO: navigate out (or timer to navigate out?)
            //         // TODO: show summary of game (answers - correct, passed, missed)
        }        
        else if (this.Countdown <= 10)
        {
            // TODO: DI this stuff
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
            // TODO: play a sound here as well
        }
    }
    

    async Task DoAnswer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            SetState(ScreenState.InAnswer);
            this.AnswerText = gameContext.CurrentAnswer;
            
            var answerType = await this.WaitForAnswer(cancellationToken);
            gameContext.MarkAnswer(answerType);

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
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                break;
            
            case ScreenState.Pass:
                StateColor = Colors.Orange;
                StateText = "Pass";
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
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