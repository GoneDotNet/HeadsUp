using System.Text.Json;
using Shiny.SqliteDocumentDb;
using Dto = GoneDotNet.HeadsUp.Services.GameCategory;
using Data = GoneDotNet.HeadsUp.Services.Impl.GameCategory;


namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class CategoryRepository(IDocumentStore store, IFileSystem fileSystem) : ICategoryRespository
{
    public async Task<List<Dto>> GetAll()
    {
        var categories = await store.Query<Data>()
            .OrderBy(x => x.Value)
            .ToList();
        
        return categories.Select(x => new Dto(x.Id, x.Value, x.Description, x.Answers ?? [])).ToList();
    }


    public async Task<Dto?> GetByName(string name)
    {
        var categories = await store.Query<Data>()
            .Where(x => x.Value == name)
            .ToList();

        var entity = categories.FirstOrDefault();
        if (entity == null)
            return null;

        return new Dto(entity.Id, entity.Value, entity.Description, entity.Answers ?? []);
    }


    public async Task<bool> Create(string value, string description, List<ProvidedAnswer> answers)
    {
        try
        {
            var existing = await store.Query<Data>()
                .Where(x => x.Value == value)
                .Any();
            
            if (existing)
                return false;
            
            await store.Insert(new Data
            {
                Value = value,
                Description = description ?? String.Empty,
                Answers = answers
            });
            return true;
        }
        catch
        {
            return false;
        }
    }


    public async Task SeedDefaults()
    {
        await using var stream = await fileSystem.OpenAppPackageFileAsync("default_categories.json");
        var defaults = await JsonSerializer.DeserializeAsync(
            stream,
            AppJsonContext.Default.ListGameCategory
        );
        if (defaults == null)
            return;

        foreach (var category in defaults)
        {
            var existing = await store.Query<Data>()
                .Where(x => x.Value == category.Value)
                .Any();

            if (!existing)
                await store.Insert(category);
        }
    }


    public async Task SaveAnswers(string categoryName, List<ProvidedAnswer> answers)
    {
        var categories = await store.Query<Data>()
            .Where(x => x.Value == categoryName)
            .ToList();

        var entity = categories.FirstOrDefault();
        if (entity == null)
            return;

        entity.Answers = answers;
        await store.Update(entity);
    }


    public Task Remove(int id)
        => store.Remove<Data>(id.ToString());
}