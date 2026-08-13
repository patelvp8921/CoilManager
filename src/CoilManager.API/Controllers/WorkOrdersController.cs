using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Application.DTOs.Dispatches;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CoilManager.Persistence;

namespace CoilManager.API.Controllers;

[Route("api/work-orders")]
public sealed class WorkOrdersController(IWorkOrderService service, ApplicationDbContext db) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiPagedResponse<WorkOrderListItemDto>>> Get([FromQuery] WorkOrderQueryRequest request, CancellationToken token)
    {
        var result = await service.GetAsync(request, token);
        return Paged(result.Items, new PaginationResult(result.PageNumber, result.PageSize, result.TotalCount));
    }
    [HttpGet("next-number")] public async Task<ActionResult<ApiResponse<string>>> NextNumber(CancellationToken token) => Success(await service.GetNextNumberAsync(token));
    [HttpGet("by-number/{workOrderNumber}")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> ByNumber(string workOrderNumber, CancellationToken token) => Success(await service.GetByNumberAsync(workOrderNumber, token));
    [HttpGet("{id:guid}")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> ById(Guid id, CancellationToken token) => Success(await service.GetByIdAsync(id, token));
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Create(CreateWorkOrderRequest request, CancellationToken token)
    {
        var result = await service.CreateAsync(request, token);
        return CreatedAtAction(nameof(ById), new { id = result.Id }, ApiResponse<WorkOrderDetailsDto>.Ok(result, "Work Order created successfully."));
    }
    [HttpPut("{id:guid}")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Update(Guid id, UpdateWorkOrderRequest request, CancellationToken token) => Success(await service.UpdateAsync(id, request, token), "Work Order updated successfully.");
    [HttpPost("{id:guid}/release"), Authorize(Policy="Permission:WorkOrders.Release")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Release(Guid id, CancellationToken token) => Action(id, service.ReleaseAsync, "Work Order released.", token);
    [HttpPost("{id:guid}/recover-lamination-job"), Authorize(Policy="Permission:WorkOrders.CreateProductionJob")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> RecoverLaminationJob(Guid id, CancellationToken token) => Action(id, service.RecoverLaminationJobAsync, "Draft Lamination Job created and linked.", token);
    [HttpPost("{id:guid}/start")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Start(Guid id, CancellationToken token) => Action(id, service.StartAsync, "Work Order started.", token);
    [HttpPost("{id:guid}/complete")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Complete(Guid id, CancellationToken token) => Action(id, service.CompleteAsync, "Work Order completed.", token);
    [HttpPost("{id:guid}/close")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Close(Guid id, CancellationToken token) => Action(id, service.CloseAsync, "Work Order closed.", token);
    [HttpPost("{id:guid}/cancel")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Cancel(Guid id, CancellationToken token) => Action(id, service.CancelAsync, "Work Order cancelled.", token);
    [HttpPut("{id:guid}/operations/slitting-requirement")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> SetSlitting(Guid id, SetSlittingRequirementRequest request, CancellationToken token) => Success(await service.SetSlittingRequirementAsync(id, request, token));
    [HttpGet("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderMaterialAllocationDto>>>> Allocations(Guid id, CancellationToken token) => Success(await service.GetAllocationsAsync(id, token));
    [HttpGet("{id:guid}/inventory-allocations")] public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderMaterialAllocationDto>>>> InventoryAllocations(Guid id, CancellationToken token) => Success(await service.GetAllocationsAsync(id, token));
    [HttpGet("{id:guid}/material-allocation")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> MaterialAllocation(Guid id, CancellationToken token) => Success(await service.GetByIdAsync(id, token));
    [HttpGet("{id:guid}/available-inventory")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableCoilDto>>>> AvailableInventory(Guid id, [FromQuery] string? search, CancellationToken token)
    {
        WorkOrderDetailsDto workOrder = await service.GetByIdAsync(id, token);
        return Success(workOrder.ProductType == WorkOrderProductType.MotherCoil
            ? await service.GetAvailableMotherCoilsAsync(id, search, token)
            : await service.GetAvailableSlitCoilsAsync(id, search, token));
    }
    [HttpGet("{id:guid}/next-actions"), Authorize(Policy="Permission:WorkOrders.View")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderNextActionDto>>>> NextActions(Guid id, CancellationToken token)
        => Success(await BuildNextActions(id, token));

    [HttpGet("{id:guid}/next-action"), Authorize(Policy="Permission:WorkOrders.View")]
    public async Task<ActionResult<ApiResponse<WorkOrderNextActionDto>>> NextAction(Guid id, CancellationToken token)
        => Success((await BuildNextActions(id, token)).First());

    private async Task<IReadOnlyList<WorkOrderNextActionDto>> BuildNextActions(Guid id, CancellationToken token)
    {       WorkOrderDetailsDto x = await service.GetByIdAsync(id, token);
        var actions = new List<WorkOrderNextActionDto>();
        QuantityUnit unit = x.RequiredWeight.HasValue ? QuantityUnit.Kg : QuantityUnit.Pieces;
        if (x.Status == WorkOrderStatus.Draft)
        {
            actions.Add(new("release", "ReleaseWorkOrder", "Release Work Order",
                "Release this Work Order to begin its product-specific fulfilment workflow.", x.PlanningRequiredQuantity,
                0, x.PlanningRequiredQuantity, unit, "Draft", "Info", true, null, "Release Work Order", null, "RELEASE_WORK_ORDER", 0));
            return actions;
        }

        if (x.ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
        {
            decimal required = x.PlanningRequiredQuantity > 0 ? x.PlanningRequiredQuantity : x.RequiredWeight ?? x.RequiredQuantity ?? 0;
            decimal reserved = x.ReservedInventoryQuantity;
            decimal remaining = Math.Max(0, required - reserved);
            IReadOnlyList<AvailableCoilDto> inventory = x.ProductType == WorkOrderProductType.MotherCoil
                ? await service.GetAvailableMotherCoilsAsync(id, null, token)
                : await service.GetAvailableSlitCoilsAsync(id, null, token);
            decimal matchingAvailable = inventory.Where(c =>
                (x.Grade is null || c.Grade == x.Grade) && c.Thickness == x.Thickness &&
                (!x.RequiredWidth.HasValue || Math.Abs(c.Width - x.RequiredWidth.Value) <= 0.01m)).Sum(c => c.AvailableWeight);
            decimal shortage = Math.Max(0, remaining - matchingAvailable);
            string product = x.ProductType == WorkOrderProductType.MotherCoil ? "Mother Coil" : "Slit Coil";
            string description = shortage > 0
                ? $"Insufficient {product} Inventory — {shortage:N3} {unit} shortage. Stock production must be planned independently."
                : remaining > 0 ? $"Select and reserve existing {product} inventory." : "The required inventory is fully reserved.";
            actions.Add(new("inventory", remaining > 0 ? "AllocateInventory" : "ViewInventoryAllocation", "Material Allocation",
                description, required, reserved, remaining, unit, remaining == 0 ? "Complete" : shortage > 0 ? "Shortage" : "Pending",
                remaining == 0 ? "Success" : shortage > 0 ? "Error" : "Warning",
                remaining == 0 || x.Status is WorkOrderStatus.Released or WorkOrderStatus.InFulfilment or WorkOrderStatus.PartiallyReady,
                x.Status == WorkOrderStatus.Draft ? "Release the Work Order before allocating inventory." : null,
                remaining > 0 ? $"Allocate {product}s" : "View Allocation", $"/work-orders/{id}/material-allocation",
                remaining > 0 ? "ALLOCATE_INVENTORY" : "VIEW_INVENTORY_ALLOCATION", 1));
        }
        else if (x.ProductType == WorkOrderProductType.Lamination)
        {
            var job = x.LinkedLaminationJob;
            if (job is null)
            {
                actions.Add(new("production", "RecoverMissingLaminationJob", "Lamination Job Missing",
                    "This Released Lamination Work Order has no linked Lamination Job. Use the recovery action to create one safely.",
                    x.PlanningRequiredQuantity, 0, x.PlanningRequiredQuantity, unit, "Missing", "Error", true, null,
                    "Create Lamination Job", null, "RECOVER_LAMINATION_JOB", 1));
            }
            else if (job.Status == LaminationJobStatus.Draft)
            {
                actions.Add(new("production", "CompleteLaminationJobSetup", "Complete Lamination Job Setup",
                    $"Linked Job: {job.LaminationJobNumber}. Complete the Step Schedule and review production details before releasing the job.",
                    x.PlanningRequiredQuantity, 0, x.PlanningRequiredQuantity, unit, "Draft", "Info", true, null,
                    "Open Lamination Job", $"/lamination-jobs/{job.Id}/edit", "OPEN_LAMINATION_JOB", 1));
            }
            else if (job.Status == LaminationJobStatus.Released && job.MaterialAllocationPercentage < 100)
            {
                actions.Add(new("production", "AllocateLaminationMaterial", "Lamination Material Allocation",
                    $"Material allocation for {job.LaminationJobNumber} is {job.MaterialAllocationPercentage:N1}% complete.",
                    100, job.MaterialAllocationPercentage, Math.Max(0, 100 - job.MaterialAllocationPercentage), null,
                    "Released", "Warning", true, null, "Allocate Lamination Material",
                    $"/lamination-jobs/{job.Id}/material-allocation", "ALLOCATE_LAMINATION_MATERIAL", 1));
            }
            else
            {
                actions.Add(new("production", "ViewLaminationJob", "Lamination Production",
                    $"Linked Job: {job.LaminationJobNumber}. Current status: {job.Status}.", x.PlanningRequiredQuantity,
                    x.ProducedQuantity, Math.Max(0, x.PlanningRequiredQuantity - x.ProducedQuantity), unit,
                    job.Status.ToString(), "Info", true, null, "View Lamination Job",
                    $"/lamination-jobs/{job.Id}", "VIEW_LAMINATION_JOB", 1));
            }
        }

        if (x.Status is WorkOrderStatus.Ready or WorkOrderStatus.PartiallyDispatched)
        {
            decimal remaining = Math.Max(0, x.PlanningRequiredQuantity - x.DispatchedQuantity);
            actions.Add(new("dispatch", "CreateDispatch", x.Status == WorkOrderStatus.Ready ? "Ready for Dispatch" : "Remaining Dispatch",
                x.Status == WorkOrderStatus.Ready ? "The required physical quantity is ready to ship." : $"{remaining:N3} {unit} remains to dispatch.",
                x.PlanningRequiredQuantity, x.DispatchedQuantity, remaining, unit, x.Status.ToString(), "Success", true, null,
                x.Status == WorkOrderStatus.Ready ? "Create Dispatch" : "Create Another Dispatch", $"/dispatch-create?workOrderId={id}", "CREATE_DISPATCH", 3));
        }
        if (x.Status == WorkOrderStatus.Completed) actions.Add(new("dispatches", "ViewDispatches", "Fulfilment Complete",
            "The Work Order requirement has been fully dispatched.", x.PlanningRequiredQuantity, x.DispatchedQuantity, 0, unit,
            "Completed", "Success", true, null, "View Dispatches", $"/dispatch?workOrderId={id}", "VIEW_DISPATCHES", 3));
        return actions.OrderBy(a => a.Order).ToArray();
    }
    [HttpGet("{id:guid}/dispatch-summary")]
    public async Task<ActionResult<ApiResponse<WorkOrderDispatchSummaryDto>>> DispatchSummary(Guid id,CancellationToken token)
    {var x=await service.GetByIdAsync(id,token);var rows=await db.Dispatches.AsNoTracking().Where(d=>d.WorkOrderId==id).ToListAsync(token);decimal required=x.PlanningRequiredQuantity,dispatched=rows.Where(d=>d.Status==DispatchStatus.Dispatched).Sum(d=>d.DispatchQuantity),available=Math.Max(0,Math.Min(x.ReadyQuantity-dispatched,required-dispatched));bool can=(x.Status is WorkOrderStatus.Ready or WorkOrderStatus.PartiallyDispatched)&&available>0;var result=new WorkOrderDispatchSummaryDto(id,x.WorkOrderNumber,required,x.RequiredWeight.HasValue?QuantityUnit.Kg:QuantityUnit.Pieces,x.ReadyQuantity,dispatched,available,Math.Max(0,required-dispatched),required<=0?0:100*dispatched/required,rows.Count,rows.Count(d=>d.Status==DispatchStatus.Draft),rows.Count(d=>d.Status==DispatchStatus.Dispatched),can,can?null:"No quantity is currently available for dispatch.");return Success(result);}
    [HttpGet("{id:guid}/dispatches")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DispatchListItemDto>>>> Dispatches(Guid id,CancellationToken token)
    { _=await service.GetByIdAsync(id,token);var rows=await db.Dispatches.AsNoTracking().Include(x=>x.Packages).Where(x=>x.WorkOrderId==id).OrderByDescending(x=>x.CreatedAtUtc).Select(x=>new DispatchListItemDto(x.Id,x.DispatchNumber,x.PackingSlipNumber,x.CustomerName,x.WorkOrderNumber,x.SalesOrderNumber,x.ProductType,x.DispatchQuantity,x.QuantityUnit,x.Packages.Count,x.DispatchDate,x.VehicleNumber,x.Status)).ToListAsync(token);return Success<IReadOnlyList<DispatchListItemDto>>(rows);}
    [HttpGet("{id:guid}/fulfilment")]
    public async Task<ActionResult<ApiResponse<WorkOrderFulfilmentDto>>> Fulfilment(Guid id, CancellationToken token)
    {
        WorkOrderDetailsDto x = await service.GetByIdAsync(id, token);
        decimal required = x.RequiredWeight ?? x.RequiredQuantity ?? 0;
        decimal reserved = x.Allocations.Where(a => a.Status is AllocationStatus.Reserved or AllocationStatus.Issued or AllocationStatus.PartiallyConsumed).Sum(a => a.AllocatedWeight);
        decimal ready = Math.Min(required, reserved);
        return Success(new WorkOrderFulfilmentDto(required, x.RequiredWeight.HasValue ? QuantityUnit.Kg : QuantityUnit.Pieces,
            reserved, reserved, Math.Max(0, required - reserved), 0, ready, 0, Math.Max(0, required - ready),
            required <= 0 ? 0 : 100, required <= 0 ? 0 : 100 * ready / required, x.Status));
    }
    [HttpPost("{id:guid}/inventory-allocations")]
    public async Task<ActionResult<ApiResponse<WorkOrderMaterialAllocationDto>>> CreateInventoryAllocation(Guid id, CreateWorkOrderInventoryAllocationRequest request, CancellationToken token)
        => Success(await service.AllocateAsync(id, new CreateMaterialAllocationRequest(request.InventoryType, request.InventoryId, request.Quantity, request.Remarks), token), "Inventory reserved.");
    [HttpPut("{id:guid}/inventory-allocations/{allocationId:guid}")]
    public async Task<ActionResult<ApiResponse<WorkOrderMaterialAllocationDto>>> UpdateInventoryAllocation(Guid id, Guid allocationId, UpdateWorkOrderInventoryAllocationRequest request, CancellationToken token)
        => Success(await service.UpdateAllocationAsync(id, allocationId, request, token), "Inventory reservation updated.");
    [HttpPost("{id:guid}/recalculate-fulfilment")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Recalculate(Guid id, CancellationToken token) => Success(await service.GetByIdAsync(id, token));
    [HttpDelete("{id:guid}/inventory-allocations/{allocationId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveInventoryAllocation(Guid id, Guid allocationId, CancellationToken token)
    { await service.ReleaseAllocationAsync(id, allocationId, null, token); return Success<object>(null, "Inventory reservation released."); }
    [HttpPost("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<WorkOrderMaterialAllocationDto>>> Allocate(Guid id, CreateMaterialAllocationRequest request, CancellationToken token) => Success(await service.AllocateAsync(id, request, token), "Material allocated successfully.");
    [HttpDelete("{id:guid}/allocations/{allocationId:guid}")] public async Task<ActionResult<ApiResponse<object>>> DeleteAllocation(Guid id, Guid allocationId, CancellationToken token) { await service.ReleaseAllocationAsync(id, allocationId, null, token); return Success<object>(null, "Allocation released."); }
    [HttpPost("{id:guid}/allocations/{allocationId:guid}/release")] public async Task<ActionResult<ApiResponse<object>>> ReleaseAllocation(Guid id, Guid allocationId, ReleaseMaterialAllocationRequest? request, CancellationToken token) { await service.ReleaseAllocationAsync(id, allocationId, request, token); return Success<object>(null, "Allocation released."); }
    [HttpGet("{id:guid}/available-mother-coils")] public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableCoilDto>>>> MotherCoils(Guid id, [FromQuery] string? search, CancellationToken token) => Success(await service.GetAvailableMotherCoilsAsync(id, search, token));
    [HttpGet("{id:guid}/available-slit-coils")] public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableCoilDto>>>> SlitCoils(Guid id, [FromQuery] string? search, CancellationToken token) => Success(await service.GetAvailableSlitCoilsAsync(id, search, token));
    [HttpGet("metrics")] public async Task<ActionResult<ApiResponse<WorkOrderMetricsDto>>> Metrics(CancellationToken token) => Success(await service.GetMetricsAsync(token));
    private async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Action(Guid id, Func<Guid,CancellationToken,Task<WorkOrderDetailsDto>> fn, string message, CancellationToken token) => Success(await fn(id, token), message);
}
