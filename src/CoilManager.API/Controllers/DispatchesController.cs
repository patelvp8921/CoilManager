using System.Text;
using CoilManager.Application.DTOs.Dispatches;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Persistence;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers;

[Route("api/dispatches"), Authorize(Policy = "Permission:Dispatch.View")]
public sealed class DispatchesController(ApplicationDbContext db, ICurrentUserService current) : BaseApiController
{
    private string Actor => current.UserName ?? current.UserId ?? "System";

    [HttpGet]
    public async Task<ActionResult<ApiPagedResponse<DispatchListItemDto>>> List(string? search, DispatchStatus? status, int page = 1, int pageSize = 100, CancellationToken ct = default)
    {
        IQueryable<Dispatch> query = db.Dispatches.AsNoTracking().Include(x => x.Packages);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.DispatchNumber.Contains(search) || x.PackingSlipNumber.Contains(search) || x.WorkOrderNumber.Contains(search) || x.CustomerName.Contains(search));
        if (status.HasValue) query = query.Where(x => x.Status == status);
        int total = await query.CountAsync(ct); page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 200);
        var rows = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new DispatchListItemDto(x.Id, x.DispatchNumber, x.PackingSlipNumber, x.CustomerName, x.WorkOrderNumber, x.SalesOrderNumber, x.ProductType, x.DispatchQuantity, x.QuantityUnit, x.Packages.Count, x.DispatchDate, x.VehicleNumber, x.Status)).ToListAsync(ct);
        return Paged(rows, new PaginationResult(page, pageSize, total));
    }

    [HttpGet("next-number")] public async Task<ActionResult<ApiResponse<string>>> Next(CancellationToken ct) => Success(await NextNumber("DSP", ct));
    [HttpGet("next-packing-slip-number")] public async Task<ActionResult<ApiResponse<string>>> NextPacking(CancellationToken ct) => Success(await NextNumber("PS", ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<ApiResponse<DispatchDetailsDto>>> Get(Guid id, CancellationToken ct) => Success(Map(await Find(id, ct)));

    [HttpPost("/api/work-orders/{workOrderId:guid}/dispatches"), Authorize(Policy = "Permission:Dispatch.Create")]
    public async Task<ActionResult<ApiResponse<DispatchDetailsDto>>> Create(Guid workOrderId, SaveDispatchRequest request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        WorkOrder wo = await FindWorkOrder(workOrderId, ct); ValidateQuantity(wo, request.DispatchQuantity);
        Guid? customerId = request.CustomerId ?? wo.CustomerId;
        Customer? customer = customerId.HasValue ? await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customerId, ct) : null;
        if (request.CustomerId.HasValue && customer is null) throw new KeyNotFoundException("Customer was not found.");
        string address = string.IsNullOrWhiteSpace(request.ShippingAddress) ? customer?.ShippingAddress ?? customer?.BillingAddress ?? "Not specified" : request.ShippingAddress;
        var dispatch = new Dispatch(await NextNumber("DSP", ct), await NextNumber("PS", ct), wo, request.DispatchQuantity, request.DispatchDate, address, request.ContactPerson ?? customer?.ContactPerson, request.ContactPhone ?? customer?.Phone, request.DispatchRemarks);
        if (customer is not null) dispatch.SelectCustomer(customer);
        dispatch.SetCreatedAudit(Actor, DateTimeOffset.UtcNow); Apply(dispatch, request, address); db.Dispatches.Add(dispatch);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return Success(Map(dispatch), "Draft Dispatch created.");
    }

    [HttpPut("{id:guid}"), Authorize(Policy = "Permission:Dispatch.Edit")]
    public async Task<ActionResult<ApiResponse<DispatchDetailsDto>>> Update(Guid id, SaveDispatchRequest request, CancellationToken ct)
    {
        Dispatch dispatch = await Find(id, ct); SetVersion(dispatch, request.RowVersion); WorkOrder wo = await FindWorkOrder(dispatch.WorkOrderId, ct);
        ValidateQuantity(wo, request.DispatchQuantity, dispatch.Id);
        if (request.CustomerId.HasValue)
        {
            Customer customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.CustomerId, ct) ?? throw new KeyNotFoundException("Customer was not found.");
            dispatch.SelectCustomer(customer);
        }
        Apply(dispatch, request); await db.SaveChangesAsync(ct);
        return Success(Map(dispatch), "Dispatch updated.");
    }

    [HttpPost("{id:guid}/confirm"), Authorize(Policy = "Permission:Dispatch.Confirm")]
    public async Task<ActionResult<ApiResponse<DispatchDetailsDto>>> Confirm(Guid id, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        Dispatch dispatch = await Find(id, ct); WorkOrder wo = await FindWorkOrder(dispatch.WorkOrderId, ct);
        ValidateQuantity(wo, dispatch.DispatchQuantity, dispatch.Id); decimal remaining = dispatch.DispatchQuantity;
        var sources = new List<DispatchInventorySource>(); DateTimeOffset now = DateTimeOffset.UtcNow;
        if (wo.ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
        {
            foreach (WorkOrderMaterialAllocation allocation in wo.Allocations.Where(x => x.IsActive).OrderBy(x => x.ReservedOn))
            {
                decimal take = Math.Min(remaining, allocation.AllocatedWeight - (allocation.ConsumedWeight ?? 0));
                if (take <= 0) continue; Guid coilId = allocation.MotherCoilId ?? allocation.SlitCoilId!.Value;
                allocation.RecordDispatch(take); bool keepReserved = allocation.IsActive;
                if (allocation.CoilType == CoilType.MotherCoil)
                {
                    RawCoil coil = await db.RawCoils.SingleAsync(x => x.Id == coilId, ct); CoilStatus before = coil.Status; coil.Dispatch(take, keepReserved);
                    AddInventoryTransaction(dispatch, wo, allocation.CoilType, coil.Id, coil.RawCoilNumber, before, coil.Status, take, now);
                    sources.Add(new(dispatch.Id, allocation.CoilType, coil.Id, coil.RawCoilNumber, coil.Width, take));
                }
                else
                {
                    SlitCoil coil = await db.SlitCoils.SingleAsync(x => x.Id == coilId, ct); CoilStatus before = coil.Status; coil.Dispatch(take, keepReserved);
                    AddInventoryTransaction(dispatch, wo, allocation.CoilType, coil.Id, coil.CoilNumber, before, coil.Status, take, now);
                    sources.Add(new(dispatch.Id, allocation.CoilType, coil.Id, coil.CoilNumber, coil.Width, take));
                }
                remaining -= take; if (remaining <= 0) break;
            }
            if (remaining > 0) throw new InvalidOperationException($"Only {dispatch.DispatchQuantity - remaining:N3} {wo.QuantityUnit} remains valid in inventory reservations.");
        }
        dispatch.Confirm(Actor, now, sources); foreach (var source in sources) db.Entry(source).State = EntityState.Added;
        decimal prior = await db.Dispatches.Where(x => x.WorkOrderId == wo.Id && x.Status == DispatchStatus.Dispatched && x.Id != dispatch.Id).SumAsync(x => (decimal?)x.DispatchQuantity, ct) ?? 0;
        wo.RecordDispatch(prior + dispatch.DispatchQuantity, Actor, now); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return Success(Map(dispatch), "Dispatch confirmed and inventory posted.");
    }

    [HttpPost("{id:guid}/cancel"), Authorize(Policy = "Permission:Dispatch.Cancel")]
    public async Task<ActionResult<ApiResponse<DispatchDetailsDto>>> Cancel(Guid id, CancelDispatchRequest request, CancellationToken ct)
    { Dispatch dispatch = await Find(id, ct); SetVersion(dispatch, request.RowVersion); dispatch.Cancel(request.Reason, Actor, DateTimeOffset.UtcNow); await db.SaveChangesAsync(ct); return Success(Map(dispatch), "Dispatch cancelled."); }

    [HttpGet("{id:guid}/packing-slip")] public async Task<IActionResult> PackingSlip(Guid id, CancellationToken ct) => Content(PackingHtml(await Find(id, ct)), "text/html", Encoding.UTF8);
    [HttpGet("{id:guid}/packing-slip/pdf")] public async Task<IActionResult> PackingSlipPdf(Guid id, CancellationToken ct)
    { Dispatch dispatch = await Find(id, ct); return File(PackingSlipPdfBuilder.Create(dispatch), "application/pdf", $"{dispatch.PackingSlipNumber.Replace('/', '-')}.pdf"); }

    private async Task<Dispatch> Find(Guid id, CancellationToken ct) => await db.Dispatches.Include(x => x.Packages).Include(x => x.InventorySources).FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Dispatch was not found.");
    private async Task<WorkOrder> FindWorkOrder(Guid id, CancellationToken ct) => await db.WorkOrders.Include(x => x.Customer).Include(x => x.Grade).Include(x => x.Allocations).FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Work Order was not found.");

    private void ValidateQuantity(WorkOrder wo, decimal quantity, Guid? excluding = null)
    {
        if (wo.Status is not (WorkOrderStatus.Ready or WorkOrderStatus.PartiallyDispatched)) throw new InvalidOperationException("Work Order is not ready for dispatch.");
        decimal confirmed = db.Dispatches.Where(x => x.WorkOrderId == wo.Id && x.Status == DispatchStatus.Dispatched && x.Id != excluding).Sum(x => (decimal?)x.DispatchQuantity) ?? 0;
        decimal available = Math.Min(Math.Max(0, wo.PlanningRequiredQuantity - confirmed), Math.Max(0, wo.ReadyQuantity - confirmed));
        if (quantity <= 0 || quantity > available) throw new InvalidOperationException($"Only {available:N3} {wo.QuantityUnit} is currently available for dispatch.");
    }
    private void Apply(Dispatch dispatch, SaveDispatchRequest request, string? shippingAddress = null)
    {
        var packages = request.Packages.Select((x, i) => new DispatchPackage(string.IsNullOrWhiteSpace(x.PackageNumber) ? $"PKG-{i + 1:00}" : x.PackageNumber, x.Description, x.Quantity, x.QuantityUnit, x.NetWeight, x.GrossWeight, x.Remarks, x.Sequence)).ToArray();
        if (packages.GroupBy(x => x.PackageNumber, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1)) throw new InvalidOperationException("Package Number must be unique within the Dispatch.");
        dispatch.Update(request.DispatchQuantity, request.DispatchDate, string.IsNullOrWhiteSpace(request.ShippingAddress) ? shippingAddress ?? dispatch.ShippingAddress : request.ShippingAddress, request.ContactPerson, request.ContactPhone, request.NetWeight, request.GrossWeight, request.TransporterName, request.VehicleNumber, request.LRGRNumber, request.LRGRDate, request.EWayBillNumber, request.EWayBillDate, request.PackingRemarks, request.DispatchRemarks, packages);
        foreach (var package in packages) db.Entry(package).State = EntityState.Added;
    }
    private void AddInventoryTransaction(Dispatch dispatch, WorkOrder wo, CoilType type, Guid coilId, string number, CoilStatus before, CoilStatus after, decimal quantity, DateTimeOffset now)
    { var entry = new InventoryTransaction(InventoryTransactionType.Dispatch, type, coilId, number, dispatch.Id, dispatch.DispatchNumber, before, after, quantity, now, $"Packing Slip {dispatch.PackingSlipNumber}; Work Order {wo.WorkOrderNumber}"); entry.SetCreatedAudit(Actor, now); db.InventoryTransactions.Add(entry); }
    private async Task<string> NextNumber(string prefix, CancellationToken ct)
    { int year = DateTime.UtcNow.Year; string start = $"{prefix}/{year}/"; IQueryable<string> query = prefix == "DSP" ? db.Dispatches.Where(x => x.DispatchNumber.StartsWith(start)).Select(x => x.DispatchNumber) : db.Dispatches.Where(x => x.PackingSlipNumber.StartsWith(start)).Select(x => x.PackingSlipNumber); string[] numbers = await query.ToArrayAsync(ct); int max = numbers.Select(x => int.TryParse(x[start.Length..], out int value) ? value : 0).DefaultIfEmpty().Max(); return $"{start}{max + 1:00000}"; }
    private void SetVersion(Dispatch dispatch, string? value) { if (!string.IsNullOrWhiteSpace(value)) db.Entry(dispatch).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(value); }
    private static DispatchDetailsDto Map(Dispatch x) => new(x.Id, x.DispatchNumber, x.PackingSlipNumber, x.WorkOrderId, x.WorkOrderNumber, x.SalesOrderNumber, x.CustomerCode, x.CustomerName, x.CustomerPONumber, x.ProductType, x.Description, x.GradeCode, x.Thickness, x.Width, x.DrawingNumber, x.DrawingRevision, x.OEMJobNumber, x.TransformerRating, x.Category, x.DispatchQuantity, x.QuantityUnit, x.NetWeight, x.GrossWeight, x.Packages.Count, x.DispatchDate, x.TransporterName, x.VehicleNumber, x.LRGRNumber, x.LRGRDate, x.EWayBillNumber, x.EWayBillDate, x.ShippingAddress, x.ContactPerson, x.ContactPhone, x.PackingRemarks, x.DispatchRemarks, x.Status, x.DispatchedBy, x.DispatchedOn, Convert.ToBase64String(x.RowVersion), x.Packages.OrderBy(p => p.Sequence).Select(p => new DispatchPackageDto(p.Id, p.PackageNumber, p.Description, p.Quantity, p.QuantityUnit, p.NetWeight, p.GrossWeight, p.Remarks, p.Sequence)).ToArray(), x.InventorySources.Select(s => new DispatchInventorySourceDto(s.InventoryType, s.InventoryId, s.InventoryNumber, s.Width, s.Quantity)).ToArray());
    private static string PackingHtml(Dispatch d) => $"<!doctype html><html><head><style>body{{font:14px Arial;margin:35px}}h1{{text-align:center}}.draft{{color:#c00}}table{{width:100%;border-collapse:collapse}}td,th{{border:1px solid #999;padding:8px}}</style></head><body><h1 class='{(d.Status == DispatchStatus.Draft ? "draft" : "")}'>{(d.Status == DispatchStatus.Draft ? "DRAFT " : "")}PACKING SLIP</h1><p><b>{d.PackingSlipNumber}</b> | Dispatch {d.DispatchNumber}</p><p>{d.CustomerName}<br>{d.ShippingAddress}<br>WO: {d.WorkOrderNumber}</p><table><tr><th>Package / Coil</th><th>Description</th><th>Quantity</th></tr>{string.Join("", d.Packages.Select(x => $"<tr><td>{x.PackageNumber}</td><td>{x.Description}</td><td>{x.Quantity:N3} {x.QuantityUnit}</td></tr>"))}{string.Join("", d.InventorySources.Select(x => $"<tr><td>{x.InventoryNumber}</td><td>{x.Width:N3} mm</td><td>{x.Quantity:N3} kg</td></tr>"))}</table><p>Vehicle: {d.VehicleNumber} | Transporter: {d.TransporterName}</p></body></html>";
}
