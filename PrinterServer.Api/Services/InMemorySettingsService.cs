using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public sealed class InMemorySettingsService : ISettingsService
{
    private readonly object _lock = new();
    private Settings _settings = new();

    public Settings GetSettings()
    {
        lock (_lock)
        {
            return Clone(_settings);
        }
    }

    public Settings UpdateSettings(Settings settings)
    {
        lock (_lock)
        {
            _settings = Clone(settings);
            return Clone(_settings);
        }
    }

    private static Settings Clone(Settings source)
    {
        return new Settings
        {
            DefaultPrinter = source.DefaultPrinter,
            RawMode = source.RawMode,
            RawTcpHost = source.RawTcpHost,
            RawTcpPort = source.RawTcpPort,
            RawEncoding = source.RawEncoding,
            RawTerminator = source.RawTerminator,
            AllowLocalNoToken = source.AllowLocalNoToken,
            MaxFileSizeMb = source.MaxFileSizeMb
        };
    }
}
