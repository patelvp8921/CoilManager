using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CoilManager.API.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.IsInRole("Administrator") || context.User.HasClaim("permission", requirement.Permission)) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public const string Prefix = "Permission:";
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string name)
    {
        if (!name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) return await base.GetPolicyAsync(name);
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(name[Prefix.Length..])).Build();
    }
}
