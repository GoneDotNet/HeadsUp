using System.Text.Json.Serialization;
using Shiny.SqliteDocumentDb;

namespace GoneDotNet.HeadsUp.Services.Impl;


public static class Database
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddSqliteDocumentStore(opts =>
        {
            opts.ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "app.db3")}";
            opts.JsonSerializerOptions = AppJsonContext.Default.Options;
            opts.UseReflectionFallback = false;
        });
        return services;
    }
}

[JsonSerializable(typeof(Game))]
[JsonSerializable(typeof(GameCategory))]
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
}