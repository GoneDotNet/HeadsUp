namespace GoneDotNet.HeadsUp;

[ShellMap<ScorePage>]
public partial class ScoreViewModel(
    INavigator navigator,
    IBeepService beeper,
    IGameService gameService,
    IFileSystem fileSystem
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string? videoUrl;
    [ObservableProperty] string? category;
    [ObservableProperty] DateTimeOffset createdAt;
    [ObservableProperty] int correctAnswers;

    [ShellProperty] public Guid GameId { get; set; }
    [ShellProperty] public bool IsFromGame { get; set; }
    [RelayCommand] Task Back() => this.IsFromGame 
        ? navigator.PopToRoot() 
        : navigator.GoBack();
    
    public async void OnAppearing()
    {
        beeper.SetThemeVolume(0.5f);
        var game = await gameService.GetGameResult(this.GameId);

        this.Category = game.Category;
        this.CreatedAt = game.CreatedAt;
        this.CorrectAnswers = game.Answers.Count(y => y.AnswerType == AnswerType.Success);
        
        this.VideoUrl = Path.Combine(fileSystem.AppDataDirectory, game.GameId + ".mp4");
    }

    
    public void OnDisappearing()
    {
        beeper.SetThemeVolume(1.0f);
    }
}