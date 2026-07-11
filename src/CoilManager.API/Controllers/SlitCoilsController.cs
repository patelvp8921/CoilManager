using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/slit-coils")]
public sealed class SlitCoilsController(ISlitCoilService slitCoilService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<SlitCoilDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiPagedResponse<SlitCoilDto>>> GetAll(
        [FromQuery] SlitCoilQueryRequest request,
        CancellationToken cancellationToken)
    {
        PagedResult<SlitCoilDto> result = await slitCoilService.GetAsync(request, cancellationToken);
        return Paged(result.Items, new PaginationResult(result.PageNumber, result.PageSize, result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilDto> result = await slitCoilService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilDto>(result.Error);
    }

    [HttpGet("by-number/{coilNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilDto>>> GetByNumber(string coilNumber, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilDto> result = await slitCoilService.GetByNumberAsync(coilNumber, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilDto>(result.Error);
    }

    [HttpGet("{id:guid}/genealogy")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilGenealogyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilGenealogyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilGenealogyDto>>> GetGenealogy(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilGenealogyDto> result = await slitCoilService.GetGenealogyAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilGenealogyDto>(result.Error);
    }

    private ObjectResult ToFailure<T>(Error error)
    {
        int statusCode = error.Code switch
        {
            "Validation" => StatusCodes.Status400BadRequest,
            "NotFound" => StatusCodes.Status404NotFound,
            "Conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Failure<T>(statusCode, error.Message, [error.Message]);
    }
}
