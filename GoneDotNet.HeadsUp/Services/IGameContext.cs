namespace GoneDotNet.HeadsUp.Services;


public interface IGameContext
{
    Guid Id { get; }
    string CurrentAnswer { get; }
    string CurrentCategory { get; }
    int AnswerNumber { get; }

    void StartGame(string category, string[] answers);
    void EndGame();
    
    void MarkAnswer(AnswerType answerType);
}