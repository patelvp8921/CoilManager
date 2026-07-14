using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Pagination;

namespace CoilManager.Application.Interfaces.Repositories;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<PagedResult<WorkOrderListItemDto>> GetPagedAsync(WorkOrderQueryRequest request, CancellationToken cancellationToken = default);
    new Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrder?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task<int> GetMaximumSequenceAsync(int year, CancellationToken cancellationToken = default);
    Task<bool> NumberExistsAsync(string number, CancellationToken cancellationToken = default);
    Task<RawCoil?> GetMotherCoilAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SlitCoil?> GetSlitCoilAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetActiveReservedWeightAsync(CoilType type, Guid coilId, Guid? excludingAllocationId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetForDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlittingJob>> GetLinkedSlittingJobsAsync(Guid workOrderId, CancellationToken cancellationToken = default);
}
