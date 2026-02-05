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
    public async Task<ActionResult<HistoryResponse>> GetHistory(
        [FromQuery] string? status,
        [FromQuery] string? printer,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100)
    {
        var items = await _historyService.GetHistoryAsync(status, printer, from, to, limit);
        return Ok(new HistoryResponse { Items = items.ToList() });
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteHistory()
    {
        await _historyService.ClearHistoryAsync();
        return NoContent();
    }
}
