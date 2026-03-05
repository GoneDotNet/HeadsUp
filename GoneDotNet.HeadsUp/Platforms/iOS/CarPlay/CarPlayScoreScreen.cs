using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayScoreScreen
{
    readonly CPInterfaceController controller;
    readonly IGameService gameService;
    readonly Guid gameId;
    readonly bool isFromGame;
    readonly CarPlaySceneDelegate navigator;

    public CPListTemplate Template { get; }

    public CarPlayScoreScreen(
        CPInterfaceController controller,
        IGameService gameService,
        Guid gameId,
        bool isFromGame,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.gameService = gameService;
        this.gameId = gameId;
        this.isFromGame = isFromGame;
        this.navigator = navigator;

        Template = new CPListTemplate("🏆 Game Results", Array.Empty<CPListSection>());
        _ = LoadScore();
    }

    async Task LoadScore()
    {
        var game = await gameService.GetGameResult(gameId);
        var score = game.Answers.Count(a => a.AnswerType == AnswerType.Success);

        var answerItems = game.Answers.Select(a =>
        {
            var icon = a.AnswerType switch
            {
                AnswerType.Success => "✅",
                AnswerType.Pass => "⏭️",
                _ => "⏰"
            };
            var resultText = a.AnswerType switch
            {
                AnswerType.Success => "Success",
                AnswerType.Pass => "Pass",
                _ => "Unanswered"
            };
            return (ICPListTemplateItem)new CPListItem($"{icon} {a.Answer}", resultText);
        }).ToArray();

        var summaryItem = new CPListItem(
            $"📊 Score: {score} correct",
            $"📂 {game.Category} • {game.CreatedAt:MM/dd/yyyy}"
        );

        var backItem = new CPListItem(
            isFromGame ? "🏠 Main Menu" : "⬅️ Back",
            "Return"
        );
        backItem.Handler = (_, completion) =>
        {
            if (isFromGame)
                navigator.PopToRoot();
            else
                navigator.GoBack();
            completion();
        };

        var summarySection = new CPListSection(new ICPListTemplateItem[] { summaryItem, backItem }, "Summary", null);
        var answersSection = new CPListSection(answerItems, "Answer Breakdown", null);

        Template.UpdateSections(new[] { summarySection, answersSection });
    }
}
