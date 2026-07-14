using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;

[Route("api/work-orders")]
public sealed class WorkOrdersController(IWorkOrderService service) : BaseApiController
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
    [HttpPost("{id:guid}/release")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Release(Guid id, CancellationToken token) => Action(id, service.ReleaseAsync, "Work Order released.", token);
    [HttpPost("{id:guid}/start")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Start(Guid id, CancellationToken token) => Action(id, service.StartAsync, "Work Order started.", token);
    [HttpPost("{id:guid}/complete")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Complete(Guid id, CancellationToken token) => Action(id, service.CompleteAsync, "Work Order completed.", token);
    [HttpPost("{id:guid}/close")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Close(Guid id, CancellationToken token) => Action(id, service.CloseAsync, "Work Order closed.", token);
    [HttpPost("{id:guid}/cancel")] public Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Cancel(Guid id, CancellationToken token) => Action(id, service.CancelAsync, "Work Order cancelled.", token);
    [HttpPut("{id:guid}/operations/slitting-requirement")] public async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> SetSlitting(Guid id, SetSlittingRequirementRequest request, CancellationToken token) => Success(await service.SetSlittingRequirementAsync(id, request, token));
    [HttpGet("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderMaterialAllocationDto>>>> Allocations(Guid id, CancellationToken token) => Success(await service.GetAllocationsAsync(id, token));
    [HttpPost("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<WorkOrderMaterialAllocationDto>>> Allocate(Guid id, CreateMaterialAllocationRequest request, CancellationToken token) => Success(await service.AllocateAsync(id, request, token), "Material allocated successfully.");
    [HttpDelete("{id:guid}/allocations/{allocationId:guid}")] public async Task<ActionResult<ApiResponse<object>>> DeleteAllocation(Guid id, Guid allocationId, CancellationToken token) { await service.ReleaseAllocationAsync(id, allocationId, null, token); return Success<object>(null, "Allocation released."); }
    [HttpPost("{id:guid}/allocations/{allocationId:guid}/release")] public async Task<ActionResult<ApiResponse<object>>> ReleaseAllocation(Guid id, Guid allocationId, ReleaseMaterialAllocationRequest? request, CancellationToken token) { await service.ReleaseAllocationAsync(id, allocationId, request, token); return Success<object>(null, "Allocation released."); }
    [HttpGet("{id:guid}/available-mother-coils")] public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableCoilDto>>>> MotherCoils(Guid id, [FromQuery] string? search, CancellationToken token) => Success(await service.GetAvailableMotherCoilsAsync(id, search, token));
    [HttpGet("{id:guid}/available-slit-coils")] public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableCoilDto>>>> SlitCoils(Guid id, [FromQuery] string? search, CancellationToken token) => Success(await service.GetAvailableSlitCoilsAsync(id, search, token));
    [HttpGet("metrics")] public async Task<ActionResult<ApiResponse<WorkOrderMetricsDto>>> Metrics(CancellationToken token) => Success(await service.GetMetricsAsync(token));
    private async Task<ActionResult<ApiResponse<WorkOrderDetailsDto>>> Action(Guid id, Func<Guid,CancellationToken,Task<WorkOrderDetailsDto>> fn, string message, CancellationToken token) => Success(await fn(id, token), message);
}
