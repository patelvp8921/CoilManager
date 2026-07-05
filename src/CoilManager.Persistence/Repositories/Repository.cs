using CoilManager.Domain.Common;

namespace CoilManager.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext dbContext) : GenericRepository<TEntity>(dbContext)
    where TEntity : BaseEntity;
