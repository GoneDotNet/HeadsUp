namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorAnswerDetector : IAnswerDetector
{
    // TODO: flip up means pass, flip down means correct
    IAccelerometer Acc => Accelerometer.Default;

    public event Action<AnswerType>? AnswerDetected;
}