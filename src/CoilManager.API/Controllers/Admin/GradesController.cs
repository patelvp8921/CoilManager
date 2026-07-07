using CoilManager.Application.DTOs.Masters;
using CoilManager.Domain.Entities;
using CoilManager.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers.Admin;

[Route("api/admin/grades")]
public sealed class GradesController : MasterDataController<Grade>
{
    private readonly ApplicationDbContext _dbContext;

    public GradesController(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    protected override DbSet<Grade> Masters => _dbContext.Grades;
    protected override Grade CreateEntity(CreateMasterRequest request, string code) => new(code, request.Name, request.Description, request.IsActive);
    protected override void UpdateEntity(Grade entity, UpdateMasterRequest request, string code) => entity.Update(code, request.Name, request.Description, request.IsActive);
    protected override void SetActive(Grade entity, bool isActive) => entity.SetActive(isActive);
    protected override Task<bool> IsUsedByRawCoilAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.RawCoils.AnyAsync(rawCoil => rawCoil.GradeId == id, cancellationToken);
    }
}
