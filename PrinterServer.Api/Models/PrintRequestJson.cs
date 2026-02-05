namespace PrinterServer.Api.Models;

public sealed class PrintRequestJson
{
    public string Type { get; init; } = "";
    public string? Printer { get; init; }
    public string? Text { get; init; }
    public string? Base64File { get; init; }
    public string? RawMode { get; init; }
}
