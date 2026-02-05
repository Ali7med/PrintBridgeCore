using Microsoft.AspNetCore.Mvc;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Controllers;

[ApiController]
[Route("history")]
public sealed class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<HistoryResponse> GetHistory(
        [FromQuery] string? status,
        [FromQuery] string? printer,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 100)
    {
        var items = _historyService.Query(status, printer, from, to, limit);
        return Ok(new HistoryResponse(items));
    }
}
