namespace GoneDotNet.HeadsUp;


[ShellMap<ScoreListPage>]
public partial class ScoreListViewModel(
    INavigator navigator,
    IGameService gameService
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] List<GameResultViewModel> games;
    
    
    public async void OnAppearing()
    {
        var gameResults = await gameService.GetGameResults();

        this.Games = gameResults
            .Select(x => new GameResultViewModel(
                navigator,
                x.GameId,
                x.Category,
                x.CreatedAt,
                x.Answers.Count(y => y.AnswerType == AnswerType.Success)
            ))
            .ToList();
    }

    public void OnDisappearing()
    {
    }
}

public partial class GameResultViewModel(
    INavigator navigator,
    Guid gameId,
    string category,
    DateTimeOffset createdAt,
    int correctAnswers
) : ObservableObject
{
    public string Category => category;
    public DateTimeOffset CreatedAt => createdAt;
    public int CorrectAnswers => correctAnswers;
    
    [RelayCommand]
    Task NavToScore() => navigator.NavigateToScore(gameId);
}