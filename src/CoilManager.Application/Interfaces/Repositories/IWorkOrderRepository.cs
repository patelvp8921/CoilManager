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
    Task<SalesOrder?> GetSalesOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RawCoil?> GetMotherCoilAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SlitCoil?> GetSlitCoilAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetActiveReservedWeightAsync(CoilType type, Guid coilId, Guid? excludingAllocationId = null, CancellationToken cancellationToken = default);
    Task AddAllocationAsync(WorkOrderMaterialAllocation allocation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(decimal thickness, decimal? width, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(decimal thickness, decimal? width, IReadOnlyCollection<Guid> excludedCoilIds, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkOrder>> GetForDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlittingJob>> GetLinkedSlittingJobsAsync(Guid workOrderId, CancellationToken cancellationToken = default);
    Task<LaminationJob?> GetLinkedLaminationJobAsync(Guid workOrderId, CancellationToken cancellationToken = default);
    Task<int> GetMaximumLaminationJobSequenceAsync(int year, CancellationToken cancellationToken = default);
    Task AddLaminationJobAsync(LaminationJob job, CancellationToken cancellationToken = default);
}
