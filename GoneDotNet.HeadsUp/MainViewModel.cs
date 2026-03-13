namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    INavigator navigator,
    IDialogs dialogs,
    ICategoryRespository repository,
    IVersionTracking versionTracking,
    IBeepService beeper,
    ILogger<MainViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToScoreList() => navigator.NavigateToScoreList();
    [RelayCommand] Task NavToManageCategories() => navigator.NavigateToCategoryList();
    [ObservableProperty] GameCategoryViewModel[] categories;
    [ObservableProperty] string themeSongIcon = GetThemeSongIcon();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotSeeding))]
    bool isSeeding;

    public bool IsNotSeeding => !this.IsSeeding;

    [RelayCommand]
    void ToggleThemeSong()
    {
        var enabled = Preferences.Default.Get("ThemeSongEnabled", true);
        enabled = !enabled;
        Preferences.Default.Set("ThemeSongEnabled", enabled);
        this.ThemeSongIcon = GetThemeSongIcon();

        if (enabled)
        {
            beeper.PlayThemeSong();
            beeper.SetThemeVolume(1.0f);
        }
        else
        {
            beeper.StopThemeSong();
        }
    }

    static string GetThemeSongIcon() =>
        Preferences.Default.Get("ThemeSongEnabled", true) ? "🔊" : "🔇";

    public async void OnAppearing()
    {
        this.ThemeSongIcon = GetThemeSongIcon();
        if (Preferences.Default.Get("ThemeSongEnabled", true))
        {
            beeper.PlayThemeSong();
            beeper.SetThemeVolume(1.0f);
        }

        if (versionTracking.IsFirstLaunchEver)
        {
            var seed = await dialogs.Confirm(
                "Welcome!",
                "Would you like to create some default categories to get started?"
            );
            if (seed)
                await SeedDefaultCategories();
        }

        await LoadCategories();
    }

    [RelayCommand]
    async Task SeedDefaults()
    {
        await SeedDefaultCategories();
        await LoadCategories();
    }

    async Task SeedDefaultCategories()
    {
        this.IsSeeding = true;
        try
        {
            await repository.SeedDefaults();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed default categories");
        }
        this.IsSeeding = false;
    }

    async Task LoadCategories()
    {
        var cats = await repository.GetAll();
        this.Categories = cats
            .Select(x => new GameCategoryViewModel(
                navigator, 
                dialogs, 
                x.Name, 
                x.Description,
                x.Answers.Count > 0
            ))
            .ToArray();
    }

    public void OnDisappearing()
    {
    }
}

public partial class GameCategoryViewModel(
    INavigator navigator,
    IDialogs dialogs,
    string name, 
    string description,
    bool hasAnswers
) : ObservableObject
{
    public string Name => name;
    public string Description => description;

    [RelayCommand]
    async Task NavToGame()
    {
        var confirm = await dialogs.Confirm(
            "Start Game", 
            $"Start a new game in the {Name} category?"
        );
        if (confirm)
            await navigator.NavigateTo<ReadyViewModel>(x => x.Category = Name);
    }
}