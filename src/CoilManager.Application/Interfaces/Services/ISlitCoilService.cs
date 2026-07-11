using CoilManager.Application.DTOs.SlitCoils;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Services;

public interface ISlitCoilService
{
    Task<PagedResult<SlitCoilDto>> GetAsync(SlitCoilQueryRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlitCoilDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SlitCoilDto>> GetByNumberAsync(string coilNumber, CancellationToken cancellationToken = default);
    Task<Result<SlitCoilGenealogyDto>> GetGenealogyAsync(Guid id, CancellationToken cancellationToken = default);
}
