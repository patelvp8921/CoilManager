using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class InventoryTransactionRepository : Repository<InventoryTransaction>, IInventoryTransactionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryTransactionRepository(ApplicationDbContext dbContext) : base(dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<InventoryTransaction>> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default) =>
        await _dbContext.InventoryTransactions.AsNoTracking()
            .Where(transaction => transaction.CoilNumber == coilNumber)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToArrayAsync(cancellationToken);
}
