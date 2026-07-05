using Microsoft.AspNetCore.Mvc;
using CoilManager.Shared.Responses;

namespace CoilManager.API.Controllers;

[Route("api/[controller]")]
public sealed class HealthController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Get()
    {
        return Success<object>(new
        {
            Application = "CoilManager",
            Status = "Healthy",
            TimestampUtc = DateTimeOffset.UtcNow
        });
    }
}
