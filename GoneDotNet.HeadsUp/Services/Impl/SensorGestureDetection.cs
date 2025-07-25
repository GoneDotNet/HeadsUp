namespace GoneDotNet.HeadsUp.Services.Impl;

[Singleton]
public class SensorGestureDetection : IGestureDetection
{
    IAccelerometer Acc => Accelerometer.Default;

    public event Action<GestureType>? GestureDetected;
}