using Microsoft.Data.Sqlite;
using PrinterServer.Api.Models;
using PrinterServer.Api.Storage;

namespace PrinterServer.Api.Services;

public sealed class SqliteHistoryService : IHistoryService
{
    private readonly SqliteDatabase _database;

    public SqliteHistoryService(SqliteDatabase database)
    {
        _database = database;
    }

    public void Add(HistoryItem item)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
INSERT INTO History (JobId, Time, Type, Printer, Status, Size, ClientIp, Error)
VALUES ($jobId, $time, $type, $printer, $status, $size, $clientIp, $error);
""";
        command.Parameters.AddWithValue("$jobId", item.JobId);
        command.Parameters.AddWithValue("$time", item.Time.ToString("O"));
        command.Parameters.AddWithValue("$type", item.Type);
        command.Parameters.AddWithValue("$printer", item.Printer);
        command.Parameters.AddWithValue("$status", item.Status);
        command.Parameters.AddWithValue("$size", item.Size);
        command.Parameters.AddWithValue("$clientIp", item.ClientIp);
        command.Parameters.AddWithValue("$error", (object?)item.Error ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void UpdateStatus(string jobId, string status, string? error)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE History
SET Status = $status, Error = $error
WHERE JobId = $jobId;
""";
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
        command.Parameters.AddWithValue("$jobId", jobId);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<HistoryItem> Query(string? status, string? printer, DateTimeOffset? from, DateTimeOffset? to, int limit)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(status))
        {
            where.Add("Status = $status");
            command.Parameters.AddWithValue("$status", status);
        }

        if (!string.IsNullOrWhiteSpace(printer))
        {
            where.Add("Printer = $printer");
            command.Parameters.AddWithValue("$printer", printer);
        }

        if (from.HasValue)
        {
            where.Add("Time >= $from");
            command.Parameters.AddWithValue("$from", from.Value.ToString("O"));
        }

        if (to.HasValue)
        {
            where.Add("Time <= $to");
            command.Parameters.AddWithValue("$to", to.Value.ToString("O"));
        }

        var whereClause = where.Count == 0 ? "" : "WHERE " + string.Join(" AND ", where);
        command.CommandText = $"""
SELECT JobId, Time, Type, Printer, Status, Size, ClientIp, Error
FROM History
{whereClause}
ORDER BY Time DESC
LIMIT $limit;
""";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 500));

        using var reader = command.ExecuteReader();
        var items = new List<HistoryItem>();
        while (reader.Read())
        {
            items.Add(new HistoryItem(
                reader.GetString(0),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt64(5),
                reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7)));
        }

        return items;
    }
}
