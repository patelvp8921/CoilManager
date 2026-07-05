namespace CoilManager.Shared.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    IReadOnlyList<string> Errors)
{
    public static ApiResponse<T> Ok(T? data, string message = "Request completed successfully.")
    {
        return new(true, message, data, []);
    }

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null)
    {
        return new(false, message, default, errors ?? []);
    }
}
