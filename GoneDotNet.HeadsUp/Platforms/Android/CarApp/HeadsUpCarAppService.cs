using AndroidX.Car.App;
using AndroidX.Car.App.Validation;

namespace GoneDotNet.HeadsUp;

[Android.App.Service(
    Exported = true,
    Label = "GDN Heads Up")]
[Android.App.IntentFilter(
    new[] { "androidx.car.app.CarAppService" },
    Categories = new[] { "androidx.car.app.category.IOT" })]
[Android.App.MetaData(
    "com.google.android.gms.car.application",
    Resource = "@xml/automotive_app_desc")]
public class HeadsUpCarAppService : CarAppService
{
    public override HostValidator CreateHostValidator()
        => HostValidator.AllowAllHostsValidator;

    public override Session OnCreateSession()
        => new HeadsUpCarSession();
}
