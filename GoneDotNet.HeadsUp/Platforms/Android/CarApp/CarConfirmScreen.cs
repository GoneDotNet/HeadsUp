using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarConfirmScreen : Screen
{
    readonly string title;
    readonly string message;
    readonly Func<Task> onConfirm;

    public CarConfirmScreen(CarContext carContext, string title, string message, Func<Task> onConfirm)
        : base(carContext)
    {
        this.title = title;
        this.message = message;
        this.onConfirm = onConfirm;
    }

    public override ITemplate OnGetTemplate()
    {
        return new MessageTemplate.Builder(message)
            .SetTitle(title)
            .SetHeaderAction(Action.Back)
            .AddAction(
                new Action.Builder()
                    .SetTitle("Delete")
                    .SetBackgroundColor(CarColor.Red)
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        _ = Task.Run(async () =>
                        {
                            await onConfirm();
                            MainThread.BeginInvokeOnMainThread(() => ScreenManager.Pop());
                        });
                    }))
                    .Build()
            )
            .AddAction(
                new Action.Builder()
                    .SetTitle("Cancel")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ScreenManager.Pop();
                    }))
                    .Build()
            )
            .Build();
    }
}
