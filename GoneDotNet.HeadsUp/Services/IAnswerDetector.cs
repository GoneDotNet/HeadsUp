namespace GoneDotNet.HeadsUp.Services;

public interface IAnswerDetector
{
    event Action<AnswerType> AnswerDetected;
    Task Start();
    Task Stop();
}
