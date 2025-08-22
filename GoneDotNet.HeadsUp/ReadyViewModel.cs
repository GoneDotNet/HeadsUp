using Microsoft.Extensions.AI;

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
            var questions = await answerProvider.GenerateAnswers(this.Category, Constants.MaxQuestionsPerGame, CancellationToken.None);
            gameContext.StartGame(this.Category, questions);
            
            // TODO: countdown to start - 3 seconds
            // TODO: navigate to game page
        }
        catch (Exception ex)
        {
            
        }
    }

    public void OnDisappearing()
    {
    }
}