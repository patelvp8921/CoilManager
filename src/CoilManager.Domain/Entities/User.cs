using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class User : SoftDeletableEntity
{
    private readonly List<UserRole> _userRoles = [];

    private User()
    {
    }

    public User(string userName, string email)
    {
        UserName = userName;
        Email = email;
    }

    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
}
