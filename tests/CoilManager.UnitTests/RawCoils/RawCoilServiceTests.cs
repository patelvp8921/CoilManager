using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Services;
using CoilManager.Application.Specifications;
using CoilManager.Application.Validators.RawCoils;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Results;

namespace CoilManager.UnitTests.RawCoils;

public sealed class RawCoilServiceTests
{
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

    private static RawCoilService CreateService(FakeRawCoilRepository repository)
    {
        return new RawCoilService(
            repository,
            new FakeUnitOfWork(repository),
            mapper: null!,
            new CreateRawCoilRequestValidator(),
            new UpdateRawCoilRequestValidator());
    }

    private static CreateRawCoilRequest ValidCreateRequest()
    {
        return new CreateRawCoilRequest(
            "CN-001",
            "HN-001",
            "Prime Mill",
            "TC-001",
            "BIS-001",
            "Prime Supplier",
            "23HP85D",
            null,
            null,
            10,
            0,
            null,
            "A1",
            DateOnly.FromDateTime(DateTime.UtcNow));
    }

    private sealed class FakeRawCoilRepository : IRawCoilRepository
    {
        public bool CoilNumberExists { get; init; }

        public IQueryable<RawCoil> Query() => Array.Empty<RawCoil>().AsQueryable();

        public Task<RawCoil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RawCoil?>(null);
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
    }
}
