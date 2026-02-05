using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IHistoryService
{
    void Add(HistoryItem item);
    void UpdateStatus(string jobId, string status, string? error);
    Task<IEnumerable<HistoryItem>> GetHistoryAsync(string? status, string? printer, DateTime? from, DateTime? to, int limit);
    Task ClearHistoryAsync();
}
