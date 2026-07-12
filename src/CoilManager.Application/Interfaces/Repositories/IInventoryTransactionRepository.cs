using CoilManager.Domain.Entities;

namespace CoilManager.Application.Interfaces.Repositories;

public interface IInventoryTransactionRepository : IRepository<InventoryTransaction>
{
    Task<IReadOnlyList<InventoryTransaction>> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default);
}
