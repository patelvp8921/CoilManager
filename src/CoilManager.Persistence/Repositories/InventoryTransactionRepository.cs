using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;

namespace CoilManager.Persistence.Repositories;

public sealed class InventoryTransactionRepository(ApplicationDbContext dbContext)
    : Repository<InventoryTransaction>(dbContext), IInventoryTransactionRepository
{
}
