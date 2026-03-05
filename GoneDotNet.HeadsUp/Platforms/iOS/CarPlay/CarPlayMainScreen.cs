using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayMainScreen
{
    readonly CPInterfaceController controller;
    readonly IBeepService beeper;
    readonly ICategoryRespository categoryRepo;
    readonly CarPlaySceneDelegate navigator;

    public CPListTemplate Template { get; }

    public CarPlayMainScreen(
        CPInterfaceController controller,
        IBeepService beeper,
        ICategoryRespository categoryRepo,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.beeper = beeper;
        this.categoryRepo = categoryRepo;
        this.navigator = navigator;

        Template = new CPListTemplate("🎯 Heads Up", Array.Empty<CPListSection>());
        _ = LoadCategories();
        StartThemeSongIfEnabled();
    }

    void StartThemeSongIfEnabled()
    {
        var enabled = Preferences.Default.Get("ThemeSongEnabled", true);
        if (enabled)
        {
            beeper.PlayThemeSong();
            beeper.SetThemeVolume(1.0f);
        }
    }

    async Task LoadCategories()
    {
        var cats = await categoryRepo.GetAll();

        var categoryItems = cats.Select(c =>
        {
            var item = new CPListItem(c.Name, c.Description);
            item.Handler = (listItem, completion) =>
            {
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    navigator.ShowAlert("No Connection", "An internet connection is required to play.");
                    completion();
                    return;
                }
                navigator.ShowConfirm("Start Game", $"Start a new game in the {c.Name} category?", confirmed =>
                {
                    if (confirmed)
                        navigator.NavigateToGame(c.Name);
                });
                completion();
            };
            return (ICPListTemplateItem)item;
        }).ToArray();

        var categorySection = new CPListSection(categoryItems, "Categories", null);

        var scoresItem = new CPListItem("🏆 Scores", "View game history");
        scoresItem.Handler = (_, completion) =>
        {
            navigator.NavigateToScoreList();
            completion();
        };

        var manageCategoriesItem = new CPListItem("⚙️ Manage Categories", "View and delete categories");
        manageCategoriesItem.Handler = (_, completion) =>
        {
            navigator.NavigateToCategoryList();
            completion();
        };

        var themeSongEnabled = Preferences.Default.Get("ThemeSongEnabled", true);
        var themeSongItem = new CPListItem(
            themeSongEnabled ? "🔊 Theme Song: ON" : "🔇 Theme Song: OFF",
            "Tap to toggle"
        );
        themeSongItem.Handler = (listItem, completion) =>
        {
            var enabled = Preferences.Default.Get("ThemeSongEnabled", true);
            enabled = !enabled;
            Preferences.Default.Set("ThemeSongEnabled", enabled);

            if (enabled)
            {
                beeper.PlayThemeSong();
                beeper.SetThemeVolume(1.0f);
            }
            else
            {
                beeper.StopThemeSong();
            }

            // Refresh the screen to update the toggle text
            _ = LoadCategories();
            completion();
        };

        var navSection = new CPListSection(
            new ICPListTemplateItem[] { scoresItem, manageCategoriesItem, themeSongItem },
            "Options",
            null
        );

        Template.UpdateSections(new[] { categorySection, navSection });
    }
}
