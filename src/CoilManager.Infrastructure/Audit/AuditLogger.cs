using Microsoft.Extensions.Logging;

namespace CoilManager.Infrastructure.Audit;

public sealed class AuditLogger(ILogger<AuditLogger> logger) : IAuditLogger
{
    public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        logger.LogInformation(
            "Audit event {Action} for {EntityName} {EntityId} by {UserId} at {TimestampUtc}.",
            entry.Action,
            entry.EntityName,
            entry.EntityId,
            entry.UserId,
            entry.TimestampUtc);

        return Task.CompletedTask;
    }
}
