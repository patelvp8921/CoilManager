using System.Linq.Expressions;
using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Mappings;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class SlitCoilRepository : Repository<SlitCoil>, ISlitCoilRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SlitCoilRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SlitCoilListItemDto>> GetPagedAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<SlitCoil> query = ApplyFilters(BaseQuery().AsNoTracking(), request);
        int totalCount = await query.CountAsync(cancellationToken);

        List<SlitCoil> coils = await ApplySorting(query, request)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SlitCoilListItemDto>(
            coils.Select(coil => new SlitCoilListItemDto(coil.Id, coil.CoilNumber,
                coil.MotherCoil!.RawCoilNumber, coil.MotherCoilId, coil.SlittingJob!.SlittingJobNo,
                coil.SlittingJobId, coil.Grade?.Code, coil.Thickness, coil.Category, coil.Width,
                coil.Weight, coil.MotherCoil?.Supplier?.Name, coil.MotherCoil?.Manufacturer?.Name, coil.Status,
                coil.WarehouseLocation, coil.CreatedAtUtc, coil.LabelVersion, coil.LabelPrinted,
                coil.LabelPrintCount, coil.LabelLastPrintedOn,
                !string.IsNullOrEmpty(coil.BarcodeValue), !string.IsNullOrEmpty(coil.QrCodeValue))).ToArray(),
            request.NormalizedPage,
            request.NormalizedPageSize,
            totalCount);
    }

    public Task<SlitCoil?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return BaseQuery().FirstOrDefaultAsync(coil => coil.Id == id, cancellationToken);
    }

    public Task<SlitCoil?> GetByNumberWithDetailsAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        return BaseQuery().FirstOrDefaultAsync(coil => coil.CoilNumber == coilNumber, cancellationToken);
    }

    public Task<bool> ExistsByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.SlitCoils.AnyAsync(coil => coil.CoilNumber == coilNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<SlitCoil>> GetBySlittingJobIdAsync(Guid slittingJobId, CancellationToken cancellationToken = default)
    {
        return await BaseQuery()
            .Where(coil => coil.SlittingJobId == slittingJobId)
            .OrderBy(coil => coil.SlitSequence)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SlitCoil>> GetByMotherCoilIdAsync(Guid motherCoilId, CancellationToken cancellationToken = default) =>
        await BaseQuery().AsNoTracking().Where(coil => coil.MotherCoilId == motherCoilId)
            .OrderBy(coil => coil.SlitSequence).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<SlitCoil>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default) =>
        await BaseQuery().AsNoTracking().ToArrayAsync(cancellationToken);

    private IQueryable<SlitCoil> BaseQuery()
    {
        return _dbContext.SlitCoils
            .Include(coil => coil.MotherCoil)
                .ThenInclude(motherCoil => motherCoil!.Supplier)
            .Include(coil => coil.MotherCoil)
                .ThenInclude(motherCoil => motherCoil!.Manufacturer)
            .Include(coil => coil.SlittingJob)
            .Include(coil => coil.Grade)
            .Include(coil => coil.Supplier)
            .Include(coil => coil.Manufacturer);
    }

    private static IQueryable<SlitCoil> ApplyFilters(IQueryable<SlitCoil> query, SlitCoilQueryRequest request)
    {
        string? search = Normalize(request.Search);

        if (search is not null)
        {
            query = query.Where(coil =>
                coil.CoilNumber.Contains(search)
                || coil.MotherCoil!.RawCoilNumber.Contains(search)
                || coil.SlittingJob!.SlittingJobNo.Contains(search)
                || coil.BarcodeValue.Contains(search)
                || coil.QrCodeValue.Contains(search)
                || coil.HeatNumber.Contains(search)
                || coil.Grade!.Code.Contains(search));
        }

        if (Normalize(request.CoilNumber) is { } coilNumber) query = query.Where(coil => coil.CoilNumber.Contains(coilNumber));
        if (Normalize(request.MotherCoilNumber) is { } motherNumber) query = query.Where(coil => coil.MotherCoil!.RawCoilNumber.Contains(motherNumber));
        if (Normalize(request.SlittingJobNo) is { } jobNumber) query = query.Where(coil => coil.SlittingJob!.SlittingJobNo.Contains(jobNumber));
        if (request.GradeId.HasValue) query = query.Where(coil => coil.GradeId == request.GradeId.Value);
        if (request.SupplierId.HasValue) query = query.Where(coil => coil.SupplierId == request.SupplierId.Value);
        if (request.ManufacturerId.HasValue) query = query.Where(coil => coil.ManufacturerId == request.ManufacturerId.Value);
        if (request.Thickness.HasValue) query = query.Where(coil => coil.Thickness == request.Thickness.Value);
        if (request.WidthFrom.HasValue) query = query.Where(coil => coil.Width >= request.WidthFrom.Value);
        if (request.WidthTo.HasValue) query = query.Where(coil => coil.Width <= request.WidthTo.Value);
        if (request.WeightFrom.HasValue) query = query.Where(coil => coil.Weight >= request.WeightFrom.Value);
        if (request.WeightTo.HasValue) query = query.Where(coil => coil.Weight <= request.WeightTo.Value);
        if (request.CreatedFrom.HasValue) query = query.Where(coil => coil.CreatedAtUtc >= request.CreatedFrom.Value);
        if (request.CreatedTo.HasValue) query = query.Where(coil => coil.CreatedAtUtc <= request.CreatedTo.Value);

        if (request.Status.HasValue)
        {
            query = query.Where(coil => coil.Status == request.Status.Value);
        }

        return query;
    }

    private static IQueryable<SlitCoil> ApplySorting(IQueryable<SlitCoil> query, SlitCoilQueryRequest request)
    {
        Expression<Func<SlitCoil, object>> keySelector = Normalize(request.SortBy)?.ToLowerInvariant() switch
        {
            "coilnumber" or "slitcoilnumber" => coil => coil.CoilNumber,
            "mothercoilnumber" => coil => coil.MotherCoil!.RawCoilNumber,
            "slittingjobnumber" => coil => coil.SlittingJob!.SlittingJobNo,
            "grade" => coil => coil.Grade!.Code,
            "width" => coil => coil.Width,
            "weight" => coil => coil.Weight,
            "status" => coil => coil.Status,
            "createdon" => coil => coil.CreatedAtUtc,
            _ => coil => coil.CreatedAtUtc
        };

        return request.SortDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
