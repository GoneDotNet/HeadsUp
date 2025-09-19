namespace GoneDotNet.HeadsUp;

[ShellMap<ScorePage>]
public partial class ScoreViewModel(
    INavigator navigator
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string videoUrl;
    
    
    public void OnAppearing()
    {
    }

    public void OnDisappearing()
    {
    }
}