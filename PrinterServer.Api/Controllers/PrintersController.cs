using Microsoft.AspNetCore.Mvc;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Controllers;

[ApiController]
[Route("printers")]
public sealed class PrintersController : ControllerBase
{
    private readonly IPrinterService _printerService;

    public PrintersController(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<PrintersResponse> GetPrinters()
    {
        var printers = _printerService.GetPrinters();
        return Ok(new PrintersResponse(printers));
    }
}
