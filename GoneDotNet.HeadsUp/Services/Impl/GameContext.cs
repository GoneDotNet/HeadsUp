namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class GameContext(MySqliteConnection conn) : IGameContext
{
    public Guid Id { get; private set; }
    public string CurrentAnswer { get; private set; }
    public string CurrentCategory { get; private set; }
    public int AnswerNumber { get; private set; }

    readonly List<(string Answer, AnswerType AnswerType)> history = new();
    string[]? answers;
    
    public void StartGame(string category, string[] answers)
    {
        this.history.Clear();
        
        this.Id = Guid.NewGuid();
        this.CurrentCategory = category;
        this.answers = answers;
        this.AnswerNumber = 1;
        this.CurrentAnswer = this.answers[0];
    }

    
    public void EndGame()
    {
        // TODO: store to sqlite
        
    }

    
    public void MarkAnswer(AnswerType answerType)
    {
        this.history.Add((this.CurrentAnswer, answerType));
        
        this.AnswerNumber++;
        this.CurrentAnswer = this.answers[this.AnswerNumber - 1];
    }
}