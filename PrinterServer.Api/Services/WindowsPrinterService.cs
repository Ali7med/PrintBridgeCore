using System.Drawing.Printing;
using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public sealed class WindowsPrinterService : IPrinterService
{
    public IReadOnlyList<PrinterInfo> GetPrinters()
    {
        var defaultPrinter = new PrinterSettings().PrinterName;
        var printers = new List<PrinterInfo>();

        foreach (string printerName in PrinterSettings.InstalledPrinters)
        {
            var isDefault = string.Equals(printerName, defaultPrinter, StringComparison.OrdinalIgnoreCase);
            printers.Add(new PrinterInfo(printerName, isDefault, true));
        }

        return printers;
    }
}
