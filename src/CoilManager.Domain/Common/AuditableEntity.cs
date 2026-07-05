namespace CoilManager.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAtUtc { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    public void SetCreatedAudit(string? userId, DateTimeOffset createdAtUtc)
    {
        CreatedBy = userId;
        CreatedAtUtc = createdAtUtc;
    }

    public void SetUpdatedAudit(string? userId, DateTimeOffset updatedAtUtc)
    {
        UpdatedBy = userId;
        UpdatedAtUtc = updatedAtUtc;
    }
}
