using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IPrintService
{
    PrintResponse EnqueuePrint(PrintJobRequest request);
}
