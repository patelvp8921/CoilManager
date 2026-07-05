using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Role : AuditableEntity
{
    private readonly List<UserRole> _userRoles = [];

    private Role()
    {
    }

    public Role(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
}
