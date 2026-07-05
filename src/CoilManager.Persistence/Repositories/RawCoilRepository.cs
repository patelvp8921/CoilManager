using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class RawCoilRepository : Repository<RawCoil>, IRawCoilRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RawCoilRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .FirstOrDefaultAsync(rawCoil => rawCoil.CoilNumber == coilNumber, cancellationToken);
    }

    public Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .AnyAsync(rawCoil =>
                rawCoil.CoilNumber == coilNumber
                && (!excludingId.HasValue || rawCoil.Id != excludingId.Value),
                cancellationToken);
    }

    public Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .CountAsync(rawCoil => rawCoil.ReceivedDate.Year == year, cancellationToken);
    }
}
