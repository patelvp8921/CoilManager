namespace CoilManager.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAtUtc { get; protected set; }
    public string? DeletedBy { get; protected set; }

    public void MarkDeleted(string? userId, DateTimeOffset deletedAtUtc)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedAtUtc = deletedAtUtc;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAtUtc = null;
    }
}
