using CoilManager.Domain.Entities;

namespace CoilManager.Application.Interfaces.Repositories;

public interface IRawCoilRepository : IRepository<RawCoil>
{
    Task<RawCoil?> GetByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default);
}
