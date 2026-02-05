using Microsoft.AspNetCore.Mvc;
using PrinterServer.Api.Models;
using PrinterServer.Api.Services;

namespace PrinterServer.Api.Controllers;

[ApiController]
[Route("print")]
public sealed class PrintController : ControllerBase
{
    private readonly IPrintService _printService;

    public PrintController(IPrintService printService)
    {
        _printService = printService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PrintResponse> PrintJson([FromBody] PrintRequestJson request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("type is required");
        }

        byte[]? fileBytes = null;
        if (!string.Equals(request.Type, "text", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(request.Base64File))
            {
                try { fileBytes = Convert.FromBase64String(request.Base64File); }
                catch { return BadRequest("invalid base64 content"); }
            }
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var response = _printService.EnqueuePrint(new PrintJobRequest
        {
            Type = request.Type,
            Printer = (request.Printer ?? string.Empty).Trim(),
            Text = request.Text,
            FileBytes = fileBytes,
            RawMode = request.RawMode ?? "winspool",
            Size = fileBytes?.Length ?? (request.Text?.Length ?? 0),
            ClientIp = clientIp
        });
        return Ok(response);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<PrintResponse> PrintForm([FromForm] PrintRequestForm request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("type is required");
        }

        if (string.Equals(request.Type, "text", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("text is required for type=text");
            }
        }
        else
        {
            if (request.File is null)
            {
                return BadRequest("file is required for non-text types");
            }
        }

        var size = request.File?.Length ?? (request.Text?.Length ?? 0);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var fileBytes = request.File is null ? null : ReadFileBytes(request.File);
        var response = _printService.EnqueuePrint(new PrintJobRequest
        {
            Type = request.Type,
            Printer = (request.Printer ?? string.Empty).Trim(),
            Text = request.Text,
            FileBytes = fileBytes,
            RawMode = request.RawMode ?? "winspool",
            Size = size,
            ClientIp = clientIp
        });
        return Ok(response);
    }

    private static byte[] ReadFileBytes(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
