using Shiny.DocumentDb;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class GameService(
    IDocumentStore store,
    TimeProvider timeProvider,
    ILogger<GameService> logger
) : IGameService
{
    public bool IsGameInProgress { get; private set; }
    public Guid Id { get; private set; }
    public ProvidedAnswer CurrentAnswer { get; private set; }
    public string CurrentCategory { get; private set; }
    public int AnswerNumber { get; private set; }

    readonly List<(string Answer, DateTimeOffset Timestamp, AnswerType AnswerType)> history = new();
    ProvidedAnswer[]? answers;
    
    public void StartGame(string category, ProvidedAnswer[] answers)
    {
        if (this.IsGameInProgress)
            return;
        
        this.IsGameInProgress = true;
        this.history.Clear();
        
        this.Id = Guid.NewGuid();
        this.CurrentCategory = category;
        this.answers = answers;
        this.AnswerNumber = 1;
        this.CurrentAnswer = this.answers[0];
    }

    
    public async Task EndGame()
    {
        if (!this.IsGameInProgress)
            return;

        this.IsGameInProgress = false;
        
        var game = new Game
        {
            Id = this.Id,
            Category = this.CurrentCategory,
            CreatedAt = timeProvider.GetUtcNow()
        };
        
        foreach (var answer in this.history)
        {
            game.Answers.Add(new GameAnswer
            {
                Value = answer.Answer,
                Timestamp = answer.Timestamp,
                AnswerType = answer.AnswerType
            });
        }

        // add current answer as unanswered
        game.Answers.Add(new GameAnswer
        {
            Value = this.CurrentAnswer.DisplayValue,
            Timestamp = timeProvider.GetUtcNow(),
            AnswerType = null
        });
        
        await store.Insert(game);
    }

    
    public void MarkAnswer(AnswerType answerType)
    {
        this.history.Add((this.CurrentAnswer.DisplayValue, DateTimeOffset.UtcNow, answerType));
        
        this.AnswerNumber++;
        this.CurrentAnswer = this.answers[this.AnswerNumber - 1];
    }
    

    public async Task<GameResult> GetGameResult(Guid gameId)
    {
        var game = await store.Get<Game>(gameId);
        if (game == null)
            throw new InvalidOperationException($"Game with id {gameId} not found");
        
        return new GameResult(
            game.Id,
            game.Category,
            game.CreatedAt,
            game.Answers
                .OrderBy(x => x.Timestamp)
                .Select(ga => (ga.Value, ga.AnswerType))
                .ToList()
        );
    }

    
    public async Task<List<GameResult>> GetGameResults()
    {
        var games = await store.Query<Game>()
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
        
        return games
            .Select(g => new GameResult(
                g.Id,
                g.Category,
                g.CreatedAt,
                g.Answers
                    .OrderBy(a => a.Timestamp)
                    .Select(ga => (ga.Value, ga.AnswerType))
                    .ToList()
            ))
            .ToList();
    }


    public async Task<List<string>> GetRecentAnswersByCategory(string categoryName)
    {
        var games = await store.Query<Game>()
            .Where(g => g.Category == categoryName)
            .OrderByDescending(g => g.CreatedAt)
            .ToList();
        
        var answers = games
            .SelectMany(g => g.Answers)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => a.Value)
            .Distinct()
            .Take(10)
            .ToList();

        logger.LogDebug(
            "GetRecentAnswersByCategory: {answersCount} for category '{categoryName}'",
            answers.Count,
            categoryName
        );
        return answers;
    }
}