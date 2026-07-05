using CoilManager.Application.DTOs.RawCoils;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Services;

public interface IRawCoilService
{
    Task<PagedResult<RawCoilDto>> GetAsync(RawCoilQueryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RawCoilDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<RawCoilDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<RawCoilDto>> CreateAsync(CreateRawCoilRequest request, CancellationToken cancellationToken = default);
    Task<Result<RawCoilDto>> UpdateAsync(Guid id, UpdateRawCoilRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
