namespace PrinterServer.Api.Models;

public sealed record HistoryItem(
    string JobId,
    DateTimeOffset Time,
    string Type,
    string Printer,
    string Status,
    long Size,
    string ClientIp,
    string? Error
);
