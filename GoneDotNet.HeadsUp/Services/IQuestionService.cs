namespace GoneDotNet.HeadsUp.Services;

public interface IQuestionService
{
    Task<string[]> GetQuestions(string category, int count, CancellationToken cancellationToken);
}