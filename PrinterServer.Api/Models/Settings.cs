namespace PrinterServer.Api.Models;

public sealed class Settings
{
    public string? DefaultPrinter { get; set; }
    public string RawMode { get; set; } = "winspool";
    public string? RawTcpHost { get; set; }
    public int RawTcpPort { get; set; } = 9100;
    public string RawEncoding { get; set; } = "utf-8";
    public string RawTerminator { get; set; } = "^XZ";
    public bool AllowLocalNoToken { get; set; } = true;
    public int MaxFileSizeMb { get; set; } = 20;
}
