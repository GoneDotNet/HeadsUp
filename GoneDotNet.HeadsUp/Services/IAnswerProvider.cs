namespace GoneDotNet.HeadsUp.Services;

public interface IAnswerProvider
{
    Task<string[]> GenerateAnswers(string category, int count, CancellationToken cancellationToken);
}