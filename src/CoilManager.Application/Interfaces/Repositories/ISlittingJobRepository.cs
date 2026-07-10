using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Domain.Entities;
using CoilManager.Shared.Pagination;

namespace CoilManager.Application.Interfaces.Repositories;

public interface ISlittingJobRepository : IRepository<SlittingJob>
{
    Task<PagedResult<SlittingJobDto>> GetPagedAsync(SlittingJobQueryRequest request, CancellationToken cancellationToken = default);
    new Task<SlittingJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<bool> ExistsByJobNumberAsync(string slittingJobNo, CancellationToken cancellationToken = default);
    Task<bool> DraftExistsForMotherCoilAsync(Guid motherCoilId, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<Guid>> GetDraftMotherCoilIdsAsync(CancellationToken cancellationToken = default);
    Task DeleteItemsForRebuildAsync(SlittingJob job, CancellationToken cancellationToken = default);
    void TrackRebuiltItemsAsAdded(IEnumerable<SlittingJobItem> items);
}
