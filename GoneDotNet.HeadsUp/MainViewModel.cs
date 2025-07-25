namespace GoneDotNet.HeadsUp;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(INavigator navigator) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] GameCategory[] categories;

    public void OnAppearing()
    {
        this.Categories =
        [
            new GameCategory(navigator, "Disney Princesses", "A collection of games featuring Disney princesses."),
        ];
    }

    public void OnDisappearing()
    {
    }
}

public partial class GameCategory(INavigator Navigator, string name, string description) : ObservableObject
{
    public string Name => name;
    public string Description => description;
    
    [RelayCommand]
    Task NavToGame() => Navigator.NavigateTo<ReadyViewModel>(x => x.Category = Name);
}