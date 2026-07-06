using CoilManager.Application.DTOs.Lookups;

namespace CoilManager.Application.Interfaces.Services;

public interface ILookupService
{
    Task<IReadOnlyList<LookupItemDto>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetActiveManufacturersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LookupItemDto>> GetActiveGradesAsync(CancellationToken cancellationToken = default);
}
