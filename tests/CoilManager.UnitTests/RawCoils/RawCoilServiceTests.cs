using AutoMapper;
using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Mappings;
using CoilManager.Application.Services;
using CoilManager.Application.Specifications;
using CoilManager.Application.Validators.RawCoils;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoilManager.UnitTests.RawCoils;

public sealed class RawCoilServiceTests
{
    private static readonly Supplier TestSupplier = new("Prime Supplier", "PS");
    private static readonly Manufacturer TestManufacturer = new("Prime Manufacturer", "PM");
    private static readonly Grade TestGrade = new("23HP85D", 0.23m, 0.85m);

    [Fact]
    public async Task CreateAsync_ReturnsConflict_WhenCoilNumberExists()
    {
        FakeRawCoilRepository repository = new() { CoilNumberExists = true };
        RawCoilService service = CreateService(repository);

        Result<RawCoilDto> result = await service.CreateAsync(ValidCreateRequest());

        Assert.True(result.IsFailure);
        Assert.Equal("Conflict", result.Error.Code);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenRawCoilDoesNotExist()
    {
        RawCoilService service = CreateService(new FakeRawCoilRepository());

        Result<RawCoilDto> result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task GetAsync_UsesPaginationDefaults_WhenRequestValuesAreInvalid()
    {
        RawCoilService service = CreateService(new FakeRawCoilRepository());
        RawCoilQueryRequest request = new() { Page = -10, PageSize = 0 };

        CoilManager.Shared.Pagination.PagedResult<RawCoilDto> result = await service.GetAsync(request);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(25, result.PageSize);
    }

    [Fact]
    public void RawCoilNumberGenerator_UsesRequestedFormat()
    {
        string rawCoilNumber = RawCoilNumberGenerator.Generate(2026, 1);

        Assert.Equal("MC-2026-0000001", rawCoilNumber);
    }

    [Fact]
    public async Task GetNextRawCoilNumberAsync_SkipsExistingGeneratedRawCoilNumber()
    {
        int currentYear = DateTime.UtcNow.Year;
        FakeRawCoilRepository repository = new()
        {
            ExistingRawCoilNumbers = new HashSet<string> { RawCoilNumberGenerator.Generate(currentYear, 1) }
        };
        RawCoilService service = CreateService(repository);

        string result = await service.GetNextRawCoilNumberAsync();

        Assert.Equal(RawCoilNumberGenerator.Generate(currentYear, 2), result);
    }

    [Fact]
    public async Task CreateAsync_CopiesThicknessCategoryAndCoreLossFromGrade()
    {
        Grade selectedGrade = new("27HP90D", 0.27m, 0.90m);
        FakeRawCoilRepository repository = new();
        RawCoilService service = CreateService(repository, selectedGrade, CreateMapper());
        CreateRawCoilRequest request = ValidCreateRequest(selectedGrade) with
        {
            Thickness = 0.35m,
            WattLossPerKg = 1.50m
        };

        Result<RawCoilDto> result = await service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.AddedRawCoil);
        Assert.Equal(0.27m, repository.AddedRawCoil.Thickness);
        Assert.Equal("M4", repository.AddedRawCoil.Category);
        Assert.Equal(0.90m, repository.AddedRawCoil.CoreLossPerKg);
        Assert.Equal(0.27m, result.Value.Thickness);
        Assert.Equal("M4", result.Value.Category);
        Assert.Equal(0.90m, result.Value.CoreLossPerKg);
    }

    private static RawCoilService CreateService(FakeRawCoilRepository repository, Grade? grade = null, IMapper? mapper = null)
    {
        return new RawCoilService(
            repository,
            new FakeRepository<Supplier>([TestSupplier]),
            new FakeRepository<Manufacturer>([TestManufacturer]),
            new FakeRepository<Grade>([grade ?? TestGrade]),
            new FakeUnitOfWork(repository),
            mapper: mapper ?? null!,
            new CreateRawCoilRequestValidator(),
            new UpdateRawCoilRequestValidator());
    }

    private static CreateRawCoilRequest ValidCreateRequest(Grade? grade = null)
    {
        grade ??= TestGrade;

        return new CreateRawCoilRequest(
            "CN-001",
            "HN-001",
            "PO-001",
            "INV-001",
            "TC-001",
            "BIS-001",
            TestSupplier.Id,
            TestManufacturer.Id,
            grade.Id,
            null,
            null,
            10,
            0,
            null,
            "A1",
            DateOnly.FromDateTime(DateTime.UtcNow));
    }

    private static IMapper CreateMapper()
    {
        MapperConfiguration configuration = new(config => config.AddProfile<RawCoilMappingProfile>(), NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }

    private sealed class FakeRawCoilRepository : IRawCoilRepository
    {
        public bool CoilNumberExists { get; init; }
        public RawCoil? AddedRawCoil { get; private set; }

        public IQueryable<RawCoil> Query() => Array.Empty<RawCoil>().AsQueryable();

        public Task<RawCoil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RawCoil?>(null);
        }

        public Task<CoilManager.Shared.Pagination.PagedResult<RawCoilDto>> GetPagedAsync(
            RawCoilQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CoilManager.Shared.Pagination.PagedResult<RawCoilDto>(
                [],
                request.NormalizedPage,
                request.NormalizedPageSize,
                0));
        }

        public Task<IReadOnlyList<RawCoil>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RawCoil>>([]);
        }

        public Task<IReadOnlyList<RawCoil>> GetAllAsync(ISpecification<RawCoil> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<RawCoil>>([]);
        }

        public Task<RawCoil?> FirstOrDefaultAsync(ISpecification<RawCoil> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RawCoil?>(null);
        }

        public Task<int> CountAsync(ISpecification<RawCoil>? specification = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task AddAsync(RawCoil entity, CancellationToken cancellationToken = default)
        {
            AddedRawCoil = entity;
            return Task.CompletedTask;
        }

        public void Update(RawCoil entity)
        {
        }

        public void Delete(RawCoil entity)
        {
        }

        public void Remove(RawCoil entity)
        {
        }

        public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RawCoil?>(null);
        }

        public Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CoilNumberExists);
        }

        public Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public IReadOnlySet<string> ExistingRawCoilNumbers { get; init; } = new HashSet<string>();

        public Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingRawCoilNumbers.Contains(rawCoilNumber));
        }
    }

    private sealed class FakeRepository<TEntity>(IReadOnlyList<TEntity> entities) : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        public IQueryable<TEntity> Query() => entities.AsQueryable();

        public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entities.FirstOrDefault(entity => entity.Id == id));
        }

        public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entities);
        }

        public Task<IReadOnlyList<TEntity>> GetAllAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entities);
        }

        public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TEntity?>(entities.FirstOrDefault());
        }

        public Task<int> CountAsync(ISpecification<TEntity>? specification = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(entities.Count);
        }

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Update(TEntity entity)
        {
        }

        public void Delete(TEntity entity)
        {
        }

        public void Remove(TEntity entity)
        {
        }
    }

    private sealed class FakeUnitOfWork(IRepository<RawCoil> repository) : IUnitOfWork
    {
        public IRepository<TEntity> Repository<TEntity>()
            where TEntity : BaseEntity
        {
            return (IRepository<TEntity>)repository;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            await operation(cancellationToken);
        }
    }
}
