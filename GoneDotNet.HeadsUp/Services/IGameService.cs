namespace GoneDotNet.HeadsUp.Services;


public interface IGameService
{
    bool IsGameInProgress { get; }
    Guid Id { get; }
    ProvidedAnswer CurrentAnswer { get; }
    string CurrentCategory { get; }
    int AnswerNumber { get; }

    void StartGame(string category, ProvidedAnswer[] answers);
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