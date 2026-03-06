using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarAlertScreen : Screen
{
    readonly string title;
    readonly string message;

    public CarAlertScreen(CarContext carContext, string title, string message) : base(carContext)
    {
        this.title = title;
        this.message = message;
    }

    public override ITemplate OnGetTemplate()
    {
        return new MessageTemplate.Builder(message)
            .SetTitle(title)
            .SetHeaderAction(Action.Back)
            .Build();
    }
}
