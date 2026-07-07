using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Supplier : AuditableEntity, IMasterDataEntity
{
    private Supplier()
    {
    }

    public Supplier(
        string name,
        string code,
        string? description = null,
        bool isActive = true,
        string? address = null,
        string? gst = null,
        string? email = null,
        string? contactNo = null)
    {
        Name = name.Trim();
        Code = code.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        GST = string.IsNullOrWhiteSpace(gst) ? null : gst.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        ContactNo = string.IsNullOrWhiteSpace(contactNo) ? null : contactNo.Trim();
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? GST { get; private set; }
    public string? Email { get; private set; }
    public string? ContactNo { get; private set; }
    public bool IsActive { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Update(
        string code,
        string name,
        string? description,
        bool isActive,
        string? address = null,
        string? gst = null,
        string? email = null,
        string? contactNo = null)
    {
        Code = code.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        GST = string.IsNullOrWhiteSpace(gst) ? null : gst.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        ContactNo = string.IsNullOrWhiteSpace(contactNo) ? null : contactNo.Trim();
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
