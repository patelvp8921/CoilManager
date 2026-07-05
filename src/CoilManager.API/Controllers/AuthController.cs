using CoilManager.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/[controller]")]
public sealed class AuthController : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Failure<object>(
            StatusCodes.Status501NotImplemented,
            "Authentication skeleton is configured. Login implementation is deferred.");
    }
}
