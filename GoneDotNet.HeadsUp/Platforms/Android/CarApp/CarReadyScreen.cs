using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarReadyScreen : Screen
{
    readonly string category;
    int countdown = 5;
    string status = "Generating answers...";
    bool isReady;

    public CarReadyScreen(CarContext carContext, string category) : base(carContext)
    {
        this.category = category;
        _ = StartCountdown();
    }

    public override ITemplate OnGetTemplate()
    {
        var paneBuilder = new Pane.Builder()
            .SetLoading(!isReady);

        if (isReady)
        {
            paneBuilder.AddRow(
                new Row.Builder()
                    .SetTitle("Category")
                    .AddText(category)
                    .Build()
            );
            paneBuilder.AddRow(
                new Row.Builder()
                    .SetTitle("Starting in")
                    .AddText(countdown.ToString())
                    .Build()
            );
        }
        else
        {
            paneBuilder.AddRow(
                new Row.Builder()
                    .SetTitle("Category")
                    .AddText(category)
                    .Build()
            );
        }

        return new PaneTemplate.Builder(paneBuilder.Build())
            .SetTitle($"🎮 GET READY! - {status}")
            .SetHeaderAction(Action.Back)
            .Build();
    }

    async Task StartCountdown()
    {
        try
        {
            var questions = await CarServiceResolver.AnswerProvider.GenerateAnswers(
                category, Constants.MaxAnswersPerGame, CancellationToken.None);
            CarServiceResolver.GameService.StartGame(category, questions);

            CarServiceResolver.Beeper.SetThemeVolume(0.5f);
            isReady = true;

            for (int count = 5; count > 0; count--)
            {
                countdown = count;
                status = $"Starting in {count}...";
                MainThread.BeginInvokeOnMainThread(() => Invalidate());
                CarServiceResolver.Beeper.Countdown();
                await Task.Delay(1000);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScreenManager.Pop();
                ScreenManager.Push(new CarGameScreen(CarContext));
            });
        }
        catch (Exception)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScreenManager.Pop();
                ScreenManager.Push(new CarAlertScreen(CarContext, "Error",
                    "An error occurred while starting the game. Make sure you are connected to the internet."));
            });
        }
    }
}
