namespace PrinterServer.Api.Models;

public sealed class HistoryResponse
{
    public List<HistoryItem> Items { get; set; } = new();
}
