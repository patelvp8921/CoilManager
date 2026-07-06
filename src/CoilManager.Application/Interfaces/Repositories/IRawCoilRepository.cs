using CoilManager.Domain.Entities;

namespace CoilManager.Application.Interfaces.Repositories;

public interface IRawCoilRepository : IRepository<RawCoil>
{
    Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCoilNumberAsync(string coilNumber, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task<int> CountByReceivedYearAsync(int year, CancellationToken cancellationToken = default);
    Task<int> CountByRawCoilYearAsync(int year, CancellationToken cancellationToken = default);
    Task<bool> ExistsByRawCoilNumberAsync(string rawCoilNumber, CancellationToken cancellationToken = default);
}
