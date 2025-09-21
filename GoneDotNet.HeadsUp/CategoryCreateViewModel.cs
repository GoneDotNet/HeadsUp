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
    
    [ObservableProperty] string description;

    
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
            await navigator.Alert(
                "Created", 
                $"Category '{this.Name}' created"
            );
            await navigator.GoBack();
        }
    }

    bool CanAdd() => !String.IsNullOrWhiteSpace(this.Name);
}