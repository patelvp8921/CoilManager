using CoilManager.Domain.Entities;

namespace CoilManager.Application.Interfaces.Repositories;

public interface ISlitCoilLabelPrintHistoryRepository : IRepository<SlitCoilLabelPrintHistory>
{
    Task<IReadOnlyList<SlitCoilLabelPrintHistory>> GetBySlitCoilIdAsync(Guid slitCoilId, CancellationToken cancellationToken = default);
}
