using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/rawcoils")]
public sealed class RawCoilsController(IRawCoilService rawCoilService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<RawCoilDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiPagedResponse<RawCoilDto>>> GetAll(
        [FromQuery] RawCoilQueryRequest request,
        CancellationToken cancellationToken)
    {
        PagedResult<RawCoilDto> result = await rawCoilService.GetAsync(request, cancellationToken);

        return Paged(
            result.Items,
            new PaginationResult(result.PageNumber, result.PageSize, result.TotalCount));
    }

    [HttpGet("next-coil-id")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetNextCoilId(CancellationToken cancellationToken)
    {
        string nextCoilId = await rawCoilService.GetNextRawCoilNumberAsync(cancellationToken);

        return Success(nextCoilId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RawCoilDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<RawCoilDto> result = await rawCoilService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value)
            : ToFailure<RawCoilDto>(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RawCoilDto>>> Create(
        [FromBody] CreateRawCoilRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<RawCoilDto> result = await rawCoilService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return ToFailure<RawCoilDto>(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            ApiResponse<RawCoilDto>.Ok(result.Value, "Raw coil created successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<RawCoilDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RawCoilDto>>> Update(
        Guid id,
        [FromBody] UpdateRawCoilRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<RawCoilDto> result = await rawCoilService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Raw coil updated successfully.")
            : ToFailure<RawCoilDto>(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result result = await rawCoilService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success<object>(null, "Raw coil deleted successfully.")
            : ToFailure<object>(result.Error);
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
