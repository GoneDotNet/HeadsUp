using SQLite;

namespace GoneDotNet.HeadsUp.Services;

[Singleton]
public class MySqliteConnection : SQLiteAsyncConnection
{
    public MySqliteConnection(
        ILogger<MySqliteConnection> logger
    ) : base(Path.Combine(FileSystem.AppDataDirectory, "app.db"))
    {
        var conn = this.GetConnection();
        conn.CreateTable<Game>();
        conn.CreateTable<GameAnswer>();

        conn.EnableWriteAheadLogging();
#if DEBUG
        conn.Trace = true;
        conn.Tracer = sql => logger.LogDebug("SQLite Query: " + sql);
#endif
    }


    public AsyncTableQuery<Game> Games => this.Table<Game>();
    public AsyncTableQuery<GameAnswer> GameAnswers => this.Table<GameAnswer>();
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
    public AnswerType? AnswerType { get; set; }
}