using CarPlay;
using Foundation;
using GoneDotNet.HeadsUp.Services;

namespace GoneDotNet.HeadsUp;

public class CarPlayCategoryListScreen
{
    readonly CPInterfaceController controller;
    readonly ICategoryRespository categoryRepo;
    readonly CarPlaySceneDelegate navigator;

    public CPListTemplate Template { get; }

    public CarPlayCategoryListScreen(
        CPInterfaceController controller,
        ICategoryRespository categoryRepo,
        CarPlaySceneDelegate navigator)
    {
        this.controller = controller;
        this.categoryRepo = categoryRepo;
        this.navigator = navigator;

        Template = new CPListTemplate("🎯 Manage Categories", Array.Empty<CPListSection>());
        _ = LoadCategories();
    }

    async Task LoadCategories()
    {
        var cats = await categoryRepo.GetAll();

        var items = new List<ICPListTemplateItem>();
        foreach (var c in cats)
        {
            var cat = c;
            var item = new CPListItem(cat.Name, cat.Description);
            item.Handler = (_, completion) =>
            {
                navigator.ShowConfirm(
                    "Delete Category",
                    $"Are you sure you want to delete the category '{cat.Name}'?",
                    confirmed =>
                    {
                        if (confirmed)
                            Task.Run(() => DeleteAndRefresh(cat));
                    }
                );
                completion();
            };
            items.Add(item);
        }

        if (items.Count == 0)
        {
            var emptyItem = new CPListItem("No categories", "Create categories from your phone");
            Template.UpdateSections(new[] { new CPListSection(new ICPListTemplateItem[] { emptyItem }) });
        }
        else
        {
            var section = new CPListSection(items.ToArray(), "Tap to delete", null);
            Template.UpdateSections(new[] { section });
        }
    }

    async Task DeleteAndRefresh(GameCategory category)
    {
        await categoryRepo.Remove(category.Id);
        navigator.ShowAlert("Done", $"Category '{category.Name}' deleted.");
        await LoadCategories();
    }
}
