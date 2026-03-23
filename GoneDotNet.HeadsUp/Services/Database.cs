using System.Text.Json.Serialization;
using Shiny.DocumentDb;
using Shiny.DocumentDb.Sqlite;

namespace GoneDotNet.HeadsUp.Services.Impl;


public static class Database
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDocumentStore(opts =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
            opts.DatabaseProvider = new SqliteDatabaseProvider($"Data Source={dbPath}");
            opts.JsonSerializerOptions = AppJsonContext.Default.Options;
            opts.UseReflectionFallback = false;
        });
        return services;
    }
}

[JsonSerializable(typeof(Game))]
[JsonSerializable(typeof(GameCategory))]
[JsonSerializable(typeof(List<GameCategory>))]
[JsonSerializable(typeof(List<ProvidedAnswer>))]
internal partial class AppJsonContext : JsonSerializerContext;

public class Game
{
    public Guid Id { get; set; }
    public string Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<GameAnswer> Answers { get; set; } = new();
}

public class GameAnswer
{
    public string Value { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public AnswerType? AnswerType { get; set; }
}

public class GameCategory
{
    public int Id { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public List<ProvidedAnswer> Answers { get; set; } = new();
}