namespace PrinterServer.Api.Services;

public interface ITokenService
{
    string GenerateToken();
    bool DisableToken();
    string? GetToken();
}
