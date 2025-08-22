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
        // conn.CreateTable<YourModel>();

        conn.EnableWriteAheadLogging();
#if DEBUG
        conn.Trace = true;
        conn.Tracer = sql => logger.LogDebug("SQLite Query: " + sql);
#endif
    }


    // public AsyncTableQuery<YourModel> Logs => this.Table<YourModel>();
}