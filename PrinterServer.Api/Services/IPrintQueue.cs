using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IPrintQueue
{
    ValueTask EnqueueAsync(PrintJob job, CancellationToken cancellationToken = default);
}
