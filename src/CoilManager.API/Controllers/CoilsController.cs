using CoilManager.Application.DTOs.Coils;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Responses;
using CoilManager.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/coils")]
public sealed class CoilsController(ICoilService coilService) : BaseApiController
{
    [HttpGet("search/{*value}")]
    public async Task<ActionResult<ApiResponse<CoilSearchResultDto>>> Search(string value, CancellationToken cancellationToken)
    {
        Result<CoilSearchResultDto> result = await coilService.SearchAsync(value, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<CoilSearchResultDto>(result.Error);
    }

    [HttpGet("{coilNumber}/traceability")]
    public async Task<ActionResult<ApiResponse<CoilTraceabilityDto>>> Traceability(string coilNumber, CancellationToken cancellationToken)
    {
        Result<CoilTraceabilityDto> result = await coilService.GetTraceabilityAsync(coilNumber, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<CoilTraceabilityDto>(result.Error);
    }

    [HttpGet("{coilNumber}/inventory-transactions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryTransactionDto>>>> Transactions(string coilNumber, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<InventoryTransactionDto>> result = await coilService.GetInventoryTransactionsAsync(coilNumber, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<IReadOnlyList<InventoryTransactionDto>>(result.Error);
    }

    private ObjectResult ToFailure<T>(Error error) => Failure<T>(error.Code switch
    {
        "Validation" => 400, "NotFound" => 404, "Conflict" => 409, _ => 500
    }, error.Message, [error.Message]);
}
