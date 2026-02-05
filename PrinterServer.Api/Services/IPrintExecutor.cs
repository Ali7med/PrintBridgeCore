using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IPrintExecutor
{
    Task ExecuteAsync(PrintJob job, CancellationToken cancellationToken);
}
