namespace GoneDotNet.HeadsUp.Services;


public class GameEngine
{
    readonly IBeepService beeper;
    readonly IGameService gameService;
    readonly System.Timers.Timer gameTimer = new();
    CancellationTokenSource? gameTokenSource;
    TaskCompletionSource<AnswerType>? answerTcs;

    public GameEngine(IBeepService beeper, IGameService gameService)
    {
        this.beeper = beeper;
        this.gameService = gameService;
    }

    public int Countdown { get; private set; } = 60;
    public string AnswerText { get; private set; } = "";
    public ScreenState State { get; private set; }
    public bool IsRunning { get; private set; }
    public Guid GameId => gameService.Id;

    public event EventHandler<GameEngineStateEventArgs>? StateChanged;
    public event EventHandler? GameOver;

    public void Start()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        Countdown = 60;
        gameTokenSource = new CancellationTokenSource();

        beeper.SetThemeVolume(0.5f);
        _ = DoAnswer(gameTokenSource.Token);

        gameTimer.Interval = 1000;
        gameTimer.Elapsed += OnGameTimerElapsed;
        gameTimer.Start();
    }

    public async Task Stop()
    {
        if (!IsRunning)
            return;

        IsRunning = false;

        gameTimer.Stop();
        gameTimer.Elapsed -= OnGameTimerElapsed;
        gameTokenSource?.Cancel();
        await gameService.EndGame();
    }

    /// <summary>
    /// Manually submit an answer (for CarPlay button presses or other non-detector input)
    /// </summary>
    public void SubmitAnswer(AnswerType answerType)
    {
        answerTcs?.TrySetResult(answerType);
    }

    async void OnGameTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Countdown--;

        if (Countdown <= 0)
        {
            await Stop();
            SetState(ScreenState.GameOver);
            await Task.Delay(2000);
            GameOver?.Invoke(this, EventArgs.Empty);
        }
        else if (Countdown <= 10)
        {
            beeper.Countdown();
        }

        StateChanged?.Invoke(this, new GameEngineStateEventArgs(State, AnswerText, Countdown));
    }

    async Task DoAnswer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            answerTcs = new TaskCompletionSource<AnswerType>();
            SetState(ScreenState.InAnswer);
            AnswerText = gameService.CurrentAnswer.DisplayValue;
            StateChanged?.Invoke(this, new GameEngineStateEventArgs(State, AnswerText, Countdown));

            try
            {
                await using var registration = cancellationToken.Register(() => answerTcs.TrySetCanceled());
                var answerType = await answerTcs.Task;
                gameService.MarkAnswer(answerType);

                var state = answerType == AnswerType.Pass ? ScreenState.Pass : ScreenState.Success;
                SetState(state);
                StateChanged?.Invoke(this, new GameEngineStateEventArgs(State, AnswerText, Countdown));
                await Task.Delay(2000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    void SetState(ScreenState state)
    {
        State = state;
        switch (state)
        {
            case ScreenState.Success:
                beeper.Success();
                break;
            case ScreenState.Pass:
                beeper.Pass();
                break;
        }
    }
}

public record GameEngineStateEventArgs(ScreenState State, string AnswerText, int Countdown);

public enum ScreenState
{
    Success,
    Pass,
    InAnswer,
    GameOver
}
