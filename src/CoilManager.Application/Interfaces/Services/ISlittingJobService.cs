using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Results;

namespace CoilManager.Application.Interfaces.Services;

public interface ISlittingJobService
{
    Task<PagedResult<SlittingJobDto>> GetAsync(SlittingJobQueryRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GetNextJobNumberAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlittingMotherCoilLookupDto>> SearchMotherCoilsAsync(string? search, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobDto>> CreateAsync(CreateSlittingJobRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobDto>> UpdateAsync(Guid id, UpdateSlittingJobRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobDto>> ReleaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<StartSlittingResponse>> StartSlittingAsync(Guid id, StartSlittingRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobDto>> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CompleteSlittingResponse>> CompleteAsync(Guid id, CompleteSlittingRequest request, CancellationToken cancellationToken = default);
    Task<Result<SlittingJobCompletionDto>> GetCompletionAsync(Guid id, CancellationToken cancellationToken = default);
}
