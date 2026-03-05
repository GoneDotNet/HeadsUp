using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayScoreListScreen
{
    readonly CPInterfaceController controller;
    readonly IGameService gameService;
    readonly CarPlaySceneDelegate navigator;

    public CPListTemplate Template { get; }

    public CarPlayScoreListScreen(
        CPInterfaceController controller,
        IGameService gameService,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.gameService = gameService;
        this.navigator = navigator;

        Template = new CPListTemplate("🏆 Game History", Array.Empty<CPListSection>());
        _ = LoadScores();
    }

    async Task LoadScores()
    {
        var games = await gameService.GetGameResults();

        var items = games.Select(g =>
        {
            var score = g.Answers.Count(a => a.AnswerType == AnswerType.Success);
            var item = new CPListItem(
                $"{g.CreatedAt:MMM dd, yyyy}",
                $"📂 {g.Category} • 🏆 {score} correct"
            );
            item.Handler = (_, completion) =>
            {
                navigator.NavigateToScore(g.GameId, false);
                completion();
            };
            return (ICPListTemplateItem)item;
        }).ToArray();

        if (items.Length == 0)
        {
            var emptyItem = new CPListItem("No games yet", "Play a game to see scores here!");
            Template.UpdateSections(new[] { new CPListSection(new ICPListTemplateItem[] { emptyItem }) });
        }
        else
        {
            Template.UpdateSections(new[] { new CPListSection(items) });
        }
    }
}
