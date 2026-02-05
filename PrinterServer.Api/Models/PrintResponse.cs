namespace PrinterServer.Api.Models;

public sealed record PrintResponse(string JobId, string Status, string Printer);
