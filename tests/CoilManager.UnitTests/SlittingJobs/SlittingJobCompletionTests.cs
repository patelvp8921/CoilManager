using System.Reflection;
using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Services;
using CoilManager.Application.Settings;
using CoilManager.Application.Specifications;
using CoilManager.Application.Validators.SlittingJobs;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Pagination;
using Microsoft.Extensions.Options;

namespace CoilManager.UnitTests.SlittingJobs;

public sealed class SlittingJobCompletionTests
{
    [Fact]
    public async Task CompleteAsync_RejectsDraftJob()
    {
        TestContext context = TestContext.Create(released: false);

        var result = await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        Assert.True(result.IsFailure);
        Assert.Contains("Only in progress", result.Error.Message);
    }

    [Fact]
    public async Task CompleteAsync_RejectsReleasedJob()
    {
        TestContext context = TestContext.Create();

        var result = await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        Assert.True(result.IsFailure);
        Assert.Contains("Only in progress", result.Error.Message);
    }

    [Fact]
    public async Task CompleteAsync_RejectsWhenMotherCoilIsNotInProcess()
    {
        TestContext context = TestContext.Create(started: true);
        context.MotherCoil.SetStatus(CoilStatus.Reserved);

        var result = await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        Assert.True(result.IsFailure);
        Assert.Contains("In Process", result.Error.Message);
    }

    [Fact]
    public async Task CompleteAsync_RejectsMissingActualWeight()
    {
        TestContext context = TestContext.Create(started: true);
        CompleteSlittingRequest request = context.ValidCompleteRequest(actualWeight: 0m);

        var result = await context.Service.CompleteAsync(context.Job.Id, request);

        Assert.True(result.IsFailure);
        Assert.Contains("actual weight", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteAsync_RejectsTotalWeightBeyondTolerance()
    {
        TestContext context = TestContext.Create(started: true);
        CompleteSlittingRequest request = context.ValidCompleteRequest(actualWeight: 60m);

        var result = await context.Service.CompleteAsync(context.Job.Id, request);

        Assert.True(result.IsFailure);
        Assert.Contains("weight", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteAsync_GeneratesSlitCoilsAndInventoryTransactions()
    {
        TestContext context = TestContext.Create(started: true);

        var result = await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(SlittingJobStatus.Completed, context.Job.Status);
        Assert.Equal(CoilStatus.Consumed, context.MotherCoil.Status);
        Assert.Equal(2, context.SlitCoilRepository.Added.Count);
        Assert.Equal("SC-2026-00001-01", context.SlitCoilRepository.Added[0].CoilNumber);
        Assert.Equal("SC-2026-00001-02", context.SlitCoilRepository.Added[1].CoilNumber);
        Assert.All(context.SlitCoilRepository.Added, coil =>
        {
            Assert.Equal(context.MotherCoil.Id, coil.ParentCoilId);
            Assert.Equal(context.MotherCoil.Id, coil.RootMotherCoilId);
            Assert.Equal(context.MotherCoil.Id, coil.MotherCoilId);
            Assert.Equal(CoilStatus.Available, coil.Status);
            Assert.Equal(coil.CoilNumber, coil.BarcodeValue);
            Assert.Equal(coil.CoilNumber, coil.QrCodeValue);
        });
        Assert.Equal(3, context.InventoryTransactionRepository.Added.Count);
        Assert.Contains(context.InventoryTransactionRepository.Added, transaction => transaction.TransactionType == InventoryTransactionType.SlittingJobComplete);
        Assert.Equal(2, context.InventoryTransactionRepository.Added.Count(transaction => transaction.TransactionType == InventoryTransactionType.SlitCoilGeneration));
    }

    [Fact]
    public async Task CompleteAsync_FailsWhenCompletedTwice()
    {
        TestContext context = TestContext.Create(started: true);
        await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        var secondResult = await context.Service.CompleteAsync(context.Job.Id, context.ValidCompleteRequest());

        Assert.True(secondResult.IsFailure);
        Assert.Contains("already completed", secondResult.Error.Message);
    }

    [Fact]
    public async Task StartSlittingAsync_SucceedsForReleasedJob()
    {
        TestContext context = TestContext.Create();

        var result = await context.Service.StartSlittingAsync(context.Job.Id, context.ValidStartRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(SlittingJobStatus.InProgress, context.Job.Status);
        Assert.Equal(CoilStatus.InProcess, context.MotherCoil.Status);
        Assert.Equal("Unit Tester", context.Job.StartedBy);
        Assert.True(context.Job.StartedOn.HasValue);
        InventoryTransaction transaction = Assert.Single(context.InventoryTransactionRepository.Added);
        Assert.Equal(InventoryTransactionType.SlittingStarted, transaction.TransactionType);
        Assert.Equal(CoilStatus.Reserved, transaction.FromStatus);
        Assert.Equal(CoilStatus.InProcess, transaction.ToStatus);
    }

    [Fact]
    public async Task StartSlittingAsync_FailsForDraftJob()
    {
        TestContext context = TestContext.Create(released: false);

        var result = await context.Service.StartSlittingAsync(context.Job.Id, context.ValidStartRequest());

        Assert.True(result.IsFailure);
        Assert.Contains("Only released", result.Error.Message);
    }

    [Fact]
    public async Task StartSlittingAsync_FailsWhenMotherCoilIsNotReserved()
    {
        TestContext context = TestContext.Create();
        context.MotherCoil.SetStatus(CoilStatus.Available);

        var result = await context.Service.StartSlittingAsync(context.Job.Id, context.ValidStartRequest());

        Assert.True(result.IsFailure);
        Assert.Contains("Reserved", result.Error.Message);
    }

    [Fact]
    public async Task StartSlittingAsync_FailsWhenStartedTwice()
    {
        TestContext context = TestContext.Create();
        await context.Service.StartSlittingAsync(context.Job.Id, context.ValidStartRequest());

        var secondResult = await context.Service.StartSlittingAsync(context.Job.Id, context.ValidStartRequest());

        Assert.True(secondResult.IsFailure);
        Assert.Contains("Only released", secondResult.Error.Message);
    }

    [Fact]
    public async Task CancelAsync_RestoresMotherCoilToAvailable()
    {
        TestContext context = TestContext.Create();

        var result = await context.Service.CancelAsync(context.Job.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(SlittingJobStatus.Cancelled, context.Job.Status);
        Assert.Equal(CoilStatus.Available, context.MotherCoil.Status);
        InventoryTransaction transaction = Assert.Single(context.InventoryTransactionRepository.Added);
        Assert.Equal(InventoryTransactionType.SlittingJobCancel, transaction.TransactionType);
        Assert.Equal(CoilStatus.Reserved, transaction.FromStatus);
        Assert.Equal(CoilStatus.Available, transaction.ToStatus);
    }

    [Fact]
    public async Task CancelAsync_FailsForInProgressJob()
    {
        TestContext context = TestContext.Create(started: true);

        var result = await context.Service.CancelAsync(context.Job.Id);

        Assert.True(result.IsFailure);
        Assert.Contains("cannot be cancelled", result.Error.Message);
    }

    [Fact]
    public void CoilNumberingService_GeneratesFirstGenerationNumbersFromMotherCoil()
    {
        CoilNumberingService service = new();

        Assert.Equal("SC-2026-00001-03", service.GenerateFirstGenerationSlitCoilNumber("MC-2026-00001", 3));
    }

    private sealed class TestContext
    {
        private TestContext(
            RawCoil motherCoil,
            SlittingJob job,
            FakeSlittingJobRepository slittingJobRepository,
            FakeRawCoilRepository rawCoilRepository,
            FakeSlitCoilRepository slitCoilRepository,
            FakeInventoryTransactionRepository inventoryTransactionRepository,
            SlittingJobService service)
        {
            MotherCoil = motherCoil;
            Job = job;
            SlittingJobRepository = slittingJobRepository;
            RawCoilRepository = rawCoilRepository;
            SlitCoilRepository = slitCoilRepository;
            InventoryTransactionRepository = inventoryTransactionRepository;
            Service = service;
        }

        public RawCoil MotherCoil { get; }
        public SlittingJob Job { get; }
        public FakeSlittingJobRepository SlittingJobRepository { get; }
        public FakeRawCoilRepository RawCoilRepository { get; }
        public FakeSlitCoilRepository SlitCoilRepository { get; }
        public FakeInventoryTransactionRepository InventoryTransactionRepository { get; }
        public SlittingJobService Service { get; }

        public static TestContext Create(bool released = true, bool started = false)
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
                released ? CoilStatus.Reserved : CoilStatus.Available);
            motherCoil.SetLookupReferences(supplier, manufacturer, grade);

            SlittingJob job = new(
                "AE/S/2026/00001",
                DateOnly.FromDateTime(DateTime.UtcNow),
                null,
                motherCoil.Id,
                null,
                null,
                0.2m,
                5m,
                5m,
                null);
            job.ReplaceItems([
                new SlittingJobItem(1, "SC-2026-00001-01", 300m, 30m),
                new SlittingJobItem(2, "SC-2026-00001-02", 300m, 30m)
            ]);
            SetPrivateProperty(job, nameof(SlittingJob.MotherCoil), motherCoil);
            SetPrivateProperty(job, nameof(SlittingJob.RowVersion), new byte[] { 1, 2, 3 });
            if (released)
            {
                job.Release("Unit Tester", DateTimeOffset.UtcNow.AddMinutes(-30));
            }

            if (started)
            {
                job.Start("Unit Tester", DateTimeOffset.UtcNow.AddMinutes(-10), null, null, null);
                motherCoil.SetStatus(CoilStatus.InProcess);
            }

            FakeSlittingJobRepository slittingJobRepository = new(job);
            FakeRawCoilRepository rawCoilRepository = new(motherCoil);
            FakeSlitCoilRepository slitCoilRepository = new(motherCoil, job, grade, supplier, manufacturer);
            FakeInventoryTransactionRepository inventoryTransactionRepository = new();
            SlittingJobService service = new(
                slittingJobRepository,
                rawCoilRepository,
                slitCoilRepository,
                inventoryTransactionRepository,
                new CoilNumberingService(),
                new FakeUnitOfWork(),
                new FakeCurrentUserService(),
                new CreateSlittingJobRequestValidator(),
                new UpdateSlittingJobRequestValidator(),
                new CompleteSlittingRequestValidator(),
                new StartSlittingRequestValidator(),
                Options.Create(new SlittingSettings
                {
                    WeightToleranceKg = 0.5m,
                    WidthToleranceMm = 0.5m,
                    MinimumBalanceWidthMm = 10m,
                    DefaultLabelVersion = "1"
                }));

            return new TestContext(
                motherCoil,
                job,
                slittingJobRepository,
                rawCoilRepository,
                slitCoilRepository,
                inventoryTransactionRepository,
                service);
        }

        public CompleteSlittingRequest ValidCompleteRequest(decimal actualWeight = 40m)
        {
            return new CompleteSlittingRequest(
                Convert.ToBase64String(Job.RowVersion),
                Job.Items
                    .OrderBy(item => item.SequenceNo)
                    .Select(item => new CompleteSlittingItemRequest(item.Id, actualWeight, item.Width, null))
                    .ToArray());
        }

        public StartSlittingRequest ValidStartRequest()
        {
            return new StartSlittingRequest(Convert.ToBase64String(Job.RowVersion), null, "A", "Start test");
        }
    }

    private sealed class FakeSlittingJobRepository(SlittingJob job) : RepositoryFake<SlittingJob>, ISlittingJobRepository
    {
        public override Task<SlittingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == job.Id ? job : null);
        }

        public Task<PagedResult<SlittingJobDto>> GetPagedAsync(SlittingJobQueryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<SlittingJobDto>([], request.NormalizedPage, request.NormalizedPageSize, 0));
        }

        public Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<bool> ExistsByJobNumberAsync(string slittingJobNo, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> DraftExistsForMotherCoilAsync(Guid motherCoilId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlySet<Guid>> GetDraftMotherCoilIdsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlySet<Guid>>(new HashSet<Guid>());
        public Task<IReadOnlyList<SlittingJob>> GetForDashboardAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SlittingJob>>([job]);
        public Task DeleteItemsForRebuildAsync(SlittingJob job, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void TrackRebuiltItemsAsAdded(IEnumerable<SlittingJobItem> items) { }
    }

    private sealed class FakeRawCoilRepository(RawCoil motherCoil) : RepositoryFake<RawCoil>, IRawCoilRepository
    {
        public override Task<RawCoil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == motherCoil.Id ? motherCoil : null);
        }

        public Task<PagedResult<CoilManager.Application.DTOs.RawCoils.RawCoilDto>> GetPagedAsync(
            CoilManager.Application.DTOs.RawCoils.RawCoilQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<CoilManager.Application.DTOs.RawCoils.RawCoilDto>([], request.NormalizedPage, request.NormalizedPageSize, 0));
        }

        public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default) => Task.FromResult<RawCoil?>(null);
        public Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeSlitCoilRepository(
        RawCoil motherCoil,
        SlittingJob job,
        Grade grade,
        Supplier supplier,
        Manufacturer manufacturer) : RepositoryFake<SlitCoil>, ISlitCoilRepository
    {
        public List<SlitCoil> Added { get; } = [];

        public override Task AddAsync(SlitCoil entity, CancellationToken cancellationToken = default)
        {
            SetPrivateProperty(entity, nameof(SlitCoil.MotherCoil), motherCoil);
            SetPrivateProperty(entity, nameof(SlitCoil.SlittingJob), job);
            SetPrivateProperty(entity, nameof(SlitCoil.Grade), grade);
            SetPrivateProperty(entity, nameof(SlitCoil.Supplier), supplier);
            SetPrivateProperty(entity, nameof(SlitCoil.Manufacturer), manufacturer);
            Added.Add(entity);
            return Task.CompletedTask;
        }

        public Task<PagedResult<SlitCoilDto>> GetPagedAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<SlitCoilDto>([], request.NormalizedPage, request.NormalizedPageSize, 0));
        }

        public Task<SlitCoil?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Added.FirstOrDefault(coil => coil.Id == id));
        }

        public Task<SlitCoil?> GetByNumberWithDetailsAsync(string coilNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Added.FirstOrDefault(coil => coil.CoilNumber == coilNumber));
        }

        public Task<bool> ExistsByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Added.Any(coil => coil.CoilNumber == coilNumber));
        }

        public Task<IReadOnlyList<SlitCoil>> GetBySlittingJobIdAsync(Guid slittingJobId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SlitCoil>>(Added.Where(coil => coil.SlittingJobId == slittingJobId).ToArray());
        }
    }

    private sealed class FakeInventoryTransactionRepository : RepositoryFake<InventoryTransaction>, IInventoryTransactionRepository
    {
        public List<InventoryTransaction> Added { get; } = [];

        public override Task AddAsync(InventoryTransaction entity, CancellationToken cancellationToken = default)
        {
            Added.Add(entity);
            return Task.CompletedTask;
        }
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

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public IRepository<TEntity> Repository<TEntity>()
            where TEntity : BaseEntity
        {
            throw new NotSupportedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            await operation(cancellationToken);
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId => "unit-test-user";
        public string? UserName => "Unit Tester";
        public bool IsAuthenticated => true;
    }

    private static void SetPrivateProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property '{propertyName}' was not found.");
        property.SetValue(target, value);
    }
}
