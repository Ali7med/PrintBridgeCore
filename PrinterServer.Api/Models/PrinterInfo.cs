namespace PrinterServer.Api.Models;

public sealed record PrinterInfo(string Name, bool IsDefault, bool SupportsRaw);
