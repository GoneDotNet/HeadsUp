using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarScoreScreen : Screen
{
    readonly Guid gameId;
    readonly bool isFromGame;
    GameResult? game;

    public CarScoreScreen(CarContext carContext, Guid gameId, bool isFromGame) : base(carContext)
    {
        this.gameId = gameId;
        this.isFromGame = isFromGame;
        _ = LoadScore();
    }

    public override ITemplate OnGetTemplate()
    {
        if (game == null)
        {
            return new PaneTemplate.Builder(
                new Pane.Builder().SetLoading(true).Build()
            )
            .SetTitle("🏆 Loading...")
            .SetHeaderAction(Action.Back)
            .Build();
        }

        var score = game.Answers.Count(a => a.AnswerType == AnswerType.Success);

        var listBuilder = new ItemList.Builder();

        foreach (var answer in game.Answers)
        {
            var icon = answer.AnswerType switch
            {
                AnswerType.Success => "✅",
                AnswerType.Pass => "⏭️",
                _ => "⏰"
            };
            var resultText = answer.AnswerType switch
            {
                AnswerType.Success => "Success",
                AnswerType.Pass => "Pass",
                _ => "Unanswered"
            };

            listBuilder.AddItem(
                new Row.Builder()
                    .SetTitle($"{icon} {answer.Answer}")
                    .AddText(resultText)
                    .Build()
            );
        }

        var headerAction = isFromGame
            ? new Action.Builder()
                .SetTitle("🏠 Menu")
                .SetOnClickListener(new ActionClickListener(() =>
                {
                    ScreenManager.PopToRoot();
                }))
                .Build()
            : Action.Back;

        return new ListTemplate.Builder()
            .SetTitle($"🏆 {game.Category} - Score: {score}")
            .SetHeaderAction(headerAction)
            .SetSingleList(listBuilder.Build())
            .Build();
    }

    async Task LoadScore()
    {
        game = await CarServiceResolver.GameService.GetGameResult(gameId);
        MainThread.BeginInvokeOnMainThread(() => Invalidate());
    }
}
