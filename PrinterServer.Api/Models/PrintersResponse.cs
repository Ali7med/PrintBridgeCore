namespace PrinterServer.Api.Models;

public sealed record PrintersResponse(IReadOnlyList<PrinterInfo> Printers);
