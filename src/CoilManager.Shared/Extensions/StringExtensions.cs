namespace CoilManager.Shared.Extensions;

public static class StringExtensions
{
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static string ToTrimmedOrEmpty(this string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
