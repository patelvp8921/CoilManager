using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Manufacturer : AuditableEntity, IMasterDataEntity
{
    private Manufacturer()
    {
    }

    public Manufacturer(string name, string code, string? description = null, bool isActive = true, string? country = null)
    {
        Name = name.Trim();
        Code = code.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Country { get; private set; }
    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Update(string code, string name, string? description, bool isActive, string? country = null)
    {
        Code = code.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim();
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
