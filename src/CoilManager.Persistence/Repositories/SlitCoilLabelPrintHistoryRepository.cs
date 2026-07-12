using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class SlitCoilLabelPrintHistoryRepository : Repository<SlitCoilLabelPrintHistory>, ISlitCoilLabelPrintHistoryRepository
{
    private readonly ApplicationDbContext _dbContext;
    public SlitCoilLabelPrintHistoryRepository(ApplicationDbContext dbContext) : base(dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<SlitCoilLabelPrintHistory>> GetBySlitCoilIdAsync(Guid slitCoilId, CancellationToken cancellationToken = default) =>
        await _dbContext.SlitCoilLabelPrintHistories.AsNoTracking().Where(row => row.SlitCoilId == slitCoilId)
            .OrderByDescending(row => row.PrintedOn).ToArrayAsync(cancellationToken);
}
