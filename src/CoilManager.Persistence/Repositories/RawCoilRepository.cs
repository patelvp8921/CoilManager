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

    public new Task<RawCoil?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
            .FirstOrDefaultAsync(rawCoil => rawCoil.Id == id, cancellationToken);
    }

    public new async Task<IReadOnlyList<RawCoil>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
            .ToListAsync(cancellationToken);
    }

    public Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .Include(rawCoil => rawCoil.Supplier)
            .Include(rawCoil => rawCoil.Manufacturer)
            .Include(rawCoil => rawCoil.Grade)
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

    public Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default)
    {
        string prefix = $"RC-{year}-";

        return _dbContext.RawCoils
            .CountAsync(rawCoil => rawCoil.RawCoilNumber.StartsWith(prefix), cancellationToken);
    }

    public Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawCoils
            .AnyAsync(rawCoil => rawCoil.RawCoilNumber == rawCoilNumber, cancellationToken);
    }
}
