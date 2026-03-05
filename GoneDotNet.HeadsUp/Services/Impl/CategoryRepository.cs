using Shiny.SqliteDocumentDb;
using Dto = GoneDotNet.HeadsUp.Services.GameCategory;
using Data = GoneDotNet.HeadsUp.Services.Impl.GameCategory;


namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class CategoryRepository(IDocumentStore store) : ICategoryRespository
{
    public async Task<List<Dto>> GetAll()
    {
        var categories = await store.Query<Data>()
            .OrderBy(x => x.Value)
            .ToList();

        if (categories.Count == 0)
        {
            await store.Insert(new Data
            {
                Value = "Disney Princesses",
                Description = "A collection of games featuring Disney princesses."
            });

            await store.Insert(new Data
            {
                Value = "Music - 80s Rock",
                Description = "A collection of popular rock songs from the 1980s."
            });
        
            await store.Insert(new Data
            {
                Value = "Movies - 90s",
                Description = "A collection of popular movies from the 1990s."
            });
            
            categories = await store.Query<Data>()
                .OrderBy(x => x.Value)
                .ToList();
        }
        
        return categories.Select(x => new Dto(x.Id, x.Value, x.Description)).ToList();
    }
    

    public async Task<bool> Create(string value, string description)
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
                Description = description ?? String.Empty
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
    

    public Task Remove(int id)
        => store.Remove<Data>(id.ToString());
}