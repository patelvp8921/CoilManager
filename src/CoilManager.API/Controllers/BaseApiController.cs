using CoilManager.Shared.Responses;
using CoilManager.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T? data, string message = "Request completed successfully.")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }
    protected ActionResult SuccessResult<T>(T? data, string message = "Request completed successfully.")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected ActionResult<ApiPagedResponse<T>> Paged<T>(
        IReadOnlyList<T> data,
        PaginationResult pagination,
        string message = "Request completed successfully.")
    {
        return Ok(new ApiPagedResponse<T>(true, message, data, pagination, []));
    }

    protected ObjectResult Failure<T>(
        int statusCode,
        string message,
        IReadOnlyList<string>? errors = null)
    {
        return StatusCode(statusCode, ApiResponse<T>.Fail(message, errors));
    }
}
