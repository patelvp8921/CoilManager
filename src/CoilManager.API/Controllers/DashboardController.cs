using CoilManager.Application.DTOs.Dashboard;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/dashboard")]
public sealed class DashboardController(IOperationsDashboardService operationsDashboardService) : BaseApiController
{
    [HttpGet("operations")]
    [ProducesResponseType(typeof(ApiResponse<OperationsDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OperationsDashboardDto>>> GetOperations(CancellationToken cancellationToken)
    {
        OperationsDashboardDto dashboard = await operationsDashboardService.GetOperationsDashboardAsync(cancellationToken);

        return Success(dashboard);
    }
}
