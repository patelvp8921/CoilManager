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
        var projected = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((request.NormalizedPage - 1) * request.NormalizedPageSize).Take(request.NormalizedPageSize)
            .Select(x => new
            {
                x.Id, x.WorkOrderNumber, x.WorkOrderType, x.ProductType, x.CustomerName, x.SalesOrderReference,
                x.RequiredDate, x.Priority, x.Status, x.CreatedAtUtc, x.RequiredWeight, x.RequiredQuantity,
                x.PlanningRequiredQuantity, x.QuantityUnit,
                Reserved = x.Allocations.Where(a => Active.Contains(a.Status)).Sum(a => (decimal?)a.AllocatedWeight) ?? 0,
                RequiredOperations = x.Operations.Count(o => o.IsRequired),
                CompletedOperations = x.Operations.Count(o => o.IsRequired && o.Status == WorkOrderOperationStatus.Completed),
                LaminationJob = db.LaminationJobs.Where(j => j.WorkOrderId == x.Id).Select(j => new
                {
                    j.Id, j.LaminationJobNumber, j.Status, j.TotalWeight, j.TotalAllocatedWeight
                }).FirstOrDefault()
            }).ToListAsync(cancellationToken);

        var rows = projected.Select(x =>
        {
            decimal required = x.PlanningRequiredQuantity > 0 ? x.PlanningRequiredQuantity : x.RequiredWeight ?? x.RequiredQuantity ?? 0;
            decimal remaining = Math.Max(0, required - x.Reserved);
            decimal progress = x.RequiredOperations == 0 ? 100 : 100m * x.CompletedOperations / x.RequiredOperations;
            string actionType; string actionLabel; string? route;
            if (x.Status == WorkOrderStatus.Draft)
            {
                actionType = "ReleaseWorkOrder"; actionLabel = "Release"; route = null;
            }
            else if (x.Status is WorkOrderStatus.Ready or WorkOrderStatus.PartiallyDispatched)
            {
                actionType = "CreateDispatch"; actionLabel = x.Status == WorkOrderStatus.Ready ? "Create Dispatch" : "Create Another Dispatch"; route = $"/dispatch-create?workOrderId={x.Id}";
            }
            else if (x.ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
            {
                actionType = x.Reserved <= 0 ? (x.ProductType == WorkOrderProductType.MotherCoil ? "AllocateMotherCoil" : "AllocateSlitCoil")
                    : remaining > 0 ? (x.ProductType == WorkOrderProductType.MotherCoil ? "ContinueMotherCoilAllocation" : "ContinueSlitCoilAllocation") : "ViewAllocation";
                actionLabel = x.Reserved <= 0 ? "Allocate Material" : remaining > 0 ? "Continue Allocation" : "View Allocation";
                route = $"/work-orders/{x.Id}/material-allocation";
            }
            else if (x.ProductType == WorkOrderProductType.Lamination && x.LaminationJob is null)
            {
                actionType = "RecoverMissingLaminationJob"; actionLabel = "Create Lamination Job"; route = null;
            }
            else if (x.LaminationJob!.Status == LaminationJobStatus.Draft)
            {
                actionType = "CompleteLaminationJobSetup"; actionLabel = "Complete Lamination Job Setup"; route = $"/lamination-jobs/{x.LaminationJob.Id}/edit";
            }
            else if (x.LaminationJob.Status == LaminationJobStatus.Released && x.LaminationJob.TotalAllocatedWeight < x.LaminationJob.TotalWeight)
            {
                actionType = "AllocateLaminationMaterial"; actionLabel = "Allocate Lamination Material"; route = $"/lamination-jobs/{x.LaminationJob.Id}/material-allocation";
            }
            else
            {
                actionType = "ViewLaminationJob"; actionLabel = "View Lamination Job"; route = $"/lamination-jobs/{x.LaminationJob.Id}";
            }
            return new WorkOrderListItemDto(x.Id, x.WorkOrderNumber, x.WorkOrderType, x.ProductType,
                x.CustomerName, x.SalesOrderReference, x.RequiredDate, x.Priority, x.Status, progress, x.CreatedAtUtc,
                required, x.QuantityUnit, x.Reserved, remaining, required <= 0 ? 0 : Math.Min(100, 100 * x.Reserved / required),
                x.LaminationJob?.Id, x.LaminationJob?.LaminationJobNumber, x.LaminationJob?.Status,
                actionType, actionLabel, route);
        }).ToArray();
        return new(rows, request.NormalizedPage, request.NormalizedPageSize, total);
    }

    public new Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<WorkOrder?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) => BaseQuery().FirstOrDefaultAsync(x => x.WorkOrderNumber == number, cancellationToken);
    public async Task<int> GetMaximumSequenceAsync(int year, CancellationToken cancellationToken = default)
    {
        string slashPrefix = $"WO/{year}/";
        string legacyPrefix = $"WO-{year}-";
        string[] numbers = await db.WorkOrders
            .Where(x => x.WorkOrderNumber.StartsWith(slashPrefix) || x.WorkOrderNumber.StartsWith(legacyPrefix))
            .Select(x => x.WorkOrderNumber).ToArrayAsync(cancellationToken);
        return numbers.Select(x => x.Length >= 5 && int.TryParse(x[^5..], out int sequence) ? sequence : 0).DefaultIfEmpty().Max();
    }
    public Task<bool> NumberExistsAsync(string number, CancellationToken cancellationToken = default) => db.WorkOrders.AnyAsync(x => x.WorkOrderNumber == number, cancellationToken);
    public Task<RawCoil?> GetMotherCoilAsync(Guid id, CancellationToken cancellationToken = default) => db.RawCoils.Include(x => x.Grade).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<SalesOrder?> GetSalesOrderAsync(Guid id, CancellationToken cancellationToken = default) => db.SalesOrders.Include(x => x.Customer).Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<SlitCoil?> GetSlitCoilAsync(Guid id, CancellationToken cancellationToken = default) => db.SlitCoils.Include(x => x.Grade).Include(x => x.MotherCoil).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<decimal> GetActiveReservedWeightAsync(CoilType type, Guid coilId, Guid? excludingAllocationId = null, CancellationToken cancellationToken = default)
        => await db.WorkOrderMaterialAllocations.Where(x => Active.Contains(x.Status) && x.Id != excludingAllocationId &&
            (type == CoilType.MotherCoil ? x.MotherCoilId == coilId : x.SlitCoilId == coilId)).SumAsync(x => (decimal?)x.AllocatedWeight, cancellationToken) ?? 0;

    public Task AddAllocationAsync(WorkOrderMaterialAllocation allocation, CancellationToken cancellationToken = default)
    {
        db.Entry(allocation).State = EntityState.Added;
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(decimal thickness, decimal? width, string? search, CancellationToken cancellationToken = default)
    {
        decimal minimumThickness = thickness - 0.001m;
        decimal maximumThickness = thickness + 0.001m;
        decimal? minimumWidth = width - 0.01m;
        decimal? maximumWidth = width + 0.01m;
        var query = db.RawCoils.AsNoTracking().Include(x => x.Grade).Where(x =>
            x.Status == CoilStatus.Available &&
            x.Thickness >= minimumThickness && x.Thickness <= maximumThickness &&
            (!width.HasValue || (x.Width >= minimumWidth!.Value && x.Width <= maximumWidth!.Value)));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.RawCoilNumber.Contains(search) || x.CoilNumber.Contains(search));
        query = query.Where(x => x.Weight > (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.MotherCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0));
        query = query.OrderBy(x => x.RawCoilNumber);
        return await query.Select(x => new AvailableCoilDto(x.Id, CoilType.MotherCoil, x.RawCoilNumber, null, x.Grade != null ? x.Grade.Code : null,
            x.Thickness, x.Width, x.Weight,
            db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.MotherCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0,
            x.Weight - (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.MotherCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0), x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(decimal thickness, decimal? width, IReadOnlyCollection<Guid> excludedCoilIds, string? search, CancellationToken cancellationToken = default)
    {
        decimal minimumThickness = thickness - 0.001m;
        decimal maximumThickness = thickness + 0.001m;
        decimal? minimumWidth = width - 0.01m;
        decimal? maximumWidth = width + 0.01m;
        var query = db.SlitCoils.AsNoTracking().Include(x => x.Grade).Include(x => x.MotherCoil)
            .Where(x => x.Status == CoilStatus.Available &&
                x.Thickness >= minimumThickness && x.Thickness <= maximumThickness &&
                (!width.HasValue || (x.Width >= minimumWidth!.Value && x.Width <= maximumWidth!.Value)) &&
                !excludedCoilIds.Contains(x.Id));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.CoilNumber.Contains(search));
        query = query.Where(x => x.Weight > (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.SlitCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0));
        query = query.OrderBy(x => x.CoilNumber);
        return await query.Select(x => new AvailableCoilDto(x.Id, CoilType.SlitCoil, x.CoilNumber, x.MotherCoil != null ? x.MotherCoil.RawCoilNumber : null,
            x.Grade != null ? x.Grade.Code : null, x.Thickness, x.Width, x.Weight,
            db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.SlitCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0,
            x.Weight - (db.WorkOrderMaterialAllocations.Where(a => Active.Contains(a.Status) && a.SlitCoilId == x.Id).Sum(a => (decimal?)a.AllocatedWeight) ?? 0), x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrder>> GetForDashboardAsync(CancellationToken cancellationToken = default) => await BaseQuery().AsNoTracking().ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<SlittingJob>> GetLinkedSlittingJobsAsync(Guid workOrderId, CancellationToken cancellationToken = default) => await db.SlittingJobs.AsNoTracking().Where(x => x.WorkOrderId == workOrderId).ToListAsync(cancellationToken);
    public Task<LaminationJob?> GetLinkedLaminationJobAsync(Guid workOrderId, CancellationToken cancellationToken = default) => db.LaminationJobs.Include(x => x.Allocations).FirstOrDefaultAsync(x => x.WorkOrderId == workOrderId, cancellationToken);
    public async Task<int> GetMaximumLaminationJobSequenceAsync(int year, CancellationToken cancellationToken = default)
    {
        string prefix = $"AE/C/{year}/";
        string? max = await db.LaminationJobs.Where(x => x.LaminationJobNumber.StartsWith(prefix)).Select(x => x.LaminationJobNumber).OrderByDescending(x => x).FirstOrDefaultAsync(cancellationToken);
        return max is not null && int.TryParse(max[prefix.Length..], out int sequence) ? sequence : 0;
    }
    public Task AddLaminationJobAsync(LaminationJob job, CancellationToken cancellationToken = default) { db.LaminationJobs.Add(job); return Task.CompletedTask; }
    private IQueryable<WorkOrder> BaseQuery() => db.WorkOrders.Include(x => x.Grade).Include(x => x.Operations).Include(x => x.Allocations);
}
