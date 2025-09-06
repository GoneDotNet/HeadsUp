namespace GoneDotNet.HeadsUp.Services;

public interface IAnswerDetector
{
    event EventHandler<AnswerType> AnswerDetected;
    Task Start();
    Task Stop();
}
