using System.Security.Cryptography;

namespace PrinterServer.Api.Services;

public sealed class InMemoryTokenService : ITokenService
{
    private readonly object _lock = new();
    private string? _token;

    public string GenerateToken()
    {
        lock (_lock)
        {
            _token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            return _token;
        }
    }

    public bool DisableToken()
    {
        lock (_lock)
        {
            _token = null;
            return true;
        }
    }

    public string? GetToken()
    {
        lock (_lock)
        {
            return _token;
        }
    }
}
