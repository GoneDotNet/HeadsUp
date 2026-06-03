using AVFoundation;
using Foundation;
using UIKit;

namespace GoneDotNet.HeadsUp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
        => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        var session = AVAudioSession.SharedInstance();
        session.SetCategory(
            AVAudioSessionCategory.PlayAndRecord,
            AVAudioSessionCategoryOptions.MixWithOthers |
            AVAudioSessionCategoryOptions.DefaultToSpeaker |
            AVAudioSessionCategoryOptions.AllowBluetooth
        );
        session.SetActive(true, out _);
        return base.FinishedLaunching(application, launchOptions);
    }

    [Export("application:configurationForConnectingSceneSession:options:")]
    public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
    {
        if (connectingSceneSession.Role.GetConstant() == UIWindowSceneSessionRole.CarTemplateApplication.GetConstant())
        {
            var config = new UISceneConfiguration("CarPlay", connectingSceneSession.Role);
            config.DelegateType = typeof(CarPlaySceneDelegate);
            return config;
        }
        var defaultConfig = base.GetConfiguration(application, connectingSceneSession, options);
        defaultConfig.DelegateType = typeof(SceneDelegate);
        return defaultConfig;
    }
}