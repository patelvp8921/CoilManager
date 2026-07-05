namespace CoilManager.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset ToUtc(this DateTimeOffset value)
    {
        return value.ToUniversalTime();
    }

    public static DateOnly ToDateOnly(this DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.UtcDateTime);
    }
}
