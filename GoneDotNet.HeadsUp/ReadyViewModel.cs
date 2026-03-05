namespace GoneDotNet.HeadsUp;


[ShellMap<ReadyPage>]
public partial class ReadyViewModel(
    INavigator navigator, 
    IGameService gameService,
    IAnswerProvider answerProvider,
    IBeepService beeper,
    ILogger<ReadyViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] string category;
    [ObservableProperty] int countdown = 5;


    public async void OnAppearing()
    {
        try
        {
            var questions =
                await answerProvider.GenerateAnswers(this.Category, Constants.MaxAnswersPerGame,
                    CancellationToken.None);
            gameService.StartGame(this.Category, questions);

            beeper.SetThemeVolume(0.5f);
            var count = 5;
            while (count != 0)
            {
                await Task.Delay(1000);
                count--;
                this.Countdown = count;
                beeper.Countdown();
            }

            await navigator.NavigateTo<GameViewModel>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting game");
            await navigator.Alert("Error", $"An error occurred while starting the game.  Make sure you are connected to the internet");
            await navigator.GoBack();
        }
    }

    public void OnDisappearing()
    {
    }
}