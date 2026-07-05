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
}
