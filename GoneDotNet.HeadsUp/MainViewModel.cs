namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    INavigator navigator,
    IBeepService beeper
) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToScoreList() => navigator.NavigateToScoreList();
    [ObservableProperty] GameCategory[] categories;

    public void OnAppearing()
    {
        beeper.PlayThemeSong();
        beeper.SetThemeVolume(1.0f);
        
        this.Categories =
        [
            new GameCategory(
                navigator, 
                "Disney Princesses", 
                "A collection of games featuring Disney princesses."
            ),
            new GameCategory(
                navigator, 
                "Popular rock songs from the 80s",
                "A collection of popular rock songs from the 1980s."
            )
        ];
    }

    public void OnDisappearing()
    {
    }
}

public partial class GameCategory(
    INavigator navigator, 
    string name, 
    string description
) : ObservableObject
{
    public string Name => name;
    public string Description => description;
    
    [RelayCommand]
    Task NavToGame() => navigator.NavigateTo<ReadyViewModel>(x => x.Category = Name);
}