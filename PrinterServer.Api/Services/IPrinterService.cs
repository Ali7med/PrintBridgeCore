using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface IPrinterService
{
    IReadOnlyList<PrinterInfo> GetPrinters();
}
