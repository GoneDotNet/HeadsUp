namespace GoneDotNet.HeadsUp.Services;


public interface ICategoryRespository
{
    Task<List<GameCategory>> GetAll();
    Task<bool> Create(string value, string description);
    Task Remove(int id);
}

public record GameCategory(int Id, string Name, string Description);