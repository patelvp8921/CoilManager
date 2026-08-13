using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Shared.Pagination;

namespace CoilManager.Application.Interfaces.Services;

public interface IWorkOrderService
{
    Task<PagedResult<WorkOrderListItemDto>> GetAsync(WorkOrderQueryRequest request, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> CreateAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> UpdateAsync(Guid id, UpdateWorkOrderRequest request, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> ReleaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> RecoverLaminationJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> StartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> CloseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderDetailsDto> SetSlittingRequirementAsync(Guid id, SetSlittingRequirementRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrderMaterialAllocationDto>> GetAllocationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderMaterialAllocationDto> AllocateAsync(Guid id, CreateMaterialAllocationRequest request, CancellationToken cancellationToken = default);
    Task<WorkOrderMaterialAllocationDto> UpdateAllocationAsync(Guid id, Guid allocationId, UpdateWorkOrderInventoryAllocationRequest request, CancellationToken cancellationToken = default);
    Task ReleaseAllocationAsync(Guid id, Guid allocationId, ReleaseMaterialAllocationRequest? request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(Guid id, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(Guid id, string? search, CancellationToken cancellationToken = default);
    Task<WorkOrderMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}
