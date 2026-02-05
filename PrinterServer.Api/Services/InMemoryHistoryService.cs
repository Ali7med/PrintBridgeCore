using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public sealed class InMemoryHistoryService : IHistoryService
{
    private readonly List<HistoryItem> _items = new();
    private readonly object _lock = new();

    public void Add(HistoryItem item)
    {
        lock (_lock)
        {
            _items.Insert(0, item);
            if (_items.Count > 5000)
            {
                _items.RemoveRange(5000, _items.Count - 5000);
            }
        }
    }

    public void UpdateStatus(string jobId, string status, string? error)
    {
        lock (_lock)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (!string.Equals(_items[i].JobId, jobId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var item = _items[i];
                _items[i] = item with { Status = status, Error = error };
                break;
            }
        }
    }

    public IReadOnlyList<HistoryItem> Query(string? status, string? printer, DateTimeOffset? from, DateTimeOffset? to, int limit)
    {
        lock (_lock)
        {
            IEnumerable<HistoryItem> query = _items;

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(item => string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(printer))
            {
                query = query.Where(item => string.Equals(item.Printer, printer, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
            {
                query = query.Where(item => item.Time >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(item => item.Time <= to.Value);
            }

            return query.Take(Math.Clamp(limit, 1, 500)).ToList();
        }
    }
}
