using PrinterServer.Api.Models;

namespace PrinterServer.Api.Services;

public interface ISettingsService
{
    Settings GetSettings();
    Settings UpdateSettings(Settings settings);
}
