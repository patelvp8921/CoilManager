using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Supplier : AuditableEntity
{
    private Supplier()
    {
    }

    public Supplier(string name, string code, bool isActive = true)
    {
        Name = name;
        Code = code;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}
