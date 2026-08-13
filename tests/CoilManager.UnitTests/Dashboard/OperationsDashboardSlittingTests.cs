using System.Reflection;
using CoilManager.Application.DTOs.Dashboard;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Services;
using CoilManager.Application.Specifications;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Pagination;

namespace CoilManager.UnitTests.Dashboard;

public sealed class OperationsDashboardSlittingTests
{
    [Fact]
    public async Task GetOperationsDashboardAsync_CountsReleasedAndInProgressJobs()
    {
        RawCoil motherCoil = CreateMotherCoil();
        SlittingJob released = CreateReleasedJob(motherCoil, "AE/S/2026/00001", DateTimeOffset.UtcNow.AddMinutes(-30));
        SlittingJob running = CreateInProgressJob(motherCoil, "AE/S/2026/00002", DateTimeOffset.UtcNow.AddMinutes(-20), DateTimeOffset.UtcNow.AddMinutes(-10));
        OperationsDashboardService service = new(new FakeRawCoilRepository([motherCoil]), new FakeSlittingJobRepository([released, running]));

        OperationsDashboardDto dashboard = await service.GetOperationsDashboardAsync();

        Assert.Equal(1, dashboard.SlittingJobMetrics.ReleasedJobs);
        Assert.Equal(1, dashboard.SlittingJobMetrics.InProgressJobs);
        Assert.Equal(2, dashboard.Slitting.SlittingJobs);
    }

    [Fact]
    public async Task GetOperationsDashboardAsync_OrdersQueueRunningThenOldestReleased()
    {
        RawCoil motherCoil = CreateMotherCoil();
        SlittingJob newerReleased = CreateReleasedJob(motherCoil, "AE/S/2026/00003", DateTimeOffset.UtcNow.AddMinutes(-10));
        SlittingJob olderReleased = CreateReleasedJob(motherCoil, "AE/S/2026/00001", DateTimeOffset.UtcNow.AddMinutes(-60));
        SlittingJob running = CreateInProgressJob(motherCoil, "AE/S/2026/00002", DateTimeOffset.UtcNow.AddMinutes(-30), DateTimeOffset.UtcNow.AddMinutes(-20));
        OperationsDashboardService service = new(new FakeRawCoilRepository([motherCoil]), new FakeSlittingJobRepository([newerReleased, olderReleased, running]));

        OperationsDashboardDto dashboard = await service.GetOperationsDashboardAsync();

        Assert.Equal(
            new[] { "AE/S/2026/00002", "AE/S/2026/00001", "AE/S/2026/00003" },
            dashboard.ProductionQueue.Select(item => item.SlittingJobNo).ToArray());
        Assert.Equal("Running", dashboard.ProductionQueue[0].Status);
        Assert.Equal("Waiting to Start", dashboard.ProductionQueue[1].Status);
    }

    [Fact]
    public async Task GetOperationsDashboardAsync_IncludesIncomingDraftJobsAndNoDispatchAlerts()
    {
        RawCoil motherCoil = CreateMotherCoil();
        SlittingJob draft = CreateJob(motherCoil, "AE/S/2026/00004");
        OperationsDashboardService service = new(new FakeRawCoilRepository([motherCoil]), new FakeSlittingJobRepository([draft]));

        OperationsDashboardDto dashboard = await service.GetOperationsDashboardAsync();

        ProductionQueueItemDto item = Assert.Single(dashboard.ProductionQueue);
        Assert.Equal("Planned", item.Status);
        Assert.DoesNotContain(dashboard.OperationalAlerts, alert => alert.Category.StartsWith("dispatch", StringComparison.OrdinalIgnoreCase));
    }
    private static RawCoil CreateMotherCoil()
    {
        Supplier supplier = new("Prime Supplier", "PS");
        Manufacturer manufacturer = new("Prime Manufacturer", "PM");
        Grade grade = new("23HP85", 0.23m, 0.85m);
        RawCoil motherCoil = new(
            "MC-2026-00001",
            "C-001",
            "H-001",
            null,
            null,
            null,
            null,
            supplier.Id,
            manufacturer.Id,
            grade.Id,
            0.23m,
            "M3",
            0.85m,
            1000m,
            100m,
            1000m,
            "WH-A",
            DateOnly.FromDateTime(DateTime.UtcNow),
            CoilStatus.Available);
        motherCoil.SetLookupReferences(supplier, manufacturer, grade);
        return motherCoil;
    }

    private static SlittingJob CreateReleasedJob(RawCoil motherCoil, string jobNumber, DateTimeOffset releasedOn)
    {
        SlittingJob job = CreateJob(motherCoil, jobNumber);
        job.Release("Planner", releasedOn);
        return job;
    }

    private static SlittingJob CreateInProgressJob(RawCoil motherCoil, string jobNumber, DateTimeOffset releasedOn, DateTimeOffset startedOn)
    {
        SlittingJob job = CreateReleasedJob(motherCoil, jobNumber, releasedOn);
        job.Start("Operator", startedOn, null, "A", null);
        return job;
    }

    private static SlittingJob CreateJob(RawCoil motherCoil, string jobNumber)
    {
        SlittingJob job = new(
            jobNumber,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            motherCoil.Id,
            null,
            null,
            0.2m,
            5m,
            5m,
            null);
        job.ReplaceItems([new SlittingJobItem(1, "SC-2026-00001-01", 300m, 30m)]);
        SetPrivateProperty(job, nameof(SlittingJob.MotherCoil), motherCoil);
        return job;
    }

    private sealed class FakeRawCoilRepository(IReadOnlyList<RawCoil> coils) : RepositoryFake<RawCoil>, IRawCoilRepository
    {
        public override Task<IReadOnlyList<RawCoil>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(coils);
        }

        public Task<PagedResult<RawCoilDto>> GetPagedAsync(RawCoilQueryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<RawCoilDto>([], request.NormalizedPage, request.NormalizedPageSize, 0));
        }

        public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default) => Task.FromResult<RawCoil?>(null);
        public Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeSlittingJobRepository(IReadOnlyList<SlittingJob> jobs) : RepositoryFake<SlittingJob>, ISlittingJobRepository
    {
        public Task<PagedResult<SlittingJobDto>> GetPagedAsync(SlittingJobQueryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<SlittingJobDto>([], request.NormalizedPage, request.NormalizedPageSize, 0));
        }

        public Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<bool> ExistsByJobNumberAsync(string slittingJobNo, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> DraftExistsForMotherCoilAsync(Guid motherCoilId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlySet<Guid>> GetDraftMotherCoilIdsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlySet<Guid>>(new HashSet<Guid>());
        public Task<IReadOnlyList<SlittingJob>> GetForDashboardAsync(CancellationToken cancellationToken = default) => Task.FromResult(jobs);
        public Task DeleteItemsForRebuildAsync(SlittingJob job, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void TrackRebuiltItemsAsAdded(IEnumerable<SlittingJobItem> items) { }
    }

    private abstract class RepositoryFake<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        public virtual IQueryable<TEntity> Query() => Array.Empty<TEntity>().AsQueryable();
        public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<TEntity?>(null);
        public virtual Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TEntity>>([]);
        public virtual Task<IReadOnlyList<TEntity>> GetAllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TEntity>>([]);
        public virtual Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) => Task.FromResult<TEntity?>(null);
        public virtual Task<int> CountAsync(ISpecification<TEntity>? specification = null, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public virtual void Update(TEntity entity) { }
        public virtual void Delete(TEntity entity) { }
        public virtual void Remove(TEntity entity) { }
    }

    private static void SetPrivateProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");
        property.SetValue(target, value);
    }
}
