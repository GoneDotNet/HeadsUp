namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorAnswerDetector : IAnswerDetector
{
    // TODO: flip up means pass, flip down means correct
    IAccelerometer Acc => Accelerometer.Default;

    public event Action<AnswerType>? AnswerDetected;
    
    public Task Start()
    {
        if (this.Acc is { IsMonitoring: false, IsSupported: true })
        {
            Acc.ReadingChanged += AccOnReadingChanged;
            Acc.Start(SensorSpeed.Game);
        }

        return Task.CompletedTask;
    }
    

    public Task Stop()
    {
        Acc.Stop();
        Acc.ReadingChanged -= AccOnReadingChanged;
        return Task.CompletedTask;
    }
    
    
    void AccOnReadingChanged(object? sender, AccelerometerChangedEventArgs args)
    {
        // TODO: device orientation may matter (reverse on landscape reverse?
        var v = args.Reading.Acceleration;
        if (v.Y > 4f)
        {
            this.AnswerDetected?.Invoke(AnswerType.Success);
        }
        else if (v.Y < -4f)
        {
            this.AnswerDetected?.Invoke(AnswerType.Pass);
        }
        // Console.WriteLine($"X: {v.X} - Y: {v.Y} - Z: {v.Z}");
    }
}