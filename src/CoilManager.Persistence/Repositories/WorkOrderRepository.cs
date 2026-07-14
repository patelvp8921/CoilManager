using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Repositories;

public sealed class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    private readonly ApplicationDbContext db;
    public WorkOrderRepository(ApplicationDbContext dbContext) : base(dbContext) => db = dbContext;
    private static readonly AllocationStatus[] Active = [AllocationStatus.Reserved, AllocationStatus.Issued, AllocationStatus.PartiallyConsumed];

    public async Task<PagedResult<WorkOrderListItemDto>> GetPagedAsync(WorkOrderQueryRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<WorkOrder> query = db.WorkOrders.AsNoTracking().Include(x => x.Operations);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            query = query.Where(x => x.WorkOrderNumber.Contains(search) || (x.CustomerName != null && x.CustomerName.Contains(search)) || (x.SalesOrderReference != null && x.SalesOrderReference.Contains(search)));
        }
        if (request.WorkOrderType.HasValue) query = query.Where(x => x.WorkOrderType == request.WorkOrderType);
        if (request.ProductType.HasValue) query = query.Where(x => x.ProductType == request.ProductType);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.Priority.HasValue) query = query.Where(x => x.Priority == request.Priority);
        if (request.DateFrom.HasValue) query = query.Where(x => x.WorkOrderDate >= request.DateFrom);
        if (request.DateTo.HasValue) query = query.Where(x => x.WorkOrderDate <= request.DateTo);
        int total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize).Take(request.NormalizedPageSize)
            .Select(x => new WorkOrderListItemDto(x.Id, x.WorkOrderNumber, x.WorkOrderType, x.ProductType,
                x.CustomerName, x.SalesOrderReference, x.RequiredDate, x.Priority, x.Status,
                x.Operations.Count(o => o.IsRequired) == 0 ? 100 :
                    100m * x.Operations.Count(o => o.IsRequired && o.Status == WorkOrderOperationStatus.Completed) / x.Operations.Count(o => o.IsRequired),
                x.CreatedAtUtc)).ToListAsync(cancellationToken);
        return new(rows, request.NormalizedPage, request.NormalizedPageSize, total);
    }

    public new Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<WorkOrder?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) => BaseQuery().FirstOrDefaultAsync(x => x.WorkOrderNumber == number, cancellationToken);
    public async Task<int> GetMaximumSequenceAsync(int year, CancellationToken cancellationToken = default)
    {
        string prefix = $"WO-{year}-";
        string? max = await db.WorkOrders.Where(x => x.WorkOrderNumber.StartsWith(prefix)).Select(x => x.WorkOrderNumber).OrderByDescending(x => x).FirstOrDefaultAsync(cancellationToken);
        return max is not null && int.TryParse(max[(prefix.Length)..], out int sequence) ? sequence : 0;
    }
    public Task<bool> NumberExistsAsync(string number, CancellationToken cancellationToken = default) => db.WorkOrders.AnyAsync(x => x.WorkOrderNumber == number, cancellationToken);
    public Task<RawCoil?> GetMotherCoilAsync(Guid id, CancellationToken cancellationToken = default) => db.RawCoils.Include(x => x.Grade).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<SlitCoil?> GetSlitCoilAsync(Guid id, CancellationToken cancellationToken = default) => db.SlitCoils.Include(x => x.Grade).Include(x => x.MotherCoil).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<decimal> GetActiveReservedWeightAsync(CoilType type, Guid coilId, Guid? excludingAllocationId = null, CancellationToken cancellationToken = default)
        => await db.WorkOrderMaterialAllocations.Where(x => Active.Contains(x.Status) && x.Id != excludingAllocationId &&
            (type == CoilType.MotherCoil ? x.MotherCoilId == coilId : x.SlitCoilId == coilId)).SumAsync(x => (decimal?)x.AllocatedWeight, cancellationToken) ?? 0;

    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = db.RawCoils.AsNoTracking().Include(x => x.Grade).Where(x => x.Status == CoilStatus.Available);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.RawCoilNumber.Contains(search) || x.CoilNumber.Contains(search));
        return await query.Select(x => new AvailableCoilDto(x.Id, CoilType.MotherCoil, x.RawCoilNumber, null, x.Grade != null ? x.Grade.Code : null,
            x.Thickness, x.Width, x.Weight,
            db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.MotherCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0,
            x.Weight - (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.MotherCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0), x.Status))
            .Where(x => x.AvailableWeight > 0).OrderBy(x => x.CoilNumber).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = db.SlitCoils.AsNoTracking().Include(x => x.Grade).Include(x => x.MotherCoil).Where(x => x.Status == CoilStatus.Available);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.CoilNumber.Contains(search));
        return await query.Select(x => new AvailableCoilDto(x.Id, CoilType.SlitCoil, x.CoilNumber, x.MotherCoil != null ? x.MotherCoil.RawCoilNumber : null,
            x.Grade != null ? x.Grade.Code : null, x.Thickness, x.Width, x.Weight,
            db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.SlitCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0,
            x.Weight - (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.SlitCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0), x.Status))
            .Where(x => x.AvailableWeight > 0).OrderBy(x => x.CoilNumber).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrder>> GetForDashboardAsync(CancellationToken cancellationToken = default) => await BaseQuery().AsNoTracking().ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<SlittingJob>> GetLinkedSlittingJobsAsync(Guid workOrderId, CancellationToken cancellationToken = default) => await db.SlittingJobs.AsNoTracking().Where(x => x.WorkOrderId == workOrderId).ToListAsync(cancellationToken);
    private IQueryable<WorkOrder> BaseQuery() => db.WorkOrders.Include(x => x.Grade).Include(x => x.Operations).Include(x => x.Allocations);
}
