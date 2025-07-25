using Android.App;
using Android.OS;
using Android.Content.PM;
using Microsoft.Maui.ApplicationModel;

namespace GoneDotNet.HeadsUp;

[Activity(
    LaunchMode = LaunchMode.SingleTop,
    ResizeableActivity = true,
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density
)]
[IntentFilter(
    [
        Platform.Intent.ActionAppAction,
        global::Android.Content.Intent.ActionView
    ],
    Categories =
    [
        global::Android.Content.Intent.CategoryDefault,
        global::Android.Content.Intent.CategoryBrowsable
    ]
)]
public class MainActivity : MauiAppCompatActivity
{
}