using System.Net;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Middleware;

public sealed class TokenAuthMiddleware
{
    private const string TokenHeaderName = "X-Print-Token";
    private readonly RequestDelegate _next;

    public TokenAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISettingsService settingsService, ITokenService tokenService)
    {
        var settings = settingsService.GetSettings();
        var isLocal = IsLocalRequest(context);

        if (isLocal && settings.AllowLocalNoToken)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(TokenHeaderName, out var providedToken))
        {
            await Reject(context, "Missing token.");
            return;
        }

        var storedToken = tokenService.GetToken();
        if (string.IsNullOrWhiteSpace(storedToken))
        {
            await Reject(context, "Token is not configured.");
            return;
        }

        if (!string.Equals(storedToken, providedToken.ToString(), StringComparison.Ordinal))
        {
            await Reject(context, "Invalid token.");
            return;
        }

        await _next(context);
    }

    private static bool IsLocalRequest(HttpContext context)
    {
        var connection = context.Connection;
        if (connection.RemoteIpAddress is null)
        {
            return true;
        }

        if (IPAddress.IsLoopback(connection.RemoteIpAddress))
        {
            return true;
        }

        if (connection.LocalIpAddress is not null)
        {
            return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
        }

        return false;
    }

    private static Task Reject(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync($"{{\"error\":\"{message}\"}}");
    }
}
