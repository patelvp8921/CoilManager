using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Common;

namespace CoilManager.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
