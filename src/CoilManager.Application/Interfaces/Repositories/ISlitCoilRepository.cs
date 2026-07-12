using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Pagination;

namespace CoilManager.Application.Interfaces.Repositories;

public interface ISlitCoilRepository : IRepository<SlitCoil>
{
    Task<PagedResult<SlitCoilListItemDto>> GetPagedAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default);
    Task<SlitCoil?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SlitCoil?> GetByNumberWithDetailsAsync(string coilNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCoilNumberAsync(string coilNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlitCoil>> GetBySlittingJobIdAsync(Guid slittingJobId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlitCoil>> GetByMotherCoilIdAsync(Guid motherCoilId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlitCoil>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}
