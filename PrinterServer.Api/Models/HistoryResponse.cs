namespace PrinterServer.Api.Models;

public sealed record HistoryResponse(IReadOnlyList<HistoryItem> Items);
