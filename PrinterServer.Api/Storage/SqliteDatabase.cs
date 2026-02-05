using Microsoft.Data.Sqlite;

namespace PrinterServer.Api.Storage;

public sealed class SqliteDatabase
{
    private readonly string _connectionString;

    public SqliteDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void EnsureCreated()
    {
        using var connection = OpenConnection();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
CREATE TABLE IF NOT EXISTS Settings (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),
    DefaultPrinter TEXT NULL,
    RawMode TEXT NOT NULL,
    RawTcpHost TEXT NULL,
    RawTcpPort INTEGER NOT NULL,
    RawEncoding TEXT NOT NULL,
    RawTerminator TEXT NOT NULL,
    AllowLocalNoToken INTEGER NOT NULL,
    MaxFileSizeMb INTEGER NOT NULL,
    Token TEXT NULL
);

CREATE TABLE IF NOT EXISTS History (
    JobId TEXT PRIMARY KEY,
    Time TEXT NOT NULL,
    Type TEXT NOT NULL,
    Printer TEXT NOT NULL,
    Status TEXT NOT NULL,
    Size INTEGER NOT NULL,
    ClientIp TEXT NOT NULL,
    Error TEXT NULL
);

CREATE INDEX IF NOT EXISTS IX_History_Time ON History(Time);
""";
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
INSERT OR IGNORE INTO Settings
(Id, RawMode, RawTcpPort, RawEncoding, RawTerminator, AllowLocalNoToken, MaxFileSizeMb)
VALUES (1, 'winspool', 9100, 'utf-8', '^XZ', 1, 20);
""";
            command.ExecuteNonQuery();
        }
    }
}
