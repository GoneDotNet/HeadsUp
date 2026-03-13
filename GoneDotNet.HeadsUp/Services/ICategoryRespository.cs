namespace GoneDotNet.HeadsUp.Services;


public interface ICategoryRespository
{
    Task<List<GameCategory>> GetAll();
    Task<GameCategory?> GetByName(string name);
    Task<bool> Create(string value, string description, List<ProvidedAnswer> answers);
    Task SaveAnswers(string categoryName, List<ProvidedAnswer> answers);
    Task SeedDefaults();
    Task Remove(int id);
}

public record GameCategory(int Id, string Name, string Description, List<ProvidedAnswer> Answers);