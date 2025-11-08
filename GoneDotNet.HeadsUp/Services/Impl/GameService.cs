namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class GameService(
    MySqliteConnection conn,
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

    
    public void EndGame()
    {
        if (!this.IsGameInProgress)
            return;

        this.IsGameInProgress = false;
        var sconn = conn.GetConnection();
        sconn.BeginTransaction();
        
        sconn.Insert(new Game
        {
            Id = this.Id,
            Category = this.CurrentCategory,
            CreatedAt = DateTimeOffset.UtcNow
        });
        
        foreach (var answer in this.history)
        {
            sconn.Insert(new GameAnswer
            {
                Id = Guid.NewGuid(),
                GameId = this.Id,
                Value = answer.Answer,
                Timestamp = answer.Timestamp,
                AnswerType = answer.AnswerType
            });
        }

        // add current answer as unanswered
        sconn.Insert(new GameAnswer
        {
            Id = Guid.NewGuid(),
            GameId = this.Id,
            Value = this.CurrentAnswer.DisplayValue,
            Timestamp = DateTimeOffset.UtcNow,
            AnswerType = null
        });
        sconn.Commit();
    }

    
    public void MarkAnswer(AnswerType answerType)
    {
        this.history.Add((this.CurrentAnswer.DisplayValue, DateTimeOffset.UtcNow, answerType));
        
        this.AnswerNumber++;
        this.CurrentAnswer = this.answers[this.AnswerNumber - 1];
    }
    

    public async Task<GameResult> GetGameResult(Guid gameId)
    {
        var game = await conn.GetAsync<Game>(gameId);
        
        var gameAnswers = await conn.Table<GameAnswer>()
            .Where(ga => ga.GameId == gameId)
            .OrderBy(ga => ga.Id)
            .ToListAsync();
        
        return new GameResult(
            game.Id,
            game.Category,
            game.CreatedAt,
            gameAnswers
                .OrderBy(x => x.Timestamp)
                .Select(ga => (ga.Value, ga.AnswerType))
                .ToList()
        );
    }

    
    public async Task<List<GameResult>> GetGameResults()
    {
        var games = await conn.Games
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        
        var gameAnswers = await conn.Table<GameAnswer>()
            .OrderBy(ga => ga.Timestamp)
            .ToListAsync();
        
        return games
            .Select(g => new GameResult(
                g.Id,
                g.Category,
                g.CreatedAt,
                gameAnswers
                    .Where(ga => ga.GameId == g.Id)
                    .Select(ga => (ga.Value, ga.AnswerType))
                    .ToList()
            ))
            .ToList();
    }


    public async Task<List<string>> GetRecentAnswersByCategory(string categoryName)
    {
        var answers = await conn.QueryAsync<GameAnswer>(
            """
            SELECT 
                DISTINCT ga.Value
            FROM 
                GameAnswer ga
            JOIN Game g ON ga.GameId = g.Id
            WHERE g.Category = ?
            ORDER BY ga.Timestamp DESC
            LIMIT 10
            """,
            categoryName
        );

        logger.LogDebug(
            "GetRecentAnswersByCategory: {answersCount} for category '{categoryName}'",
            answers.Count,
            categoryName
        );
        return answers.Select(x => x.Value).ToList();
    }
}