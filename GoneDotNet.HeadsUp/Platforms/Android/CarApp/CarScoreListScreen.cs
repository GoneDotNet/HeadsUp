using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarScoreListScreen : Screen
{
    IReadOnlyList<GameResult>? games;

    public CarScoreListScreen(CarContext carContext) : base(carContext)
    {
        _ = LoadGames();
    }

    public override ITemplate OnGetTemplate()
    {
        if (games == null)
        {
            return new ListTemplate.Builder()
                .SetTitle("🏆 Game History")
                .SetHeaderAction(Action.Back)
                .SetLoading(true)
                .Build();
        }

        if (games.Count == 0)
        {
            return new MessageTemplate.Builder("No games played yet!")
                .SetTitle("🏆 Game History")
                .SetHeaderAction(Action.Back)
                .Build();
        }

        var listBuilder = new ItemList.Builder();

        foreach (var g in games)
        {
            var game = g;
            var score = game.Answers.Count(a => a.AnswerType == AnswerType.Success);

            listBuilder.AddItem(
                new Row.Builder()
                    .SetTitle($"🎯 {game.Category}")
                    .AddText($"Score: {score} | {game.CreatedAt:d}")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ScreenManager.Push(new CarScoreScreen(CarContext, game.GameId, false));
                    }))
                    .Build()
            );
        }

        return new ListTemplate.Builder()
            .SetTitle("🏆 Game History")
            .SetHeaderAction(Action.Back)
            .SetSingleList(listBuilder.Build())
            .Build();
    }

    async Task LoadGames()
    {
        var results = await CarServiceResolver.GameService.GetGameResults();
        games = results.OrderByDescending(g => g.CreatedAt).ToList();
        MainThread.BeginInvokeOnMainThread(() => Invalidate());
    }
}
