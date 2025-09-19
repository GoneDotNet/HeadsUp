namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(INavigator navigator) : ObservableObject, IPageLifecycleAware
{
    [RelayCommand] Task NavToTest() => navigator.NavigateToTest();
    [RelayCommand] Task NavToScoreList() => navigator.NavigateToScoreList();
    [ObservableProperty] GameCategory[] categories;

    public void OnAppearing()
    {
        this.Categories =
        [
            new GameCategory(
                navigator, 
                "Disney Princesses", 
                "A collection of games featuring Disney princesses."
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