namespace PrinterServer.Api.Models;

public sealed class PrintJob
{
    public string JobId { get; init; } = "";
    public string Type { get; init; } = "";
    public string Printer { get; init; } = "";
    public string? Text { get; init; }
    public byte[]? FileBytes { get; init; }
    public string RawMode { get; init; } = "winspool";
    public long Size { get; init; }
    public string ClientIp { get; init; } = "";
}
