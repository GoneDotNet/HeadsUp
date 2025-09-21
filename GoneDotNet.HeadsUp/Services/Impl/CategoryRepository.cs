using Dto = GoneDotNet.HeadsUp.Services.GameCategory;
using Data = GoneDotNet.HeadsUp.Services.Impl.GameCategory;


namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class CategoryRepository(MySqliteConnection data) : ICategoryRespository
{
    public async Task<List<Dto>> GetAll()
    {
        var categories = await data.Categories
            .OrderBy(x => x.Value)
            .ToListAsync();
        
        return categories.Select(x => new Dto(x.Id, x.Value, x.Description)).ToList();
    }
    

    public async Task<bool> Create(string value, string description)
    {
        try
        {
            await data.InsertAsync(new Data
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
    

    public async Task Remove(int id)
    {
        var cat = await data.GetAsync<Data>(id);
        if (cat != null)
            await data.DeleteAsync(cat);
    }
}