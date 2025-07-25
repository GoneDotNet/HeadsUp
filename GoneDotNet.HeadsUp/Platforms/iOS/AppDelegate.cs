using Foundation;
using UIKit;

namespace GoneDotNet.HeadsUp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
        => MauiProgram.CreateMauiApp();
}