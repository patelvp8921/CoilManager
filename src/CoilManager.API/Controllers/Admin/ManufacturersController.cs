using CoilManager.Application.DTOs.Masters;
using CoilManager.Domain.Entities;
using CoilManager.Persistence;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers.Admin;

[Route("api/admin/manufacturers")]
public sealed class ManufacturersController : MasterDataController<Manufacturer>
{
    private readonly ApplicationDbContext _dbContext;

    public ManufacturersController(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    protected override DbSet<Manufacturer> Masters => _dbContext.Manufacturers;
    protected override Manufacturer CreateEntity(CreateMasterRequest request, string code) => new(request.Name, code, request.Description, request.IsActive, request.Country);
    protected override void UpdateEntity(Manufacturer entity, UpdateMasterRequest request, string code) => entity.Update(code, request.Name, request.Description, request.IsActive, request.Country);
    protected override void SetActive(Manufacturer entity, bool isActive) => entity.SetActive(isActive);
    protected override string? GetCountry(Manufacturer entity) => entity.Country;
    protected override Task<bool> IsUsedByRawCoilAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.RawCoils.AnyAsync(rawCoil => rawCoil.ManufacturerId == id, cancellationToken);
    }

    [HttpDelete("inactive")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteInactive(CancellationToken cancellationToken)
    {
        List<Guid> inactiveManufacturerIds = await _dbContext.Manufacturers
            .Where(manufacturer => !manufacturer.IsActive)
            .Select(manufacturer => manufacturer.Id)
            .ToListAsync(cancellationToken);

        if (inactiveManufacturerIds.Count == 0)
        {
            return Success<object>(new { deletedRawCoils = 0, deletedManufacturers = 0 }, "No inactive manufacturers were found.");
        }

        int deletedRawCoils = await _dbContext.RawCoils
            .IgnoreQueryFilters()
            .Where(rawCoil => inactiveManufacturerIds.Contains(rawCoil.ManufacturerId))
            .ExecuteDeleteAsync(cancellationToken);

        int deletedManufacturers = await _dbContext.Manufacturers
            .Where(manufacturer => inactiveManufacturerIds.Contains(manufacturer.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return Success<object>(
            new { deletedRawCoils, deletedManufacturers },
            "Inactive manufacturers and linked mother coils were deleted successfully.");
    }

    protected override IQueryable<Manufacturer> ApplySearch(IQueryable<Manufacturer> query, string search)
    {
        return query.Where(manufacturer =>
            manufacturer.Code.Contains(search)
            || manufacturer.Name.Contains(search)
            || (manufacturer.Description != null && manufacturer.Description.Contains(search))
            || (manufacturer.Country != null && manufacturer.Country.Contains(search)));
    }

    protected override IQueryable<Manufacturer> ApplySorting(IQueryable<Manufacturer> query, MasterQueryRequest request)
    {
        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? string.Empty;
        return (sortBy, request.SortDescending) switch
        {
            ("country", true) => query.OrderByDescending(manufacturer => manufacturer.Country),
            ("country", false) => query.OrderBy(manufacturer => manufacturer.Country),
            _ => base.ApplySorting(query, request)
        };
    }
}
