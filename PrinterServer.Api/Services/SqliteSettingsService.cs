using Microsoft.Data.Sqlite;
using PrinterServer.Api.Models;
using PrinterServer.Api.Storage;

namespace PrinterServer.Api.Services;

public sealed class SqliteSettingsService : ISettingsService
{
    private readonly SqliteDatabase _database;

    public SqliteSettingsService(SqliteDatabase database)
    {
        _database = database;
    }

    public Settings GetSettings()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT DefaultPrinter, RawMode, RawTcpHost, RawTcpPort, RawEncoding, RawTerminator, AllowLocalNoToken, MaxFileSizeMb
FROM Settings WHERE Id = 1;
""";

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new Settings();
        }

        return new Settings
        {
            DefaultPrinter = reader.IsDBNull(0) ? null : reader.GetString(0),
            RawMode = reader.GetString(1),
            RawTcpHost = reader.IsDBNull(2) ? null : reader.GetString(2),
            RawTcpPort = reader.GetInt32(3),
            RawEncoding = reader.GetString(4),
            RawTerminator = reader.GetString(5),
            AllowLocalNoToken = reader.GetInt32(6) == 1,
            MaxFileSizeMb = reader.GetInt32(7)
        };
    }

    public Settings UpdateSettings(Settings settings)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE Settings
SET DefaultPrinter = $defaultPrinter,
    RawMode = $rawMode,
    RawTcpHost = $rawTcpHost,
    RawTcpPort = $rawTcpPort,
    RawEncoding = $rawEncoding,
    RawTerminator = $rawTerminator,
    AllowLocalNoToken = $allowLocalNoToken,
    MaxFileSizeMb = $maxFileSizeMb
WHERE Id = 1;
""";
        command.Parameters.AddWithValue("$defaultPrinter", (object?)settings.DefaultPrinter ?? DBNull.Value);
        command.Parameters.AddWithValue("$rawMode", settings.RawMode);
        command.Parameters.AddWithValue("$rawTcpHost", (object?)settings.RawTcpHost ?? DBNull.Value);
        command.Parameters.AddWithValue("$rawTcpPort", settings.RawTcpPort);
        command.Parameters.AddWithValue("$rawEncoding", settings.RawEncoding);
        command.Parameters.AddWithValue("$rawTerminator", settings.RawTerminator);
        command.Parameters.AddWithValue("$allowLocalNoToken", settings.AllowLocalNoToken ? 1 : 0);
        command.Parameters.AddWithValue("$maxFileSizeMb", settings.MaxFileSizeMb);
        command.ExecuteNonQuery();

        return GetSettings();
    }
}
