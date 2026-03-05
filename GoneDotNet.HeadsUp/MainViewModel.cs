namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    INavigator navigator,
    ICategoryRespository repository,
    IBeepService beeper
) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToScoreList() => navigator.NavigateToScoreList();
    [RelayCommand] Task NavToManageCategories() => navigator.NavigateToCategoryList();
    [ObservableProperty] GameCategoryViewModel[] categories;
    [ObservableProperty] string themeSongIcon = GetThemeSongIcon();

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

        var cats = await repository.GetAll();
        this.Categories = cats
            .Select(x => new GameCategoryViewModel(navigator, x.Name, x.Description))
            .ToArray();
    }

    public void OnDisappearing()
    {
    }
}

public partial class GameCategoryViewModel(
    INavigator navigator, 
    string name, 
    string description
) : ObservableObject
{
    public string Name => name;
    public string Description => description;

    [RelayCommand]
    async Task NavToGame()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await navigator.Alert("No Connection", "An internet connection is required to play. Please check your connection and try again.");
            return;
        }
        
        var confirm = await navigator.Confirm(
            "Start Game", 
            $"Start a new game in the {Name} category?"
        );
        if (confirm)
            await navigator.NavigateTo<ReadyViewModel>(x => x.Category = Name);
    }
}