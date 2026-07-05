using CoilManager.Domain.Common;

namespace CoilManager.Application.Interfaces;

public interface IRepository<TEntity> : Repositories.IRepository<TEntity>
    where TEntity : BaseEntity;
