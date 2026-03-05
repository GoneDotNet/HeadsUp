using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayReadyScreen
{
    readonly CPInterfaceController controller;
    readonly IBeepService beeper;
    readonly IGameService gameService;
    readonly IAnswerProvider answerProvider;
    readonly string category;
    readonly CarPlaySceneDelegate navigator;

    public CPInformationTemplate Template { get; }

    public CarPlayReadyScreen(
        CPInterfaceController controller,
        IBeepService beeper,
        IGameService gameService,
        IAnswerProvider answerProvider,
        string category,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.beeper = beeper;
        this.gameService = gameService;
        this.answerProvider = answerProvider;
        this.category = category;
        this.navigator = navigator;

        var items = new[]
        {
            new CPInformationItem("Category", category),
            new CPInformationItem("Status", "Generating answers...")
        };

        Template = new CPInformationTemplate(
            "🎮 GET READY!",
            CPInformationTemplateLayout.Leading,
            items,
            Array.Empty<CPTextButton>()
        );

        _ = StartCountdown();
    }

    async Task StartCountdown()
    {
        try
        {
            var questions = await answerProvider.GenerateAnswers(category, Constants.MaxAnswersPerGame, CancellationToken.None);
            gameService.StartGame(category, questions);

            beeper.SetThemeVolume(0.5f);

            for (int count = 5; count > 0; count--)
            {
                Template.Items = new[]
                {
                    new CPInformationItem("Category", category),
                    new CPInformationItem("Starting in", count.ToString())
                };

                beeper.Countdown();
                await Task.Delay(1000);
            }

            navigator.GoBack(); // pop ready screen
            navigator.NavigateToGamePlay();
        }
        catch (Exception)
        {
            navigator.ShowAlert("Error", "An error occurred while starting the game. Make sure you are connected to the internet.", () =>
            {
                navigator.GoBack();
            });
        }
    }
}
