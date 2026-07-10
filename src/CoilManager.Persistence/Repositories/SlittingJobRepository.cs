using System.Linq.Expressions;
using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Mappings;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class SlittingJobRepository : Repository<SlittingJob>, ISlittingJobRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SlittingJobRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SlittingJobDto>> GetPagedAsync(
        SlittingJobQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<SlittingJob> query = ApplyListFilters(BaseQuery().AsNoTracking(), request);
        int totalCount = await query.CountAsync(cancellationToken);

        List<SlittingJob> jobs = await ApplyListSorting(query, request)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        IReadOnlyList<SlittingJobDto> items = jobs.Select(SlittingJobDtoMapper.MapToDto).ToList();

        return new PagedResult<SlittingJobDto>(
            items,
            request.NormalizedPage,
            request.NormalizedPageSize,
            totalCount);
    }

    public new Task<SlittingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return BaseQuery()
            .FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        string prefix = $"AE/S/{year}/";

        return _dbContext.SlittingJobs
            .CountAsync(job => job.SlittingJobNo.StartsWith(prefix), cancellationToken);
    }

    public Task<bool> ExistsByJobNumberAsync(string slittingJobNo, CancellationToken cancellationToken = default)
    {
        return _dbContext.SlittingJobs
            .AnyAsync(job => job.SlittingJobNo == slittingJobNo, cancellationToken);
    }

    public async Task DeleteItemsForRebuildAsync(SlittingJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);

        await _dbContext.SlittingJobItems
            .Where(item => item.SlittingJobId == job.Id)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (SlittingJobItem item in job.Items)
        {
            _dbContext.Entry(item).State = EntityState.Detached;
        }
    }

    public void TrackRebuiltItemsAsAdded(IEnumerable<SlittingJobItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (SlittingJobItem item in items)
        {
            _dbContext.Entry(item).State = EntityState.Added;
        }
    }

    private IQueryable<SlittingJob> BaseQuery()
    {
        return _dbContext.SlittingJobs
            .Include(job => job.MotherCoil)!.ThenInclude(rawCoil => rawCoil!.Supplier)
            .Include(job => job.MotherCoil)!.ThenInclude(rawCoil => rawCoil!.Manufacturer)
            .Include(job => job.MotherCoil)!.ThenInclude(rawCoil => rawCoil!.Grade)
            .Include(job => job.Items);
    }

    private static IQueryable<SlittingJob> ApplyListFilters(IQueryable<SlittingJob> query, SlittingJobQueryRequest request)
    {
        string? search = Normalize(request.Search);

        if (search is not null)
        {
            query = query.Where(job =>
                job.SlittingJobNo.Contains(search)
                || job.MotherCoil!.RawCoilNumber.Contains(search)
                || job.MotherCoil.CoilNumber.Contains(search)
                || job.MotherCoil.HeatNumber.Contains(search)
                || job.MotherCoil.Grade!.Code.Contains(search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(job => job.Status == request.Status.Value);
        }

        return query;
    }

    private static IQueryable<SlittingJob> ApplyListSorting(IQueryable<SlittingJob> query, SlittingJobQueryRequest request)
    {
        Expression<Func<SlittingJob, object>> keySelector = Normalize(request.SortBy)?.ToLowerInvariant() switch
        {
            "slittingjobno" => job => job.SlittingJobNo,
            "planningdate" => job => job.PlanningDate,
            "mothercoil" or "mothercoilno" => job => job.MotherCoil!.RawCoilNumber,
            "status" => job => job.Status,
            _ => job => job.CreatedAtUtc
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
