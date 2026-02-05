using System.Threading.Channels;
using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public sealed class PrintQueueService : BackgroundService, IPrintQueue
{
    private readonly Channel<PrintJob> _queue;
    private readonly IPrintExecutor _printExecutor;
    private readonly IHistoryService _historyService;

    public PrintQueueService(IPrintExecutor printExecutor, IHistoryService historyService)
    {
        _printExecutor = printExecutor;
        _historyService = historyService;
        _queue = Channel.CreateUnbounded<PrintJob>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(PrintJob job, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(job, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            _historyService.UpdateStatus(job.JobId, "processing", null);

            try
            {
                await _printExecutor.ExecuteAsync(job, stoppingToken);
                _historyService.UpdateStatus(job.JobId, "success", null);
            }
            catch (Exception ex)
            {
                _historyService.UpdateStatus(job.JobId, "failed", ex.Message);
            }
        }
    }
}
