using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IHistoryService
{
    void Add(HistoryItem item);
    void UpdateStatus(string jobId, string status, string? error);
    IReadOnlyList<HistoryItem> Query(string? status, string? printer, DateTimeOffset? from, DateTimeOffset? to, int limit);
}
