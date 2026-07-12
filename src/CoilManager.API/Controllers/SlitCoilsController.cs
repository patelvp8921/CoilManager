using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/slit-coils")]
public sealed class SlitCoilsController(ISlitCoilService slitCoilService, ISlitCoilLabelService labelService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<SlitCoilListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiPagedResponse<SlitCoilListItemDto>>> GetAll(
        [FromQuery] SlitCoilQueryRequest request,
        CancellationToken cancellationToken)
    {
        PagedResult<SlitCoilListItemDto> result = await slitCoilService.GetAsync(request, cancellationToken);
        return Paged(result.Items, new PaginationResult(result.PageNumber, result.PageSize, result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilDetailsDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilDetailsDto> result = await slitCoilService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilDetailsDto>(result.Error);
    }

    [HttpGet("by-number/{coilNumber}")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilDetailsDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilDetailsDto>>> GetByNumber(string coilNumber, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilDetailsDto> result = await slitCoilService.GetByNumberAsync(coilNumber, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilDetailsDto>(result.Error);
    }

    [HttpGet("{id:guid}/genealogy")]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilGenealogyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlitCoilGenealogyDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlitCoilGenealogyDto>>> GetGenealogy(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlitCoilGenealogyDto> result = await slitCoilService.GetGenealogyAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilGenealogyDto>(result.Error);
    }

    [HttpGet("{id:guid}/label")]
    public async Task<ActionResult<ApiResponse<SlitCoilLabelDto>>> GetLabel(Guid id, CancellationToken cancellationToken)
    {
        var result = await labelService.GetLabelAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<SlitCoilLabelDto>(result.Error);
    }

    [HttpPost("{id:guid}/label/print")]
    public async Task<ActionResult<ApiResponse<PrintSlitCoilLabelResultDto>>> PrintLabel(Guid id, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken)
    {
        var result = await labelService.PrintAsync(id, request, cancellationToken);
        return result.IsSuccess ? Success(result.Value, "Slit Coil label printed successfully.") : ToFailure<PrintSlitCoilLabelResultDto>(result.Error);
    }

    [HttpPost("{id:guid}/label/version/increment")]
    public async Task<ActionResult<ApiResponse<SlitCoilLabelDto>>> IncrementLabelVersion(Guid id, IncrementLabelVersionRequest request, CancellationToken cancellationToken)
    {
        var result = await labelService.IncrementVersionAsync(id, request, cancellationToken);
        return result.IsSuccess ? Success(result.Value, "Label Version incremented successfully.") : ToFailure<SlitCoilLabelDto>(result.Error);
    }

    [HttpGet("{id:guid}/label/print-history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LabelPrintHistoryDto>>>> GetPrintHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await labelService.GetHistoryAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<IReadOnlyList<LabelPrintHistoryDto>>(result.Error);
    }

    [HttpPost("labels/batch")]
    public async Task<ActionResult<ApiResponse<BatchPrintSlitCoilLabelsResultDto>>> BatchPrint(BatchPrintSlitCoilLabelsRequest request, CancellationToken cancellationToken)
    {
        var result = await labelService.BatchPrintAsync(request, cancellationToken);
        return Success(result, result.Failed.Count == 0 ? "Slit Coil labels printed successfully." : "Batch Print completed with partial failures.");
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
