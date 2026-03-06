using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarMainScreen : Screen
{
    IReadOnlyList<GameCategory>? categories;

    public CarMainScreen(CarContext carContext) : base(carContext)
    {
        StartThemeSongIfEnabled();
        _ = LoadCategories();
    }

    void StartThemeSongIfEnabled()
    {
        var enabled = Preferences.Default.Get("ThemeSongEnabled", true);
        if (enabled)
        {
            CarServiceResolver.Beeper.PlayThemeSong();
            CarServiceResolver.Beeper.SetThemeVolume(1.0f);
        }
    }

    async Task LoadCategories()
    {
        categories = await CarServiceResolver.CategoryRepo.GetAll();
        MainThread.BeginInvokeOnMainThread(() => Invalidate());
    }

    public override ITemplate OnGetTemplate()
    {
        var actionStrip = new ActionStrip.Builder()
            .AddAction(
                new Action.Builder()
                    .SetTitle("🏆 Scores")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ScreenManager.Push(new CarScoreListScreen(CarContext));
                    }))
                    .Build()
            )
            .AddAction(
                new Action.Builder()
                    .SetTitle("⚙️ Categories")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ScreenManager.Push(new CarCategoryListScreen(CarContext));
                    }))
                    .Build()
            )
            .AddAction(
                new Action.Builder()
                    .SetTitle(GetThemeSongLabel())
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ToggleThemeSong();
                        Invalidate();
                    }))
                    .Build()
            )
            .Build();

        if (categories == null)
        {
            return new ListTemplate.Builder()
                .SetTitle("🎯 Heads Up")
                .SetHeaderAction(Action.AppIcon)
                .SetActionStrip(actionStrip)
                .SetLoading(true)
                .Build();
        }

        var listBuilder = new ItemList.Builder();
        foreach (var cat in categories)
        {
            var c = cat;
            listBuilder.AddItem(
                new Row.Builder()
                    .SetTitle(c.Name)
                    .AddText(c.Description ?? "")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        if (Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                        {
                            ScreenManager.Push(new CarAlertScreen(CarContext, "No Connection",
                                "An internet connection is required to play."));
                            return;
                        }
                        ScreenManager.Push(new CarReadyScreen(CarContext, c.Name));
                    }))
                    .Build()
            );
        }

        return new ListTemplate.Builder()
            .SetTitle("🎯 Heads Up")
            .SetHeaderAction(Action.AppIcon)
            .SetActionStrip(actionStrip)
            .SetSingleList(listBuilder.Build())
            .Build();
    }

    static string GetThemeSongLabel()
        => Preferences.Default.Get("ThemeSongEnabled", true) ? "🔊 Music" : "🔇 Music";

    static void ToggleThemeSong()
    {
        var enabled = Preferences.Default.Get("ThemeSongEnabled", true);
        enabled = !enabled;
        Preferences.Default.Set("ThemeSongEnabled", enabled);

        if (enabled)
        {
            CarServiceResolver.Beeper.PlayThemeSong();
            CarServiceResolver.Beeper.SetThemeVolume(1.0f);
        }
        else
        {
            CarServiceResolver.Beeper.StopThemeSong();
        }
    }
}

internal class ActionClickListener : Java.Lang.Object, IOnClickListener
{
    readonly System.Action action;
    public ActionClickListener(System.Action action) => this.action = action;
    public void OnClick() => action();
}
