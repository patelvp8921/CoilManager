using CoilManager.Domain.Common;

namespace CoilManager.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    CoilManager.Application.Interfaces.Repositories.IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
