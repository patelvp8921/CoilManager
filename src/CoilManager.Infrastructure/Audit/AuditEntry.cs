namespace CoilManager.Infrastructure.Audit;

public sealed record AuditEntry(
    string Action,
    string EntityName,
    string? EntityId,
    string? UserId,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, object?>? Changes = null);
