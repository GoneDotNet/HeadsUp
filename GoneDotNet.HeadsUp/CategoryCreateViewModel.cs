namespace GoneDotNet.HeadsUp;


[ShellMap<CategoryCreatePage>]
public partial class CategoryCreateViewModel(
    INavigator navigator,
    IDialogs dialogs,
    ICategoryRespository repo
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    string name;
    
    [ObservableProperty] string description;

    
    [RelayCommand(CanExecute = nameof(CanAdd))]
    async Task Add()
    {
        var created = await repo.Create(this.Name, this.Description);
        if (!created)
        {
            await dialogs.Alert("Error", "Could not create category, it may already exist.");
        }
        else
        {
            await dialogs.Alert(
                "Created", 
                $"Category '{this.Name}' created"
            );
            await navigator.GoBack();
        }
    }

    bool CanAdd() => !String.IsNullOrWhiteSpace(this.Name);
}