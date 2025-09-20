namespace GoneDotNet.HeadsUp;

[ShellMap<ScorePage>]
public partial class ScoreViewModel(
    INavigator navigator,
    IGameService gameService
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string? videoUrl;
    [ObservableProperty] string? category;
    [ObservableProperty] DateTimeOffset createdAt;
    [ObservableProperty] int correctAnswers;

    [ShellProperty] public Guid GameId { get; set; }
    [RelayCommand] Task Back() => navigator.NavigateTo($"//{nameof(MainPage)}");
    
    public async void OnAppearing()
    {
        var game = await gameService.GetGameResult(this.GameId);

        this.Category = game.Category;
        this.CreatedAt = game.CreatedAt;
        this.CorrectAnswers = game.Answers.Count(y => y.AnswerType == AnswerType.Success);
        
        this.VideoUrl = Path.Combine(FileSystem.AppDataDirectory, game.GameId + ".mp4");
    }

    public void OnDisappearing()
    {
    }
}