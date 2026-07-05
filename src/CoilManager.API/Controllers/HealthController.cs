using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            Application = "CoilManager",
            Status = "Healthy",
            TimestampUtc = DateTimeOffset.UtcNow
        });
    }
}
