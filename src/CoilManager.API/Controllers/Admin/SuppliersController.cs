using CoilManager.Application.DTOs.Masters;
using CoilManager.Domain.Entities;
using CoilManager.Persistence;
using CoilManager.Shared.Responses;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers.Admin;

[Route("api/admin/suppliers")]
public sealed class SuppliersController : MasterDataController<Supplier>
{
    private readonly ApplicationDbContext _dbContext;

    public SuppliersController(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    protected override DbSet<Supplier> Masters => _dbContext.Suppliers;
    protected override Supplier CreateEntity(CreateMasterRequest request, string code)
    {
        return new(
            request.Name,
            code,
            request.Description,
            request.IsActive,
            request.Address,
            request.GST,
            request.Email,
            request.ContactNo);
    }

    protected override void UpdateEntity(Supplier entity, UpdateMasterRequest request, string code)
    {
        entity.Update(
            code,
            request.Name,
            request.Description,
            request.IsActive,
            request.Address,
            request.GST,
            request.Email,
            request.ContactNo);
    }

    protected override void SetActive(Supplier entity, bool isActive) => entity.SetActive(isActive);
    protected override string? GetAddress(Supplier entity) => entity.Address;
    protected override string? GetGST(Supplier entity) => entity.GST;
    protected override string? GetEmail(Supplier entity) => entity.Email;
    protected override string? GetContactNo(Supplier entity) => entity.ContactNo;

    protected override string? Validate(string? code, string name)
    {
        return string.IsNullOrWhiteSpace(name) ? "Name is required." : null;
    }

    protected override string GetCreateCode(CreateMasterRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Code)
            ? GenerateSupplierCode(request.Name)
            : request.Code.Trim();
    }

    protected override string GetUpdateCode(Supplier entity, UpdateMasterRequest request)
    {
        return string.IsNullOrWhiteSpace(request.Code) ? entity.Code : request.Code.Trim();
    }

    protected override Task<bool> IsUsedByRawCoilAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.RawCoils.AnyAsync(rawCoil => rawCoil.SupplierId == id, cancellationToken);
    }

    protected override IQueryable<Supplier> ApplySearch(IQueryable<Supplier> query, string search)
    {
        return query.Where(supplier =>
            supplier.Name.Contains(search)
            || supplier.Code.Contains(search)
            || (supplier.Description != null && supplier.Description.Contains(search))
            || (supplier.Address != null && supplier.Address.Contains(search))
            || (supplier.GST != null && supplier.GST.Contains(search))
            || (supplier.Email != null && supplier.Email.Contains(search))
            || (supplier.ContactNo != null && supplier.ContactNo.Contains(search)));
    }

    protected override IQueryable<Supplier> ApplySorting(IQueryable<Supplier> query, MasterQueryRequest request)
    {
        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? string.Empty;
        return (sortBy, request.SortDescending) switch
        {
            ("address", true) => query.OrderByDescending(supplier => supplier.Address),
            ("address", false) => query.OrderBy(supplier => supplier.Address),
            ("gst", true) => query.OrderByDescending(supplier => supplier.GST),
            ("gst", false) => query.OrderBy(supplier => supplier.GST),
            ("email", true) => query.OrderByDescending(supplier => supplier.Email),
            ("email", false) => query.OrderBy(supplier => supplier.Email),
            ("contactno", true) => query.OrderByDescending(supplier => supplier.ContactNo),
            ("contactno", false) => query.OrderBy(supplier => supplier.ContactNo),
            _ => base.ApplySorting(query, request)
        };
    }

    [HttpDelete("inactive")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteInactive(CancellationToken cancellationToken)
    {
        List<Guid> inactiveSupplierIds = await _dbContext.Suppliers
            .Where(supplier => !supplier.IsActive)
            .Select(supplier => supplier.Id)
            .ToListAsync(cancellationToken);

        if (inactiveSupplierIds.Count == 0)
        {
            return Success<object>(new { deletedRawCoils = 0, deletedSuppliers = 0 }, "No inactive suppliers were found.");
        }

        int deletedRawCoils = await _dbContext.RawCoils
            .IgnoreQueryFilters()
            .Where(rawCoil => inactiveSupplierIds.Contains(rawCoil.SupplierId))
            .ExecuteDeleteAsync(cancellationToken);

        int deletedSuppliers = await _dbContext.Suppliers
            .Where(supplier => inactiveSupplierIds.Contains(supplier.Id))
            .ExecuteDeleteAsync(cancellationToken);

        return Success<object>(
            new { deletedRawCoils, deletedSuppliers },
            "Inactive suppliers and linked mother coils were deleted successfully.");
    }

    private static string GenerateSupplierCode(string name)
    {
        StringBuilder builder = new();
        foreach (char character in name.ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }

            if (builder.Length == 20)
            {
                break;
            }
        }

        return builder.Length == 0 ? $"SUP{DateTimeOffset.UtcNow:yyyyMMddHHmmss}" : builder.ToString();
    }
}
