using SQLite;

namespace GoneDotNet.HeadsUp.Services.Impl;


[Singleton]
public class MySqliteConnection : SQLiteAsyncConnection
{
    public MySqliteConnection(
        ILogger<MySqliteConnection> logger,
        IFileSystem fileSystem
    ) : base(Path.Combine(fileSystem.AppDataDirectory, "app.db"))
    {
        var conn = this.GetConnection();
        conn.CreateTable<Game>();
        conn.CreateTable<GameAnswer>();
        var result = conn.CreateTable<GameCategory>();

        conn.EnableWriteAheadLogging();
#if DEBUG
        conn.Trace = true;
        conn.Tracer = sql => logger.LogDebug("SQLite Query: " + sql);
#endif
        if (result == CreateTableResult.Created)
        {
            conn.Insert(new GameCategory
            {
                Value = "Disney Princesses",
                Description = "A collection of games featuring Disney princesses."
            });
            conn.Insert(new GameCategory
            {
                Value = "Music - 80s Rock",
                Description = "A collection of popular rock songs from the 1980s."
            });
            conn.Insert(new GameCategory
            {
                Value = "Movies - 90s",
                Description = "A collection of popular movies from the 1990s."
            });
        }
    }


    public AsyncTableQuery<Game> Games => this.Table<Game>();
    public AsyncTableQuery<GameAnswer> GameAnswers => this.Table<GameAnswer>();
    public AsyncTableQuery<GameCategory> Categories => this.Table<GameCategory>();
}

public class Game
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class GameAnswer
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Value { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public AnswerType? AnswerType { get; set; }
}

public class GameCategory
{
    [PrimaryKey]
    [AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public string Value { get; set; }
    public string Description { get; set; }
}