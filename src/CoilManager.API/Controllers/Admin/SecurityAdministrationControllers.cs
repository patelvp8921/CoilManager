using System.Security.Claims;
using CoilManager.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers.Admin;

[Authorize]
public abstract class SecurityAdminControllerBase : BaseApiController
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    protected Guid CurrentSessionId => Guid.TryParse(User.FindFirstValue("sid"), out Guid id) ? id : Guid.Empty;
}

[Route("api/admin/users")]
public sealed class UsersController(ISecurityPlatformService security) : SecurityAdminControllerBase
{
    [HttpGet, Authorize(Policy="Permission:Administration.Users.View")]
    public async Task<ActionResult> List([FromQuery]string? search, [FromQuery]bool? active, [FromQuery]int page=1, [FromQuery]int pageSize=25, CancellationToken ct=default) => SuccessResult(await security.GetUsersAsync(search, active, page, Math.Clamp(pageSize,1,100),ct));
    [HttpGet("{id:guid}"), Authorize(Policy="Permission:Administration.Users.View")] public async Task<ActionResult> Get(Guid id,CancellationToken ct)=>SuccessResult(await security.GetUserAsync(id,ct));
    [HttpPost, Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Create(CreateUserRequest request,CancellationToken ct){Guid id=await security.CreateUserAsync(request,CurrentUserId,ct);return CreatedAtAction(nameof(Get),new{id},new{id});}
    [HttpPut("{id:guid}"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Update(Guid id,UpdateUserRequest request,CancellationToken ct){await security.UpdateUserAsync(id,request,CurrentUserId,ct);return NoContent();}
    [HttpPost("{id:guid}/activate"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Activate(Guid id,CancellationToken ct){await security.SetUserActiveAsync(id,true,CurrentUserId,ct);return NoContent();}
    [HttpPost("{id:guid}/deactivate"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Deactivate(Guid id,CancellationToken ct){await security.SetUserActiveAsync(id,false,CurrentUserId,ct);return NoContent();}
    [HttpPost("{id:guid}/unlock"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Unlock(Guid id,CancellationToken ct){await security.UnlockUserAsync(id,CurrentUserId,ct);return NoContent();}
    [HttpPost("{id:guid}/reset-password"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Reset(Guid id,AdminResetPasswordRequest r,CancellationToken ct){await security.AdminResetPasswordAsync(id,r.TemporaryPassword,CurrentUserId,ct);return NoContent();}
    [HttpGet("{id:guid}/sessions"), Authorize(Policy="Permission:Administration.Users.View")] public async Task<ActionResult> Sessions(Guid id,CancellationToken ct)=>SuccessResult(await security.GetSessionsAsync(id,CurrentSessionId,ct));
}
public sealed record AdminResetPasswordRequest(string TemporaryPassword);

[Route("api/admin/roles")]
public sealed class RolesController(ISecurityPlatformService security) : SecurityAdminControllerBase
{
    [HttpGet, Authorize(Policy="Permission:Administration.Roles.View")] public async Task<ActionResult> List(CancellationToken ct)=>SuccessResult(await security.GetRolesAsync(ct));
    [HttpPost, Authorize(Policy="Permission:Administration.Roles.Manage")] public async Task<ActionResult> Create(SaveRoleRequest r,CancellationToken ct)=>SuccessResult(await security.CreateRoleAsync(r,CurrentUserId,ct));
    [HttpPut("{id:guid}"), Authorize(Policy="Permission:Administration.Roles.Manage")] public async Task<ActionResult> Update(Guid id,SaveRoleRequest r,CancellationToken ct){await security.UpdateRoleAsync(id,r,CurrentUserId,ct);return NoContent();}
    [HttpDelete("{id:guid}"), Authorize(Policy="Permission:Administration.Roles.Manage")] public async Task<ActionResult> Delete(Guid id,CancellationToken ct){await security.DeleteRoleAsync(id,CurrentUserId,ct);return NoContent();}
    [HttpPut("{id:guid}/permissions"), Authorize(Policy="Permission:Administration.Roles.Manage")] public async Task<ActionResult> Permissions(Guid id,UpdateRolePermissionsRequest r,CancellationToken ct){await security.SetRolePermissionsAsync(id,r.Permissions,CurrentUserId,ct);return NoContent();}
}
public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> Permissions);

[Route("api/admin/permissions")]
public sealed class PermissionsController(ISecurityPlatformService security) : SecurityAdminControllerBase
{ [HttpGet, Authorize(Policy="Permission:Administration.Roles.View")] public async Task<ActionResult> List(CancellationToken ct)=>SuccessResult(await security.GetPermissionsAsync(ct)); }

[Route("api/admin/audit")]
public sealed class AuditController(ISecurityPlatformService security) : SecurityAdminControllerBase
{ [HttpGet, Authorize(Policy="Permission:Administration.Audit.View")] public async Task<ActionResult> Search([FromQuery]AuditSearchRequest request,CancellationToken ct)=>SuccessResult(await security.SearchAuditAsync(request,ct)); }

[Route("api/admin/sessions")]
public sealed class SessionsController(ISecurityPlatformService security) : SecurityAdminControllerBase
{ [HttpGet, Authorize(Policy="Permission:Administration.Users.View")] public async Task<ActionResult> List(CancellationToken ct)=>SuccessResult(await security.GetSessionsAsync(null,CurrentSessionId,ct)); [HttpDelete("{id:guid}"), Authorize(Policy="Permission:Administration.Users.Manage")] public async Task<ActionResult> Revoke(Guid id,CancellationToken ct){await security.RevokeSessionAsync(id,CurrentUserId,ct);return NoContent();} }

[Route("api/admin/company")]
public sealed class CompanyController(ISecurityPlatformService security) : SecurityAdminControllerBase
{ [HttpGet] public async Task<ActionResult> Get(CancellationToken ct)=>SuccessResult(await security.GetCompanyAsync(ct)); [HttpPut, Authorize(Policy="Permission:Administration.Company.Manage")] public async Task<ActionResult> Update(UpdateCompanyProfileRequest r,CancellationToken ct){await security.UpdateCompanyAsync(r,CurrentUserId,ct);return NoContent();} }
