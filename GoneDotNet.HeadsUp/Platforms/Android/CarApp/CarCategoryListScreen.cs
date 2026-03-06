using AndroidX.Car.App;
using AndroidX.Car.App.Model;
using GoneDotNet.HeadsUp.Services;
using Action = AndroidX.Car.App.Model.Action;

namespace GoneDotNet.HeadsUp;

public class CarCategoryListScreen : Screen
{
    IReadOnlyList<GameCategory>? categories;

    public CarCategoryListScreen(CarContext carContext) : base(carContext)
    {
        _ = LoadCategories();
    }

    public override ITemplate OnGetTemplate()
    {
        if (categories == null)
        {
            return new ListTemplate.Builder()
                .SetTitle("⚙️ Categories")
                .SetHeaderAction(Action.Back)
                .SetLoading(true)
                .Build();
        }

        if (categories.Count == 0)
        {
            return new MessageTemplate.Builder("No categories found!")
                .SetTitle("⚙️ Categories")
                .SetHeaderAction(Action.Back)
                .Build();
        }

        var listBuilder = new ItemList.Builder();

        foreach (var cat in categories)
        {
            var c = cat;
            listBuilder.AddItem(
                new Row.Builder()
                    .SetTitle(c.Name)
                    .AddText(c.Description ?? "")
                    .SetOnClickListener(new ActionClickListener(() =>
                    {
                        ConfirmDelete(c);
                    }))
                    .Build()
            );
        }

        return new ListTemplate.Builder()
            .SetTitle("⚙️ Categories")
            .SetHeaderAction(Action.Back)
            .SetSingleList(listBuilder.Build())
            .Build();
    }

    void ConfirmDelete(GameCategory cat)
    {
        ScreenManager.Push(new CarConfirmScreen(CarContext, $"Delete \"{cat.Name}\"?",
            "This will permanently delete this category.", async () =>
            {
                await CarServiceResolver.CategoryRepo.Remove(cat.Id);
                categories = null;
                await LoadCategories();
            }));
    }

    async Task LoadCategories()
    {
        var all = await CarServiceResolver.CategoryRepo.GetAll();
        categories = all.ToList();
        MainThread.BeginInvokeOnMainThread(() => Invalidate());
    }
}
