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
    [ObservableProperty] int score;
    [ObservableProperty] List<AnswerResult> answers = [];
    
    [ShellProperty] public Guid GameId { get; set; }
    
    [ShellProperty] 
    [ObservableProperty]
    public partial bool IsFromGame { get; set; }

    
    [RelayCommand] Task Back() => this.IsFromGame 
        ? navigator.PopToRoot() 
        : navigator.GoBack();
    
    public async void OnAppearing()
    {
        beeper.SetThemeVolume(0.3f);
        var game = await gameService.GetGameResult(this.GameId);

        this.Category = game.Category;
        this.CreatedAt = game.CreatedAt;
        this.Score = game.Answers.Count(y => y.AnswerType == AnswerType.Success);
        this.VideoUrl = Path.Combine(fileSystem.AppDataDirectory, game.GameId + ".mp4");
        
        this.Answers = game.Answers
            .Select(x => new AnswerResult(
                x.Answer, 
                x.AnswerType switch
                {
                    AnswerType.Success => AnswerResultType.Success,
                    AnswerType.Pass => AnswerResultType.Pass,
                    _ => AnswerResultType.Unanswered
                }
            ))
            .ToList();
    }

    
    public void OnDisappearing()
    {
        beeper.SetThemeVolume(1.0f);
    }
}

public record AnswerResult(string Text, AnswerResultType ResultType);

public enum AnswerResultType
{
    Success,
    Pass,
    Unanswered
}