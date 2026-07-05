using CoilManager.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            Message = "Authentication skeleton is configured. Login implementation is deferred."
        });
    }
}
