namespace PrinterServer.Api.Models;

public sealed class PrintJobRequest
{
    public string Type { get; init; } = "";
    public string Printer { get; init; } = "";
    public string? Text { get; init; }
    public byte[]? FileBytes { get; init; }
    public string? RawMode { get; init; }
    public long Size { get; init; }
    public string ClientIp { get; init; } = "";
}
