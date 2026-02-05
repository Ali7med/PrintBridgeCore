using Microsoft.AspNetCore.Mvc;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Controllers;

[ApiController]
[Route("settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Settings> GetSettings()
    {
        return Ok(_settingsService.GetSettings());
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Settings> UpdateSettings([FromBody] Settings settings)
    {
        return Ok(_settingsService.UpdateSettings(settings));
    }
}
