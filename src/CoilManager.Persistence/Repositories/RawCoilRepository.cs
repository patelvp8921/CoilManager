using System.Linq.Expressions;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class RawCoilRepository : Repository<RawCoil>, IRawCoilRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RawCoilRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<RawCoilDto>> GetPagedAsync(
        RawCoilQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<RawCoil> query = ApplyListFilters(_dbContext.RawCoils.AsNoTracking(), request);
        int totalCount = await query.CountAsync(cancellationToken);

        List<RawCoilListProjection> rows = await ApplyListSorting(query, request)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize)
            .Take(request.NormalizedPageSize)
            .Select(rawCoil => new RawCoilListProjection(
                rawCoil.Id,
                rawCoil.RawCoilNumber,
                rawCoil.CoilNumber,
                rawCoil.HeatNumber,
                rawCoil.PONumber,
                rawCoil.InvoiceNo,
                rawCoil.MillTCNo,
                rawCoil.BISLicNumber,
                rawCoil.SupplierId,
                rawCoil.Supplier!.Name,
                rawCoil.ManufacturerId,
                rawCoil.Manufacturer!.Name,
                rawCoil.GradeId,
                rawCoil.Grade!.Code,
                rawCoil.Thickness,
                rawCoil.Width,
                rawCoil.Weight,
                rawCoil.Length,
                rawCoil.WattLossPerKg,
                rawCoil.WarehouseLocation,
                rawCoil.Status,
                rawCoil.ReceivedDate,
                rawCoil.CreatedAtUtc,
                rawCoil.CreatedBy,
                rawCoil.UpdatedAtUtc,
                rawCoil.UpdatedBy,
                rawCoil.IsDeleted,
                rawCoil.DeletedAtUtc,
                rawCoil.RowVersion))
            .ToListAsync(cancellationToken);

        IReadOnlyList<RawCoilDto> items = rows.Select(row => new RawCoilDto(
            row.Id,
            row.RawCoilNumber,
            row.RawCoilNumber,
            row.CoilNumber,
            row.HeatNumber,
            row.PONumber,
            row.InvoiceNo,
            row.MillTCNo,
            row.BISLicNumber,
            row.SupplierId,
            row.SupplierName,
            row.ManufacturerId,
            row.ManufacturerName,
            row.GradeId,
            row.Grade,
            row.Thickness,
            row.Width,
            row.Weight,
            row.Length,
            row.WattLossPerKg,
            row.WarehouseLocation,
            row.Status,
            row.ReceivedDate,
            row.CreatedOn,
            row.CreatedBy,
            row.ModifiedOn,
            row.ModifiedBy,
            row.IsDeleted,
            row.DeletedOn,
            Convert.ToBase64String(row.RowVersion),
            []))
            .ToList();

        return new PagedResult<RawCoilDto>(
            items,
            request.NormalizedPage,
            request.NormalizedPageSize,
            totalCount);
    }

    public new Task<RawCoil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
            .FirstOrDefaultAsync(rawCoil => rawCoil.Id == id, cancellationToken);
    }

    public new async Task<IReadOnlyList<RawCoil>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
            .ToListAsync(cancellationToken);
    }

    public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
            .FirstOrDefaultAsync(rawCoil => rawCoil.CoilNumber == coilNumber, cancellationToken);
    }

    public Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .AnyAsync(rawCoil =>
                rawCoil.CoilNumber == coilNumber
                && (!excludingId.HasValue || rawCoil.Id != excludingId.Value),
                cancellationToken);
    }

    public Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .CountAsync(rawCoil => rawCoil.ReceivedDate.Year == year, cancellationToken);
    }

    public Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default)
    {
        string prefix = $"RC-{year}-";

        return _dbContext.RawCoils
            .CountAsync(rawCoil => rawCoil.RawCoilNumber.StartsWith(prefix), cancellationToken);
    }

    public Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .AnyAsync(rawCoil => rawCoil.RawCoilNumber == rawCoilNumber, cancellationToken);
    }

    private static IQueryable<RawCoil> ApplyListFilters(IQueryable<RawCoil> query, RawCoilQueryRequest request)
    {
        string? search = Normalize(request.Search);
        string? grade = Normalize(request.Grade);
        string? manufacturer = Normalize(request.Manufacturer);

        if (search is not null)
        {
            query = query.Where(rawCoil =>
                rawCoil.CoilNumber.Contains(search)
                || rawCoil.RawCoilNumber.Contains(search)
                || rawCoil.HeatNumber.Contains(search)
                || rawCoil.Supplier!.Name.Contains(search)
                || rawCoil.Manufacturer!.Name.Contains(search)
                || rawCoil.Grade!.Code.Contains(search));
        }

        if (grade is not null)
        {
            query = query.Where(rawCoil => rawCoil.Grade!.Code == grade);
        }

        if (manufacturer is not null)
        {
            query = query.Where(rawCoil => rawCoil.Manufacturer!.Name == manufacturer);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(rawCoil => rawCoil.Status == request.Status.Value);
        }

        return query;
    }

    private static IQueryable<RawCoil> ApplyListSorting(IQueryable<RawCoil> query, RawCoilQueryRequest request)
    {
        Expression<Func<RawCoil, object>> keySelector = Normalize(request.SortBy)?.ToLowerInvariant() switch
        {
            "coilid" or "rawcoilnumber" => rawCoil => rawCoil.RawCoilNumber,
            "coilnumber" => rawCoil => rawCoil.CoilNumber,
            "grade" => rawCoil => rawCoil.Grade!.Code,
            "manufacturer" or "millname" => rawCoil => rawCoil.Manufacturer!.Name,
            "status" => rawCoil => rawCoil.Status,
            "receiveddate" => rawCoil => rawCoil.ReceivedDate,
            "weight" => rawCoil => rawCoil.Weight,
            "width" => rawCoil => rawCoil.Width,
            "thickness" => rawCoil => rawCoil.Thickness,
            _ => rawCoil => rawCoil.CreatedAtUtc
        };

        return request.SortDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record RawCoilListProjection(
        Guid Id,
        string RawCoilNumber,
        string CoilNumber,
        string HeatNumber,
        string? PONumber,
        string? InvoiceNo,
        string? MillTCNo,
        string? BISLicNumber,
        Guid SupplierId,
        string SupplierName,
        Guid ManufacturerId,
        string ManufacturerName,
        Guid GradeId,
        string Grade,
        decimal Thickness,
        decimal Width,
        decimal Weight,
        decimal Length,
        decimal WattLossPerKg,
        string? WarehouseLocation,
        Domain.Enums.CoilStatus Status,
        DateOnly ReceivedDate,
        DateTimeOffset CreatedOn,
        string? CreatedBy,
        DateTimeOffset? ModifiedOn,
        string? ModifiedBy,
        bool IsDeleted,
        DateTimeOffset? DeletedOn,
        byte[] RowVersion);
}
