namespace GoneDotNet.HeadsUp;


[ShellMap<CategoryCreatePage>]
public partial class CategoryCreateViewModel(
    INavigator navigator,
    IDialogs dialogs,
    ICategoryRespository repo,
    IAnswerProvider answerProvider,
    IConnectivity connectivity
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    string name;
    
    [ObservableProperty] string description;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    public bool IsNotBusy => !this.IsBusy;

    [ObservableProperty] string busyMessage;


    [RelayCommand(CanExecute = nameof(CanAdd))]
    async Task Add()
    {
        if (connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await dialogs.Alert("No Connection", "An internet connection is required to generate answers for the category.");
            return;
        }

        this.IsBusy = true;
        this.BusyMessage = "Generating answers with AI...";
        try
        {
            var answers = await answerProvider.GenerateAnswers(
                this.Name,
                Constants.MaxAnswersPerCategory,
                CancellationToken.None
            );

            this.BusyMessage = "Saving category...";
            var created = await repo.Create(this.Name, this.Description, answers.ToList());
            if (!created)
            {
                await dialogs.Alert("Error", "Could not create category, it may already exist.");
            }
            else
            {
                await dialogs.Alert(
                    "Created", 
                    $"Category '{this.Name}' created with {answers.Length} answers!"
                );
                await navigator.GoBack();
            }
        }
        catch (Exception)
        {
            await dialogs.Alert("Error", "Failed to generate answers. Please check your internet connection and try again.");
        }
        finally
        {
            this.IsBusy = false;
            this.BusyMessage = string.Empty;
        }
    }

    bool CanAdd() => !String.IsNullOrWhiteSpace(this.Name) && !this.IsBusy;
}