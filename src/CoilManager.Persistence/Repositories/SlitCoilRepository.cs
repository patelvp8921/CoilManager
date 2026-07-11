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

    public async Task<PagedResult<SlitCoilDto>> GetPagedAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<SlitCoil> query = ApplyFilters(BaseQuery().AsNoTracking(), request);
        int totalCount = await query.CountAsync(cancellationToken);

        List<SlitCoil> coils = await ApplySorting(query, request)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SlitCoilDto>(
            coils.Select(SlitCoilDtoMapper.MapToDto).ToArray(),
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

    private IQueryable<SlitCoil> BaseQuery()
    {
        return _dbContext.SlitCoils
            .Include(coil => coil.MotherCoil)
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
                || coil.Grade!.Code.Contains(search));
        }

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
