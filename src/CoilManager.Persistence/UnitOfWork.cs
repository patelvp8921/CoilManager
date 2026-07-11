using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Common;
using CoilManager.Persistence.Repositories;

namespace CoilManager.Persistence;

public sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity
    {
        Type entityType = typeof(TEntity);

        if (_repositories.TryGetValue(entityType, out object? repository))
        {
            return (IRepository<TEntity>)repository;
        }

        Repository<TEntity> createdRepository = new(dbContext);
        _repositories[entityType] = createdRepository;

        return createdRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
