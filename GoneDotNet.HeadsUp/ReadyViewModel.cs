namespace GoneDotNet.HeadsUp;


[ShellMap<ReadyPage>]
public partial class ReadyViewModel(
    INavigator navigator, 
    IGameContext gameContext,
    IAnswerProvider answerProvider
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string category;
    [ObservableProperty] int countdown;


    public async void OnAppearing()
    {
        var questions = await answerProvider.GenerateAnswers(this.Category, Constants.MaxAnswersPerGame, CancellationToken.None);
        gameContext.StartGame(this.Category, questions);
        
        // TODO: countdown to start - 3 seconds
        var count = 5;
        while (count != 0)
        {
            await Task.Delay(1000);
            count--;
            this.Countdown = count;
        }
        await navigator.NavigateTo<GameViewModel>();
    }

    public void OnDisappearing()
    {
    }
}