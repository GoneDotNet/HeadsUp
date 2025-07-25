namespace GoneDotNet.HeadsUp.Services;

public interface IGestureDetection
{
    event Action<GestureType> GestureDetected;
}

public enum GestureType
{
    Pass,
    Correct
}

public class NoOpGestureDetection : IGestureDetection
{
    public event Action<GestureType>? GestureDetected;

    public void RaiseGestureDetected(GestureType gestureType)
    {
        GestureDetected?.Invoke(gestureType);
    }
}