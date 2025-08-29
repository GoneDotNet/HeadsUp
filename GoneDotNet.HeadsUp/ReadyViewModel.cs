namespace GoneDotNet.HeadsUp;


[ShellMap<ReadyPage>]
public partial class ReadyViewModel(
    INavigator navigator, 
    IGameContext gameContext,
    IAnswerProvider answerProvider
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string category;


    public async void OnAppearing()
    {
        try
        {
            // TODO: loading indicator
            var questions = await answerProvider.GenerateAnswers(this.Category, Constants.MaxAnswersPerGame, CancellationToken.None);
            gameContext.StartGame(this.Category, questions);
            
            // TODO: countdown to start - 3 seconds
            await navigator.NavigateTo<GameViewModel>();
        }
        catch (Exception ex)
        {
            
        }
    }

    public void OnDisappearing()
    {
    }
}