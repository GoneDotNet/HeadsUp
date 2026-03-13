namespace GoneDotNet.HeadsUp;


[ShellMap<ReadyPage>]
public partial class ReadyViewModel(
    INavigator navigator, 
    IDialogs dialogs,
    IGameService gameService,
    IAnswerProvider answerProvider,
    ICategoryRespository categoryRepository,
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
            var cat = await categoryRepository.GetByName(this.Category);
            ProvidedAnswer[] answers;

            if (cat != null && cat.Answers.Count > 0)
            {
                // use stored answers in random order
                answers = cat.Answers
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(Constants.MaxAnswersPerGame)
                    .ToArray();
            }
            else
            {
                // fallback for legacy/seed categories without stored answers - generate and save
                answers = await answerProvider.GenerateAnswers(
                    this.Category,
                    Constants.MaxAnswersPerCategory,
                    CancellationToken.None
                );
                await categoryRepository.SaveAnswers(this.Category, answers.ToList());

                answers = answers
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(Constants.MaxAnswersPerGame)
                    .ToArray();
            }

            gameService.StartGame(this.Category, answers);

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
            await dialogs.Alert("Error", $"An error occurred while starting the game.  Make sure you are connected to the internet");
            await navigator.GoBack();
        }
    }

    public void OnDisappearing()
    {
    }
}