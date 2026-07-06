using CoilManager.Application.DTOs.Lookups;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/lookups")]
public sealed class LookupsController(ILookupService lookupService) : BaseApiController
{
    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupItemDto>>>> GetSuppliers(CancellationToken cancellationToken)
    {
        IReadOnlyList<LookupItemDto> suppliers = await lookupService.GetActiveSuppliersAsync(cancellationToken);

        return Success(suppliers);
    }

    [HttpGet("manufacturers")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupItemDto>>>> GetManufacturers(CancellationToken cancellationToken)
    {
        IReadOnlyList<LookupItemDto> manufacturers = await lookupService.GetActiveManufacturersAsync(cancellationToken);

        return Success(manufacturers);
    }

    [HttpGet("grades")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupItemDto>>>> GetGrades(CancellationToken cancellationToken)
    {
        IReadOnlyList<LookupItemDto> grades = await lookupService.GetActiveGradesAsync(cancellationToken);

        return Success(grades);
    }
}
