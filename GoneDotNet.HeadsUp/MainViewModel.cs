namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    INavigator navigator,
    ICategoryRespository repository,
    IBeepService beeper
) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToScoreList() => navigator.NavigateToScoreList();
    [ObservableProperty] GameCategoryViewModel[] categories;

    public async void OnAppearing()
    {
        beeper.PlayThemeSong();
        beeper.SetThemeVolume(1.0f);

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
    Task NavToGame() => navigator.NavigateTo<ReadyViewModel>(x => x.Category = Name);
}