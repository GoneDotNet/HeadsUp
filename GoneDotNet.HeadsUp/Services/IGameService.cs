namespace GoneDotNet.HeadsUp.Services;


public interface IGameService
{
    Guid Id { get; }
    string CurrentAnswer { get; }
    string CurrentCategory { get; }
    int AnswerNumber { get; }

    void StartGame(string category, string[] answers);
    void EndGame();
    
    void MarkAnswer(AnswerType answerType);

    Task<GameResult> GetGameResult(Guid gameId);
    Task<List<GameResult>> GetGameResults();
}

public record GameResult(
    Guid GameId,
    string Category,
    DateTimeOffset CreatedAt,
    List<(string Answer, AnswerType? AnswerType)> Answers
);