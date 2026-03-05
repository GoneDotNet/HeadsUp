using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayGameScreen
{
    readonly CPInterfaceController controller;
    readonly IBeepService beeper;
    readonly IGameService gameService;
    readonly CarPlaySceneDelegate navigator;
    readonly GameEngine engine;
    bool answerBlurred;

    public CPInformationTemplate Template { get; private set; }

    public CarPlayGameScreen(
        CPInterfaceController controller,
        IBeepService beeper,
        IGameService gameService,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.beeper = beeper;
        this.gameService = gameService;
        this.navigator = navigator;

        engine = new GameEngine(beeper, gameService);
        engine.StateChanged += OnStateChanged;
        engine.GameOver += OnGameOver;

        Template = BuildTemplate();
        engine.Start();
    }

    CPInformationTemplate BuildTemplate()
    {
        var displayText = answerBlurred ? "???" : engine.AnswerText;

        var items = new[]
        {
            new CPInformationItem("Answer", displayText),
            new CPInformationItem("Time", $"{engine.Countdown}s"),
            new CPInformationItem("Status", GetStateText(engine.State))
        };

        var successButton = new CPTextButton("✅ Success", CPTextButtonStyle.Confirm, _ =>
        {
            engine.SubmitAnswer(AnswerType.Success);
        });

        var passButton = new CPTextButton("⏭️ Pass", CPTextButtonStyle.Normal, _ =>
        {
            engine.SubmitAnswer(AnswerType.Pass);
        });

        var blurButton = new CPTextButton(answerBlurred ? "👁️ Show" : "👁️ Hide", CPTextButtonStyle.Normal, _ =>
        {
            answerBlurred = !answerBlurred;
            UpdateTemplate();
        });

        return new CPInformationTemplate(
            "🎮 GAME",
            CPInformationTemplateLayout.Leading,
            items,
            new[] { successButton, passButton, blurButton }
        );
    }

    void UpdateTemplate()
    {
        var displayText = answerBlurred ? "???" : engine.AnswerText;

        Template.Items = new[]
        {
            new CPInformationItem("Answer", displayText),
            new CPInformationItem("Time", $"{engine.Countdown}s"),
            new CPInformationItem("Status", GetStateText(engine.State))
        };

        Template.Actions = new[]
        {
            new CPTextButton("✅ Success", CPTextButtonStyle.Confirm, _ => engine.SubmitAnswer(AnswerType.Success)),
            new CPTextButton("⏭️ Pass", CPTextButtonStyle.Normal, _ => engine.SubmitAnswer(AnswerType.Pass)),
            new CPTextButton(answerBlurred ? "👁️ Show" : "👁️ Hide", CPTextButtonStyle.Normal, _ =>
            {
                answerBlurred = !answerBlurred;
                UpdateTemplate();
            })
        };
    }

    static string GetStateText(ScreenState state) => state switch
    {
        ScreenState.Success => "Success!",
        ScreenState.Pass => "Pass",
        ScreenState.GameOver => "Game Over",
        _ => "Waiting..."
    };

    void OnStateChanged(object? sender, GameEngineStateEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateTemplate());
    }

    async void OnGameOver(object? sender, EventArgs e)
    {
        engine.StateChanged -= OnStateChanged;
        engine.GameOver -= OnGameOver;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            navigator.GoBack(); // pop game screen
            navigator.NavigateToScore(gameService.Id, true);
        });
    }
}
