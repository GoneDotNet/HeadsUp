namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorAnswerDetector : IAnswerDetector
{
    IAccelerometer Acc => Accelerometer.Default;

    public event Action<AnswerType>? AnswerDetected;
}