using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers.Development;

[ApiController]
[Route("api/development/demo-data")]
public sealed class DemoDataController(IDemoDataSeeder seeder,IHostEnvironment environment) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponse<DemoDataSummary>>> Generate([FromBody]GenerateDemoDataCommand command,CancellationToken token)
    {
        if(!environment.IsDevelopment())return StatusCode(StatusCodes.Status403Forbidden,ApiResponse<DemoDataSummary>.Fail("Demo data generation is available only in Development."));
        var summary=await seeder.GenerateAsync(command,token);
        return Ok(ApiResponse<DemoDataSummary>.Ok(summary,summary.Message));
    }
}