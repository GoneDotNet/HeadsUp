using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarGameScreen : Screen
{
    readonly GameEngine engine;
    readonly IAnswerDetector[] detectors;
    bool answerBlurred;

    public CarGameScreen(CarContext carContext) : base(carContext)
    {
        detectors = CarServiceResolver.AnswerDetectors.ToArray();
        engine = new GameEngine(CarServiceResolver.Beeper, CarServiceResolver.GameService);
        engine.StateChanged += OnStateChanged;
        engine.GameOver += OnGameOver;
        engine.Start();

        _ = StartDetectorsAsync();
    }

    async Task StartDetectorsAsync()
    {
        foreach (var detector in detectors)
        {
            detector.AnswerDetected += OnDetectorAnswerDetected;
            await detector.Start();
        }
    }

    void OnDetectorAnswerDetected(object? sender, AnswerType answerType)
        => engine.SubmitAnswer(answerType);

    public override ITemplate OnGetTemplate()
    {
        var displayText = answerBlurred ? "???" : engine.AnswerText;
        var stateText = engine.State switch
        {
            ScreenState.Success => "✅ Success!",
            ScreenState.Pass => "⏭️ Pass",
            ScreenState.GameOver => "🏁 Game Over",
            _ => "Waiting for answer..."
        };

        var paneBuilder = new Pane.Builder();

        paneBuilder.AddRow(
            new Row.Builder()
                .SetTitle(displayText)
                .AddText($"⏰ {engine.Countdown}s remaining")
                .Build()
        );

        paneBuilder.AddRow(
            new Row.Builder()
                .SetTitle("Status")
                .AddText(stateText)
                .Build()
        );

        paneBuilder.AddAction(
            new Action.Builder()
                .SetTitle("✅ Success")
                .SetBackgroundColor(CarColor.Green)
                .SetOnClickListener(new ActionClickListener(() =>
                {
                    engine.SubmitAnswer(AnswerType.Success);
                }))
                .Build()
        );

        paneBuilder.AddAction(
            new Action.Builder()
                .SetTitle("⏭️ Pass")
                .SetBackgroundColor(CarColor.Yellow)
                .SetOnClickListener(new ActionClickListener(() =>
                {
                    engine.SubmitAnswer(AnswerType.Pass);
                }))
                .Build()
        );

        var actionStrip = new ActionStrip.Builder()
            .AddAction(
                new Action.Builder()
                    .SetTitle(answerBlurred ? "👁️ Show" : "👁️ Hide")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        answerBlurred = !answerBlurred;
                        Invalidate();
                    }))
                    .Build()
            )
            .Build();

        return new PaneTemplate.Builder(paneBuilder.Build())
            .SetTitle("🎮 GAME")
            .SetActionStrip(actionStrip)
            .Build();
    }

    void OnStateChanged(object? sender, GameEngineStateEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => Invalidate());
    }

    async void OnGameOver(object? sender, EventArgs e)
    {
        engine.StateChanged -= OnStateChanged;
        engine.GameOver -= OnGameOver;

        foreach (var detector in detectors)
        {
            detector.AnswerDetected -= OnDetectorAnswerDetected;
            await detector.Stop();
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ScreenManager.Pop();
            ScreenManager.Push(new CarScoreScreen(CarContext, CarServiceResolver.GameService.Id, true));
        });
    }
}
