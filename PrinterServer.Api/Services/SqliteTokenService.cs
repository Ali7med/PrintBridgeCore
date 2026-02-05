using PrinterServer.Api.Storage;

namespace PrinterServer.Api.Services;

public sealed class SqliteTokenService : ITokenService
{
    private readonly SqliteDatabase _database;

    public SqliteTokenService(SqliteDatabase database)
    {
        _database = database;
    }

    public string GenerateToken()
    {
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE Settings
SET Token = $token
WHERE Id = 1;
""";
        command.Parameters.AddWithValue("$token", token);
        command.ExecuteNonQuery();
        return token;
    }

    public bool DisableToken()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE Settings
SET Token = NULL
WHERE Id = 1;
""";
        command.ExecuteNonQuery();
        return true;
    }

    public string? GetToken()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT Token FROM Settings WHERE Id = 1;
""";
        var result = command.ExecuteScalar();
        return result == DBNull.Value ? null : result as string;
    }
}
