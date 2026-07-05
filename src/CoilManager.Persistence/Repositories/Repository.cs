using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Remove(entity);
    }
}
