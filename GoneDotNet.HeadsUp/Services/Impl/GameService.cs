namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class GameService(MySqliteConnection conn) : IGameService
{
    public bool IsGameInProgress { get; private set; }
    public Guid Id { get; private set; }
    public string CurrentAnswer { get; private set; }
    public string CurrentCategory { get; private set; }
    public int AnswerNumber { get; private set; }

    readonly List<(string Answer, AnswerType AnswerType)> history = new();
    string[]? answers;
    
    public void StartGame(string category, string[] answers)
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
                AnswerType = answer.AnswerType
            });
        }

        // add current answer as unanswered
        sconn.Insert(new GameAnswer
        {
            Id = Guid.NewGuid(),
            GameId = this.Id,
            Value = this.CurrentAnswer,
            AnswerType = null
        });
        sconn.Commit();
    }

    
    public void MarkAnswer(AnswerType answerType)
    {
        this.history.Add((this.CurrentAnswer, answerType));
        
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
            .OrderBy(ga => ga.Id)
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
}