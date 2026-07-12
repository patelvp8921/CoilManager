using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Errors;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/slitting-jobs")]
public sealed class SlittingJobsController(ISlittingJobService slittingJobService, ISlitCoilLabelService labelService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<SlittingJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiPagedResponse<SlittingJobDto>>> GetAll(
        [FromQuery] SlittingJobQueryRequest request,
        CancellationToken cancellationToken)
    {
        PagedResult<SlittingJobDto> result = await slittingJobService.GetAsync(request, cancellationToken);

        return Paged(
            result.Items,
            new PaginationResult(result.PageNumber, result.PageSize, result.TotalCount));
    }

    [HttpGet("next-job-number")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetNextJobNumber(CancellationToken cancellationToken)
    {
        string nextJobNumber = await slittingJobService.GetNextJobNumberAsync(cancellationToken);

        return Success(nextJobNumber);
    }

    [HttpGet("mother-coils")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SlittingMotherCoilLookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SlittingMotherCoilLookupDto>>>> SearchMotherCoils(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SlittingMotherCoilLookupDto> motherCoils = await slittingJobService.SearchMotherCoilsAsync(search, cancellationToken);

        return Success(motherCoils);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlittingJobDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobDto> result = await slittingJobService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value)
            : ToFailure<SlittingJobDto>(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SlittingJobDto>>> Create(
        [FromBody] CreateSlittingJobRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobDto> result = await slittingJobService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            return ToFailure<SlittingJobDto>(result.Error);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            ApiResponse<SlittingJobDto>.Ok(result.Value, "Slitting job saved successfully."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<SlittingJobDto>>> Update(
        Guid id,
        [FromBody] UpdateSlittingJobRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobDto> result = await slittingJobService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Slitting job updated successfully.")
            : ToFailure<SlittingJobDto>(result.Error);
    }

    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlittingJobDto>>> Release(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobDto> result = await slittingJobService.ReleaseAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Slitting job released successfully.")
            : ToFailure<SlittingJobDto>(result.Error);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<StartSlittingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StartSlittingResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StartSlittingResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<StartSlittingResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<StartSlittingResponse>>> Start(
        Guid id,
        [FromBody] StartSlittingRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<StartSlittingResponse> result = await slittingJobService.StartSlittingAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Slitting started successfully.")
            : ToFailure<StartSlittingResponse>(result.Error);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<CompleteSlittingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CompleteSlittingResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CompleteSlittingResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CompleteSlittingResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CompleteSlittingResponse>>> Complete(
        Guid id,
        [FromBody] CompleteSlittingRequest request,
        CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<CompleteSlittingResponse> result = await slittingJobService.CompleteAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Slitting completed and slit coil inventory generated successfully.")
            : ToFailure<CompleteSlittingResponse>(result.Error);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlittingJobDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobDto> result = await slittingJobService.CancelAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value, "Slitting job cancelled successfully.")
            : ToFailure<SlittingJobDto>(result.Error);
    }

    [HttpGet("{id:guid}/completion")]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobCompletionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SlittingJobCompletionDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SlittingJobCompletionDto>>> GetCompletion(Guid id, CancellationToken cancellationToken)
    {
        CoilManager.Shared.Results.Result<SlittingJobCompletionDto> result = await slittingJobService.GetCompletionAsync(id, cancellationToken);

        return result.IsSuccess
            ? Success(result.Value)
            : ToFailure<SlittingJobCompletionDto>(result.Error);
    }

    [HttpGet("{id:guid}/slit-coil-labels")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SlitCoilLabelDto>>>> GetSlitCoilLabels(Guid id, CancellationToken cancellationToken)
    {
        var result = await labelService.GetJobLabelsAsync(id, cancellationToken);
        return result.IsSuccess ? Success(result.Value) : ToFailure<IReadOnlyList<SlitCoilLabelDto>>(result.Error);
    }

    [HttpPost("{id:guid}/slit-coil-labels/print")]
    public async Task<ActionResult<ApiResponse<BatchPrintSlitCoilLabelsResultDto>>> PrintSlitCoilLabels(Guid id, PrintSlitCoilLabelRequest request, CancellationToken cancellationToken)
    {
        var result = await labelService.PrintJobLabelsAsync(id, request, cancellationToken);
        return result.IsSuccess ? Success(result.Value, "Slit Coil labels printed successfully.") : ToFailure<BatchPrintSlitCoilLabelsResultDto>(result.Error);
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
