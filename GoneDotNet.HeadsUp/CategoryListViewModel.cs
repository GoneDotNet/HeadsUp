namespace GoneDotNet.HeadsUp;


[ShellMap<CategoryListPage>]
public partial class CategoryListViewModel(
    INavigator navigator,
    ICategoryRespository repository
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty] bool isBusy;
    [ObservableProperty] List<CategoryItemViewModel> categories;
    [RelayCommand] Task Create() => navigator.NavigateToCategoryCreate();
    [RelayCommand]
    async Task Load()
    {
        this.IsBusy = true;
        var items = await repository.GetAll();
        this.Categories = items
            .Select(x => new CategoryItemViewModel(
                navigator, 
                x,
                async () =>
                {
                    await repository.Remove(x.Id);
                    this.LoadCommand.Execute(null!);
                }
            ))
            .ToList();
        
        this.IsBusy = false;
    }

    public void OnAppearing()
    {
        this.LoadCommand.Execute(null!);
    }

    public void OnDisappearing()
    {
    }
}

public partial class CategoryItemViewModel(
    INavigator navigator, 
    GameCategory category,
    Func<Task> callback
) : ObservableObject
{
    public string Name => category.Name;
    public string Description => category.Description;

    [RelayCommand]
    async Task Delete()
    {
        var confirm = await navigator.Confirm(
            "Delete Category", 
            $"Are you sure you want to delete the category '{Name}'?"
        );
        if (confirm)
        {
            await callback.Invoke();
            await navigator.Alert("Done", $"Category '{Name}' deleted.");
        }
    }
}