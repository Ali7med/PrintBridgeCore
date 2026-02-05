using Microsoft.AspNetCore.Http;

namespace PrinterServer.Api.Models;

public sealed class PrintRequestForm
{
    public string Type { get; init; } = "";
    public string? Printer { get; init; }
    public string? Text { get; init; }
    public IFormFile? File { get; init; }
    public string? RawMode { get; init; }
}
