using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Grade : AuditableEntity, IMasterDataEntity
{
    private Grade()
    {
    }

    public Grade(string code, string description, bool isActive = true)
        : this(code, code, description, isActive)
    {
    }

    public Grade(string code, string name, string? description, bool isActive = true)
    {
        Code = code.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Update(string code, string name, string? description, bool isActive)
    {
        Code = code.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
