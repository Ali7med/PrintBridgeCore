using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public sealed class InMemoryPrintService : IPrintService
{
    private readonly ISettingsService _settingsService;
    private readonly IPrinterService _printerService;
    private readonly IHistoryService _historyService;
    private readonly IPrintQueue _printQueue;

    public InMemoryPrintService(
        ISettingsService settingsService,
        IPrinterService printerService,
        IHistoryService historyService,
        IPrintQueue printQueue)
    {
        _settingsService = settingsService;
        _printerService = printerService;
        _historyService = historyService;
        _printQueue = printQueue;
    }

    public PrintResponse EnqueuePrint(PrintJobRequest request)
    {
        var settings = _settingsService.GetSettings();
        var resolvedPrinter = ResolvePrinter(request.Printer, settings);
        var rawMode = string.IsNullOrWhiteSpace(request.RawMode) ? settings.RawMode : request.RawMode;

        var jobId = Guid.NewGuid().ToString("N");
        var status = "queued";

        var job = new PrintJob
        {
            JobId = jobId,
            Type = request.Type,
            Printer = resolvedPrinter,
            Text = request.Text,
            FileBytes = request.FileBytes,
            RawMode = rawMode,
            Size = request.Size,
            ClientIp = request.ClientIp
        };

        _historyService.Add(new HistoryItem(
            jobId,
            DateTimeOffset.UtcNow,
            request.Type,
            resolvedPrinter,
            status,
            request.Size,
            request.ClientIp,
            null));

        _ = _printQueue.EnqueueAsync(job);
        return new PrintResponse(jobId, status, resolvedPrinter);
    }

    private string ResolvePrinter(string? printer, Settings settings)
    {
        if (!string.IsNullOrWhiteSpace(printer))
        {
            return printer;
        }

        if (!string.IsNullOrWhiteSpace(settings.DefaultPrinter))
        {
            return settings.DefaultPrinter;
        }

        var printers = _printerService.GetPrinters();
        var defaultPrinter = printers.FirstOrDefault(p => p.IsDefault)?.Name;
        return defaultPrinter ?? "";
    }
}
