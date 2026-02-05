using Microsoft.AspNetCore.Mvc;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Controllers;

[ApiController]
[Route("token")]
public sealed class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<TokenResponse> GenerateToken()
    {
        var token = _tokenService.GenerateToken();
        return Ok(new TokenResponse(token));
    }

    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<TokenDisabledResponse> DisableToken()
    {
        var disabled = _tokenService.DisableToken();
        return Ok(new TokenDisabledResponse(disabled));
    }
}
