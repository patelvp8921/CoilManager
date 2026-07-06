using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Grade : AuditableEntity
{
    private Grade()
    {
    }

    public Grade(string code, string description, bool isActive = true)
    {
        Code = code;
        Description = description;
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}
