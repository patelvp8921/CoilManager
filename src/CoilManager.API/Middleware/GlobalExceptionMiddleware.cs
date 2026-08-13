using System.Net;
using System.Text.Json;
using CoilManager.Shared.Responses;
using SharedExceptions = CoilManager.Shared.Exceptions;

namespace CoilManager.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (exception is SharedExceptions.ValidationException)
            {
                _logger.LogWarning(exception, "Validation exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogError(exception, "Exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            }

            await WriteProblemDetailsAsync(context, exception);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        (HttpStatusCode statusCode, string message, IReadOnlyList<string> errors) = exception switch
        {
            SharedExceptions.ValidationException validationException => (HttpStatusCode.BadRequest, validationException.Message, validationException.Errors),
            SharedExceptions.NotFoundException notFoundException => (HttpStatusCode.NotFound, notFoundException.Message, []),
            SharedExceptions.ConflictException conflictException => (HttpStatusCode.Conflict, conflictException.Message, []),
            SharedExceptions.UnauthorizedException unauthorizedException => (HttpStatusCode.Unauthorized, unauthorizedException.Message, []),
            SharedExceptions.BusinessRuleException businessRuleException => ((HttpStatusCode)422, businessRuleException.Message, []),
            _ => (HttpStatusCode.InternalServerError, _environment.IsDevelopment() ? exception.GetBaseException().Message : "An unexpected error occurred.", [])
        };

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        ApiResponse<object> response = ApiResponse<object>.Fail(message, errors);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
