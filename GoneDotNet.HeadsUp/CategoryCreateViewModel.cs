namespace GoneDotNet.HeadsUp;


[ShellMap<CategoryCreatePage>]
public partial class CategoryCreateViewModel(
    INavigator navigator,
    ICategoryRespository repo
) : ObservableObject
{
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    string name;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    string description;

    
    [RelayCommand(CanExecute = nameof(this.CanAdd))]
    async Task Add()
    {
        var created = await repo.Create(this.Name, this.Description);
        if (!created)
        {
            await navigator.Alert("Error", "Could not create category, it may already exist.");
        }
        else
        {
            await navigator.GoBack();
            await navigator.Alert(
                "Created", 
                $"Category '{this.Name}' created"
            );
        }
    }
    
    bool CanAdd() => !String.IsNullOrWhiteSpace(this.Name) && 
                     !String.IsNullOrWhiteSpace(this.Description);
}