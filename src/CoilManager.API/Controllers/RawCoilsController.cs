using CoilManager.Application.DTOs.RawCoils;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[ApiController]
[Route("api/rawcoils")]
public sealed class RawCoilsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetAll()
    {
        return Placeholder();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetById(Guid id)
    {
        return Placeholder(id);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Create([FromBody] CreateRawCoilRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Placeholder();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Update(Guid id, [FromBody] UpdateRawCoilRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Placeholder(id);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Delete(Guid id)
    {
        return Placeholder(id);
    }

    private ObjectResult Placeholder(Guid? id = null)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            Message = "Raw Coil backend foundation is configured. Service implementation is deferred.",
            Id = id
        });
    }
}
