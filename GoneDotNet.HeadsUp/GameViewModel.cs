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
    readonly GameEngine engine = new(beeper, gameService);
    
    [ObservableProperty] string answerText = "";
    [ObservableProperty] int countdown = 60;
    [ObservableProperty] string stateText = "";
    [ObservableProperty] Color stateColor = Colors.Blue;
    [ObservableProperty] bool controlsVisible = true;

    [RelayCommand]
    void ToggleControls() => ControlsVisible = !ControlsVisible;
    
    public async void OnAppearing()
    {
        foreach (var detector in answerDetectors)
            await detector.Start();
        
        try
        {
            var path = Path.Combine(fileSystem.AppDataDirectory, gameService.Id + ".mp4");
            await videoRecorder.StartRecording(path, true, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start video recording");
        }

        // wire detectors to engine
        foreach (var detector in answerDetectors)
            detector.AnswerDetected += OnDetectorAnswerDetected;

        engine.StateChanged += OnEngineStateChanged;
        engine.GameOver += OnEngineGameOver;
        engine.Start();
    }

    public void OnDisappearing()
    {
        this.StopGame();
    }

    async Task StopGame()
    {
        if (!gameService.IsGameInProgress)
            return;
        
        try
        {
            engine.StateChanged -= OnEngineStateChanged;
            engine.GameOver -= OnEngineGameOver;
            await engine.Stop();

            foreach (var detector in answerDetectors)
            {
                detector.AnswerDetected -= OnDetectorAnswerDetected;
                await detector.Stop();
            }
            
            videoRecorder.StopRecording();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop game");
        }
    }

    void OnDetectorAnswerDetected(object? sender, AnswerType answerType)
        => engine.SubmitAnswer(answerType);

    void OnEngineStateChanged(object? sender, GameEngineStateEventArgs e)
    {
        this.AnswerText = e.AnswerText;
        this.Countdown = e.Countdown;
        SetState(e.State);
    }

    async void OnEngineGameOver(object? sender, EventArgs e)
    {
        await navigator.NavigateToScore(gameService.Id, true);
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
                break;
            
            case ScreenState.Pass:
                StateColor = Colors.Orange;
                StateText = "Pass";
                break;
            
            case ScreenState.GameOver:
                StateText = "Game Over";
                StateColor = Colors.BlueViolet;
                break;
        }
    }
}