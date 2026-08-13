using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Exceptions;
using CoilManager.Shared.Pagination;

namespace CoilManager.Application.Services;

public sealed class WorkOrderService(IWorkOrderRepository repository, IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IWorkOrderService
{
    public Task<PagedResult<WorkOrderListItemDto>> GetAsync(WorkOrderQueryRequest request, CancellationToken cancellationToken = default) => repository.GetPagedAsync(request, cancellationToken);
    public async Task<WorkOrderDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await MapAsync(await FindAsync(id, cancellationToken), cancellationToken);
    public async Task<WorkOrderDetailsDto> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
        => await MapAsync(await repository.GetByNumberAsync(number, cancellationToken) ?? throw new NotFoundException($"Work Order '{number}' was not found."), cancellationToken);
    public async Task<string> GetNextNumberAsync(CancellationToken cancellationToken = default)
    {
        int year = DateTime.UtcNow.Year;
        return FormatNumber(year, await repository.GetMaximumSequenceAsync(year, cancellationToken) + 1);
    }
    public static string FormatNumber(int year, int sequence) => $"WO/{year}/{sequence:00000}";

    public async Task<WorkOrderDetailsDto> CreateAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        WorkOrder? workOrder = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            string number = await GetNextNumberAsync(ct);
            while (await repository.NumberExistsAsync(number, ct)) number = FormatNumber(DateTime.UtcNow.Year, int.Parse(number[^5..]) + 1);
            try
            {
                workOrder = Build(number, request);
                await ApplySalesOrderLineAsync(workOrder, request, ct);
                ValidateWorkOrder(workOrder);
            }
            catch (NotSupportedException ex) { throw new BusinessRuleException(ex.Message); }
            workOrder.SetCreatedAudit(Actor(), DateTimeOffset.UtcNow);
            await repository.AddAsync(workOrder, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
        ArgumentNullException.ThrowIfNull(workOrder);
        return await MapAsync(workOrder, cancellationToken);
    }

    public async Task<WorkOrderDetailsDto> UpdateAsync(Guid id, UpdateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        WorkOrder wo = await FindAsync(id, cancellationToken); EnsureRowVersion(wo, request.RowVersion);
        try { wo.Update(request.WorkOrderType, request.ProductType, request.CustomerName, request.SalesOrderReference, request.WorkOrderDate,
            request.RequiredDate, request.Priority, request.GradeId, request.Thickness, request.Category, request.CoreLossPerKg,
            request.DrawingNumber, request.RequiredWidth, request.RequiredWeight, request.RequiredQuantity, request.Remarks);
            ApplyFulfilmentPlan(wo, request);
            await ApplySalesOrderLineAsync(wo, request, cancellationToken);
            ValidateWorkOrder(wo); }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException) { throw new BusinessRuleException(ex.Message); }
        wo.SetUpdatedAudit(Actor(), DateTimeOffset.UtcNow); await unitOfWork.SaveChangesAsync(cancellationToken); return await MapAsync(wo, cancellationToken);
    }

    public async Task<WorkOrderDetailsDto> ReleaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        WorkOrder? released = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            WorkOrder wo = await FindAsync(id, ct);
            LaminationJob? linked = wo.ProductType == WorkOrderProductType.Lamination
                ? await repository.GetLinkedLaminationJobAsync(wo.Id, ct)
                : null;

            if (wo.Status != WorkOrderStatus.Draft)
            {
                if (wo.ProductType == WorkOrderProductType.Lamination && linked is not null &&
                    wo.Status is WorkOrderStatus.Released or WorkOrderStatus.InFulfilment or WorkOrderStatus.PartiallyReady or WorkOrderStatus.Ready)
                {
                    released = wo;
                    return;
                }
                throw new BusinessRuleException($"Invalid Work Order transition from {wo.Status}; expected Draft.");
            }

            if (wo.ProductType == WorkOrderProductType.Lamination && linked is null)
            {
                ValidateLaminationSpecification(wo);
                int year = DateTime.UtcNow.Year;
                string number = LaminationJob.FormatNumber(year, await repository.GetMaximumLaminationJobSequenceAsync(year, ct) + 1);
                linked = new LaminationJob(number, wo.DrawingNumber, wo.CustomerName,
                    wo.TransformerRating!, LaminationDesignType.Simple, StepLapOrientation.NotApplicable, 1,
                    wo.GradeId!.Value, wo.Thickness, wo.Category, wo.CoreLossPerKg,
                    wo.RequiredWeight ?? wo.PlanningRequiredQuantity, wo.CoreLossPerKg, wo.Id, wo.WorkOrderNumber,
                    wo.PlannedStartDate ?? wo.WorkOrderDate, wo.RequiredDate, null, wo.Planner, wo.Remarks);
                linked.SetCreatedAudit(Actor(), DateTimeOffset.UtcNow);
                await repository.AddLaminationJobAsync(linked, ct);
                wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Lamination).LinkDocument(linked.Id, linked.LaminationJobNumber);
            }

            try { wo.Release(Actor(), DateTimeOffset.UtcNow); }
            catch (InvalidOperationException ex) { throw new BusinessRuleException(ex.Message); }
            await unitOfWork.SaveChangesAsync(ct);
            released = wo;
        }, cancellationToken);
        return await MapAsync(released!, cancellationToken);
    }
    public async Task<WorkOrderDetailsDto> RecoverLaminationJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        WorkOrder? workOrder = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            WorkOrder wo = await FindAsync(id, ct);
            if (wo.ProductType != WorkOrderProductType.Lamination) throw new BusinessRuleException("Only Lamination Work Orders can create a Lamination Job.");
            if (wo.Status is not (WorkOrderStatus.Released or WorkOrderStatus.InFulfilment or WorkOrderStatus.PartiallyReady)) throw new BusinessRuleException("The Work Order must be Released before recovering its Lamination Job.");
            LaminationJob? existing = await repository.GetLinkedLaminationJobAsync(wo.Id, ct);
            if (existing is not null) { workOrder = wo; return; }
            ValidateLaminationSpecification(wo);
            int year = DateTime.UtcNow.Year;
            string number = LaminationJob.FormatNumber(year, await repository.GetMaximumLaminationJobSequenceAsync(year, ct) + 1);
            var job = new LaminationJob(number, wo.DrawingNumber, wo.CustomerName, wo.TransformerRating!,
                LaminationDesignType.Simple, StepLapOrientation.NotApplicable, 1, wo.GradeId!.Value,
                wo.Thickness, wo.Category, wo.CoreLossPerKg, wo.RequiredWeight ?? wo.PlanningRequiredQuantity,
                wo.CoreLossPerKg, wo.Id, wo.WorkOrderNumber, wo.PlannedStartDate ?? wo.WorkOrderDate,
                wo.RequiredDate, null, wo.Planner, wo.Remarks);
            job.SetCreatedAudit(Actor(), DateTimeOffset.UtcNow);
            await repository.AddLaminationJobAsync(job, ct);
            wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Lamination).LinkDocument(job.Id, job.LaminationJobNumber);
            await unitOfWork.SaveChangesAsync(ct);
            workOrder = wo;
        }, cancellationToken);
        return await MapAsync(workOrder!, cancellationToken);
    }
    public Task<WorkOrderDetailsDto> StartAsync(Guid id, CancellationToken cancellationToken = default) => Transition(id, (x, a, t) => x.Start(a, t), cancellationToken);
    public Task<WorkOrderDetailsDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default) => Transition(id, (x, a, t) => x.Complete(a, t), cancellationToken);
    public Task<WorkOrderDetailsDto> CloseAsync(Guid id, CancellationToken cancellationToken = default) => Transition(id, (x, a, t) => x.Close(a, t), cancellationToken);
    public async Task<WorkOrderDetailsDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        WorkOrder wo = await FindAsync(id, cancellationToken); string actor = Actor(); DateTimeOffset now = DateTimeOffset.UtcNow;
        try { wo.Cancel(actor, now); } catch (InvalidOperationException ex) { throw new BusinessRuleException(ex.Message); }
        foreach (var allocation in wo.Allocations.Where(x => x.IsActive)) allocation.Release(now, actor);
        await unitOfWork.SaveChangesAsync(cancellationToken); return await MapAsync(wo, cancellationToken);
    }
    public async Task<WorkOrderDetailsDto> SetSlittingRequirementAsync(Guid id, SetSlittingRequirementRequest request, CancellationToken cancellationToken = default)
    {
        WorkOrder wo = await FindAsync(id, cancellationToken);
        if (wo.ProductType != WorkOrderProductType.Lamination || request.IsRequired) throw new BusinessRuleException("MVP only allows Lamination Work Orders to mark Slitting as not required.");
        try { wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Slitting).MarkNotRequired(request.Remarks); }
        catch (InvalidOperationException ex) { throw new BusinessRuleException(ex.Message); }
        await unitOfWork.SaveChangesAsync(cancellationToken); return await MapAsync(wo, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkOrderMaterialAllocationDto>> GetAllocationsAsync(Guid id, CancellationToken cancellationToken = default)
        => (await FindAsync(id, cancellationToken)).Allocations.OrderByDescending(x => x.ReservedOn).Select(MapAllocation).ToArray();

    public async Task<WorkOrderMaterialAllocationDto> AllocateAsync(Guid id, CreateMaterialAllocationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.AllocatedWeight <= 0) throw new BusinessRuleException("Allocated Weight must be greater than zero.");
        WorkOrderMaterialAllocation? allocation = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            WorkOrder wo = await FindAsync(id, ct);
            if (wo.Status is not (WorkOrderStatus.Released or WorkOrderStatus.InFulfilment or WorkOrderStatus.PartiallyReady)) throw new BusinessRuleException("Material may only be allocated to a Released or In Fulfilment Work Order.");
            if (wo.PlannedInventoryQuantity <= 0 && wo.ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
                wo.ConfigureProductFulfilment();
            if (wo.PlannedInventoryQuantity <= 0) throw new BusinessRuleException("This Work Order has no planned inventory portion.");
            if (wo.ProductType == WorkOrderProductType.Lamination) throw new BusinessRuleException("Lamination input material is allocated within its linked Lamination Job.");
            CoilType expectedType = wo.ProductType == WorkOrderProductType.MotherCoil ? CoilType.MotherCoil : CoilType.SlitCoil;
            if (request.CoilType != expectedType) throw new BusinessRuleException($"{wo.ProductType} Work Orders require {expectedType} inventory.");
            decimal alreadyReserved = wo.Allocations.Where(x => x.IsActive).Sum(x => x.AllocatedWeight);
            decimal remainingPlan = wo.PlannedInventoryQuantity - alreadyReserved;
            if (request.AllocatedWeight > remainingPlan) throw new BusinessRuleException($"Only {remainingPlan:N3} {wo.QuantityUnit} remains in the planned inventory portion.");
            string number; decimal weight; CoilStatus status;
            if (request.CoilType == CoilType.MotherCoil)
            {
                RawCoil coil = await repository.GetMotherCoilAsync(request.CoilId, ct) ?? throw new NotFoundException("Mother Coil was not found.");
                number = coil.RawCoilNumber; weight = coil.Weight; status = coil.Status;
            }
            else if (request.CoilType == CoilType.SlitCoil)
            {
                SlitCoil coil = await repository.GetSlitCoilAsync(request.CoilId, ct) ?? throw new NotFoundException("Slit Coil was not found.");
                number = coil.CoilNumber; weight = coil.Weight; status = coil.Status;
            }
            else throw new BusinessRuleException("Only Mother Coil and Slit Coil allocations are supported.");
if (status != CoilStatus.Available) throw new BusinessRuleException("Only available coils can be allocated.");
            if (request.CoilType == CoilType.MotherCoil)
            {
                RawCoil matched = await repository.GetMotherCoilAsync(request.CoilId, ct) ?? throw new NotFoundException("Mother Coil was not found.");
                if (Math.Abs(matched.Thickness - wo.Thickness) > 0.001m || (wo.RequiredWidth.HasValue && Math.Abs(matched.Width - wo.RequiredWidth.Value) > 0.01m)) throw new BusinessRuleException("Mother Coil does not match the Work Order thickness and required width.");
                matched.SetStatus(CoilStatus.Reserved);
            }
            else
            {
                SlitCoil matched = await repository.GetSlitCoilAsync(request.CoilId, ct) ?? throw new NotFoundException("Slit Coil was not found.");
                if (matched.GradeId != wo.GradeId || matched.Thickness != wo.Thickness || (wo.RequiredWidth.HasValue && Math.Abs(matched.Width - wo.RequiredWidth.Value) > 0.01m)) throw new BusinessRuleException("Slit Coil does not match the Work Order grade, thickness, and required width.");
                matched.SetStatus(CoilStatus.Reserved);
            }
            decimal reserved = await repository.GetActiveReservedWeightAsync(request.CoilType, request.CoilId, null, ct);
            decimal remaining = MaterialAllocationCalculator.ValidateAndCalculateRemaining(weight, reserved, request.AllocatedWeight);
            allocation = new WorkOrderMaterialAllocation(wo.Id, request.CoilType, request.CoilId, number,
                request.AllocatedWeight, remaining, DateTimeOffset.UtcNow, Actor(), request.Remarks);
            wo.AddAllocation(allocation);
            await repository.AddAllocationAsync(allocation, ct);
            wo.RecalculateFulfilment(alreadyReserved + request.AllocatedWeight, wo.ProducedQuantity, true);
            await unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);
        ArgumentNullException.ThrowIfNull(allocation);
        return MapAllocation(allocation);
    }

    public async Task<WorkOrderMaterialAllocationDto> UpdateAllocationAsync(Guid id, Guid allocationId, UpdateWorkOrderInventoryAllocationRequest request, CancellationToken cancellationToken = default)
    {
        WorkOrderMaterialAllocation? updated = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            WorkOrder wo = await FindAsync(id, ct);
            if (wo.Status is not (WorkOrderStatus.Released or WorkOrderStatus.InFulfilment or WorkOrderStatus.PartiallyReady)) throw new BusinessRuleException("Allocations can be edited only while fulfilment is active.");
            if (wo.PlannedInventoryQuantity <= 0 && wo.ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
                wo.ConfigureProductFulfilment();
            var allocation = wo.Allocations.SingleOrDefault(x => x.Id == allocationId) ?? throw new NotFoundException("Allocation was not found.");
            if (request.Quantity <= 0) throw new BusinessRuleException("Allocation quantity must be greater than zero.");
            decimal otherWorkOrderReservations = wo.Allocations.Where(x => x.IsActive && x.Id != allocationId).Sum(x => x.AllocatedWeight);
            decimal remainingPlan = wo.PlannedInventoryQuantity - otherWorkOrderReservations;
            if (request.Quantity > remainingPlan) throw new BusinessRuleException($"Only {remainingPlan:N3} {wo.QuantityUnit} remains in the inventory plan.");
            Guid coilId = allocation.MotherCoilId ?? allocation.SlitCoilId!.Value;
            decimal physicalWeight = allocation.CoilType == CoilType.MotherCoil
                ? (await repository.GetMotherCoilAsync(coilId, ct) ?? throw new NotFoundException("Mother Coil was not found.")).Weight
                : (await repository.GetSlitCoilAsync(coilId, ct) ?? throw new NotFoundException("Slit Coil was not found.")).Weight;
            decimal reservedByOthers = await repository.GetActiveReservedWeightAsync(allocation.CoilType, coilId, allocationId, ct);
            decimal available = physicalWeight - reservedByOthers;
            if (request.Quantity > available) throw new BusinessRuleException($"The selected inventory has only {available:N3} {wo.QuantityUnit} available.");
            allocation.Adjust(request.Quantity, available - request.Quantity, request.Remarks);
            wo.RecalculateFulfilment(otherWorkOrderReservations + request.Quantity, wo.ProducedQuantity, true);
            await unitOfWork.SaveChangesAsync(ct); updated = allocation;
        }, cancellationToken);
        return MapAllocation(updated!);
    }
    public async Task ReleaseAllocationAsync(Guid id, Guid allocationId, ReleaseMaterialAllocationRequest? request, CancellationToken cancellationToken = default)
    {
        WorkOrder wo = await FindAsync(id, cancellationToken);
        var allocation = wo.Allocations.SingleOrDefault(x => x.Id == allocationId) ?? throw new NotFoundException("Allocation was not found.");
        try { allocation.Release(DateTimeOffset.UtcNow, Actor()); } catch (InvalidOperationException ex) { throw new BusinessRuleException(ex.Message); }
        Guid coilId = allocation.MotherCoilId ?? allocation.SlitCoilId!.Value;
        decimal otherReservations = await repository.GetActiveReservedWeightAsync(allocation.CoilType, coilId, allocation.Id, cancellationToken);
        if (otherReservations <= 0)
        {
            if (allocation.CoilType == CoilType.MotherCoil)
                (await repository.GetMotherCoilAsync(coilId, cancellationToken) ?? throw new NotFoundException("Mother Coil was not found.")).SetStatus(CoilStatus.Available);
            else
                (await repository.GetSlitCoilAsync(coilId, cancellationToken) ?? throw new NotFoundException("Slit Coil was not found.")).SetStatus(CoilStatus.Available);
        }
        decimal reserved = wo.Allocations.Where(x => x.IsActive).Sum(x => x.AllocatedWeight);
        wo.RecalculateFulfilment(reserved, wo.ProducedQuantity, reserved > 0);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableMotherCoilsAsync(Guid id, string? search, CancellationToken cancellationToken = default) { WorkOrder wo = await FindAsync(id, cancellationToken); return await repository.GetAvailableMotherCoilsAsync(wo.Thickness, wo.RequiredWidth, search, cancellationToken); }
    public async Task<IReadOnlyList<AvailableCoilDto>> GetAvailableSlitCoilsAsync(Guid id, string? search, CancellationToken cancellationToken = default)
    {
        WorkOrder wo = await FindAsync(id, cancellationToken);
        Guid[] selectedCoilIds = wo.Allocations.Where(x => x.IsActive && x.SlitCoilId.HasValue)
            .Select(x => x.SlitCoilId!.Value).ToArray();
        return await repository.GetAvailableSlitCoilsAsync(wo.Thickness, wo.RequiredWidth,
            selectedCoilIds, search, cancellationToken);
    }

    public async Task<WorkOrderMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await repository.GetForDashboardAsync(cancellationToken); DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new(rows.Count(x => x.Status == WorkOrderStatus.Draft), rows.Count(x => x.Status == WorkOrderStatus.Released),
            rows.Count(x => x.Status == WorkOrderStatus.InProduction), rows.Count(x => x.Status == WorkOrderStatus.Completed && x.CompletedOn?.UtcDateTime.Date == DateTime.UtcNow.Date),
            rows.Count(x => x.Status == WorkOrderStatus.Completed),
            rows.Count(x => x.RequiredDate < today && x.Status is not (WorkOrderStatus.Completed or WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)),
            rows.Count(x => x.WorkOrderType == WorkOrderType.CustomerOrder), rows.Count(x => x.WorkOrderType == WorkOrderType.InventoryProduction),
            rows.Where(x => x.Status is WorkOrderStatus.Released or WorkOrderStatus.InProduction).OrderBy(x => x.RequiredDate).ThenByDescending(x => x.Priority)
                .Select(x => new WorkOrderQueueItemDto(x.Id, x.WorkOrderNumber, x.ProductType, x.RequiredDate, x.Priority, x.Status,
                    x.RequiredWeight > 0 ? Math.Min(100, 100 * x.Allocations.Where(a => a.IsActive).Sum(a => a.AllocatedWeight) / x.RequiredWeight.Value) : 0,
                    x.Operations.Count(o => o.IsRequired) == 0 ? 100 : 100m * x.Operations.Count(o => o.IsRequired && o.Status == WorkOrderOperationStatus.Completed) / x.Operations.Count(o => o.IsRequired))).ToArray());
    }

    private async Task<WorkOrderDetailsDto> Transition(Guid id, Action<WorkOrder,string,DateTimeOffset> transition, CancellationToken token)
    {
        WorkOrder wo = await FindAsync(id, token); try { transition(wo, Actor(), DateTimeOffset.UtcNow); } catch (InvalidOperationException ex) { throw new BusinessRuleException(ex.Message); }
        await unitOfWork.SaveChangesAsync(token); return await MapAsync(wo, token);
    }
    private Task<WorkOrder?> FindRaw(Guid id, CancellationToken token) => repository.GetByIdAsync(id, token);
    private async Task<WorkOrder> FindAsync(Guid id, CancellationToken token) => await FindRaw(id, token) ?? throw new NotFoundException($"Work Order '{id}' was not found.");
    private async Task<WorkOrderDetailsDto> MapAsync(WorkOrder x, CancellationToken token)
    {
        var jobs = await repository.GetLinkedSlittingJobsAsync(x.Id, token);
        var laminationJob = await repository.GetLinkedLaminationJobAsync(x.Id, token);
        return new(x.Id, x.WorkOrderNumber, x.WorkOrderType, x.ProductType, x.SalesOrderId, x.SalesOrderLineId, x.CustomerName, x.SalesOrderReference, x.WorkOrderDate,
            x.RequiredDate, x.Priority, x.GradeId, x.Grade?.Code, x.Thickness, x.Category, x.CoreLossPerKg, x.DrawingNumber,
            x.RequiredWidth, x.RequiredWeight, x.RequiredQuantity, x.Status, x.Remarks, x.ReleasedBy, x.ReleasedOn, x.StartedBy,
            x.StartedOn, x.CompletedBy, x.CompletedOn, x.ClosedBy, x.ClosedOn, x.CancelledBy, x.CancelledOn, x.CreatedBy, x.CreatedOn,
            x.ModifiedBy, x.ModifiedOn, Convert.ToBase64String(x.RowVersion), x.Allocations.Where(a => a.IsActive).Sum(a => a.AllocatedWeight),
            x.Allocations.Sum(a => a.IssuedWeight ?? 0), x.Allocations.Sum(a => a.ConsumedWeight ?? 0),
            x.Operations.OrderBy(o => o.Sequence).Select(o => new WorkOrderOperationDto(o.Id, o.OperationType, o.Sequence, o.IsRequired, o.Status, o.RelatedDocumentId, o.RelatedDocumentNumber, o.StartedOn, o.CompletedOn, o.Remarks)).ToArray(),
            x.Allocations.OrderByDescending(a => a.ReservedOn).Select(MapAllocation).ToArray(), jobs.Select(j => new WorkOrderSlittingJobDto(j.Id, j.SlittingJobNo, j.Status)).ToArray(),
            x.FulfilmentStrategy, x.PlanningRequiredQuantity, x.PlannedInventoryQuantity, x.PlannedProductionQuantity,
            x.ReservedInventoryQuantity, x.ProducedQuantity, x.ReadyQuantity, x.DispatchedQuantity, x.UnplannedQuantity, x.CoveragePercentage, x.ProductionRoute,
            laminationJob is null ? null : new WorkOrderLaminationJobDto(laminationJob.Id, laminationJob.LaminationJobNumber,
                laminationJob.JobOrDrawingNumber, x.PlanningRequiredQuantity, laminationJob.Status,
                laminationJob.TotalWeight <= 0 ? 0 : Math.Min(100, 100 * laminationJob.TotalAllocatedWeight / laminationJob.TotalWeight),
                laminationJob.CreatedAtUtc, laminationJob.ReleasedOn, laminationJob.CompletedOn));
    }
    private static WorkOrderMaterialAllocationDto MapAllocation(WorkOrderMaterialAllocation a) => new(a.Id, a.CoilType,
        a.MotherCoilId ?? a.SlitCoilId!.Value, a.CoilNumber, a.AllocatedWeight, a.IssuedWeight, a.ConsumedWeight,
        a.RemainingWeightAfterAllocation, a.Status, a.ReservedOn, a.ReservedBy, a.ReleasedOn, a.ReleasedBy, a.Remarks);
    private static WorkOrder Build(string number, CreateWorkOrderRequest r)
    {
        var workOrder = new WorkOrder(number, r.WorkOrderType, r.ProductType, r.CustomerName, r.SalesOrderReference,
            r.WorkOrderDate, r.RequiredDate, r.Priority, r.GradeId, r.Thickness, r.Category, r.CoreLossPerKg,
            r.DrawingNumber, r.RequiredWidth, r.RequiredWeight, r.RequiredQuantity, r.Remarks);
        ApplyFulfilmentPlan(workOrder, r);
        return workOrder;
    }
    private static void ApplyFulfilmentPlan(WorkOrder workOrder, CreateWorkOrderRequest request)
    {
        if (!request.FulfilmentStrategy.HasValue) return;
        workOrder.ConfigureFulfilment(request.FulfilmentStrategy.Value, request.PlannedInventoryQuantity ?? 0,
            request.PlannedProductionQuantity ?? 0, request.ProductionRoute ?? ProductionRoute.None);
    }
    private static void ValidateLaminationSpecification(WorkOrder wo)
    {
        List<string> missing = [];
        if (!wo.GradeId.HasValue) missing.Add("Grade");
        if (wo.Thickness <= 0) missing.Add("Thickness");
        if (wo.CoreLossPerKg <= 0) missing.Add("Core Loss");
        if (string.IsNullOrWhiteSpace(wo.TransformerRating)) missing.Add("Transformer Rating");
        if (wo.PlanningRequiredQuantity <= 0) missing.Add("Required Quantity");
        if (missing.Count > 0) throw new BusinessRuleException($"Lamination specification is incomplete: {string.Join(", ", missing)}.");
    }

    private static void Validate(CreateWorkOrderRequest r)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(r.Category)) errors.Add("Category is required.");
        if (r.Thickness <= 0) errors.Add("Thickness must be greater than zero.");
        if (r.Priority is < 1 or > 5) errors.Add("Priority must be between 1 and 5.");
        if (r.RequiredDate < r.WorkOrderDate) errors.Add("Required Date cannot be before Work Order Date.");
        if (r.RequiredWeight <= 0 && r.RequiredQuantity <= 0) errors.Add("Required Weight or Required Quantity must be greater than zero.");
        if (errors.Count > 0) throw new ValidationException(errors);
    }
    private async Task ApplySalesOrderLineAsync(WorkOrder workOrder, CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductType is not (WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil or WorkOrderProductType.Lamination)) return;
        if (!request.SalesOrderId.HasValue || !request.SalesOrderLineId.HasValue) throw new BusinessRuleException("A confirmed Sales Order line is required.");
        SalesOrder order = await repository.GetSalesOrderAsync(request.SalesOrderId.Value, cancellationToken) ?? throw new NotFoundException("Sales Order was not found.");
        SalesOrderLine line = order.Lines.SingleOrDefault(x => x.Id == request.SalesOrderLineId.Value) ?? throw new NotFoundException("Sales Order line was not found.");
        if ((WorkOrderProductType)line.ProductType != request.ProductType) throw new BusinessRuleException("The selected Sales Order line does not match the Work Order product type.");
        workOrder.InitializeSalesOrderPlan(order, line, line.OrderedQuantity, request.WorkOrderDate,
            line.RequiredDeliveryDate ?? order.RequiredDeliveryDate, request.Priority, Actor(),
            request.ProductType == WorkOrderProductType.Lamination ? FulfilmentStrategy.ProductionOnly : FulfilmentStrategy.ExistingInventoryOnly,
            request.ProductType == WorkOrderProductType.Lamination ? 0 : line.OrderedQuantity,
            request.ProductType == WorkOrderProductType.Lamination ? line.OrderedQuantity : 0,
            request.ProductType == WorkOrderProductType.Lamination ? ProductionRoute.LaminationOnly : ProductionRoute.None,
            request.Remarks ?? line.Remarks);
    }
    private static void ValidateWorkOrder(WorkOrder workOrder)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(workOrder.Category)) errors.Add("Category is required.");
        if (workOrder.Thickness <= 0) errors.Add("Thickness must be greater than zero.");
        if (workOrder.Priority is < 1 or > 5) errors.Add("Priority must be between 1 and 5.");
        if (workOrder.RequiredDate < workOrder.WorkOrderDate) errors.Add("Required Date cannot be before Work Order Date.");
        if (workOrder.PlanningRequiredQuantity <= 0) errors.Add("Required Weight or Required Quantity must be greater than zero.");
        if (errors.Count > 0) throw new ValidationException(errors);
    }
    private static void EnsureRowVersion(WorkOrder x, string version)
    {
        try { if (!x.RowVersion.SequenceEqual(Convert.FromBase64String(version))) throw new ConflictException("The Work Order was modified by another process."); }
        catch (FormatException) { throw new ConflictException("The Work Order row version is invalid."); }
    }
    private string Actor() => string.IsNullOrWhiteSpace(currentUser.UserName) ? currentUser.UserId ?? "System" : currentUser.UserName;
}
