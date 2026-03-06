using Android.Content;
using AndroidX.Car.App;

namespace GoneDotNet.HeadsUp;

public class HeadsUpCarSession : Session
{
    public override Screen OnCreateScreen(Intent? intent)
    {
        return new CarMainScreen(CarContext);
    }
}
