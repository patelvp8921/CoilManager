using System.ComponentModel.DataAnnotations.Schema;
using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class WorkOrder : AuditableEntity
{
    private readonly List<WorkOrderOperation> _operations = [];
    private readonly List<WorkOrderMaterialAllocation> _allocations = [];
    private WorkOrder() { }

    public WorkOrder(string number, WorkOrderType type, WorkOrderProductType productType,
        string? customerName, string? salesOrderReference, DateOnly workOrderDate, DateOnly? requiredDate,
        int priority, Guid? gradeId, decimal thickness, string category, decimal coreLossPerKg,
        string? drawingNumber, decimal? requiredWidth, decimal? requiredWeight, int? requiredQuantity, string? remarks)
    {
        WorkOrderNumber = number;
        Apply(type, productType, customerName, salesOrderReference, workOrderDate, requiredDate, priority,
            gradeId, thickness, category, coreLossPerKg, drawingNumber, requiredWidth, requiredWeight, requiredQuantity, remarks);
        Status = WorkOrderStatus.Draft;
        ConfigureRouting(productType);
    }

    public string WorkOrderNumber { get; private set; } = string.Empty;
    public WorkOrderSourceType SourceType { get; private set; } = WorkOrderSourceType.StockProduction;
    public Guid? SalesOrderId { get; private set; }
    public SalesOrder? SalesOrder { get; private set; }
    public Guid? SalesOrderLineId { get; private set; }
    public SalesOrderLine? SalesOrderLine { get; private set; }
    public string? SalesOrderNumber { get; private set; }
    public int? SalesOrderLineNumber { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string? CustomerCode { get; private set; }
    public string? CustomerPONumber { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? GradeCode { get; private set; }
    public decimal? Length { get; private set; }
    public string? DrawingRevision { get; private set; }
    public string? OEMJobNumber { get; private set; }
    public string? TransformerRating { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; } = QuantityUnit.Kg;
    public DateOnly? PlannedStartDate { get; private set; }
    public string? Planner { get; private set; }
    public FulfilmentStrategy FulfilmentStrategy { get; private set; } = FulfilmentStrategy.ProductionOnly;
    public decimal PlannedInventoryQuantity { get; private set; }
    public decimal PlannedProductionQuantity { get; private set; }
    public decimal UnplannedQuantity => Math.Max(0, PlanningRequiredQuantity - PlannedInventoryQuantity - PlannedProductionQuantity);
    public ProductionRoute ProductionRoute { get; private set; }
    public decimal ReservedInventoryQuantity { get; private set; }
    public decimal ProducedQuantity { get; private set; }
    public decimal ReadyQuantity { get; private set; }
    public decimal DispatchedQuantity { get; private set; }
    public decimal PlanningRequiredQuantity { get; private set; }
    public decimal CoveragePercentage => PlanningRequiredQuantity <= 0 ? 0 : Math.Min(100, 100 * (PlannedInventoryQuantity + PlannedProductionQuantity) / PlanningRequiredQuantity);
    public string? CancellationReason { get; private set; }
    public WorkOrderType WorkOrderType { get; private set; }
    public WorkOrderProductType ProductType { get; private set; }
    public string? CustomerName { get; private set; }
    public string? SalesOrderReference { get; private set; }
    public DateOnly WorkOrderDate { get; private set; }
    public DateOnly? RequiredDate { get; private set; }
    public int Priority { get; private set; }
    public Guid? GradeId { get; private set; }
    public Grade? Grade { get; private set; }
    public decimal Thickness { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal CoreLossPerKg { get; private set; }
    public string? DrawingNumber { get; private set; }
    public decimal? RequiredWidth { get; private set; }
    public decimal? RequiredWeight { get; private set; }
    public int? RequiredQuantity { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public string? Remarks { get; private set; }
    public string? ReleasedBy { get; private set; }
    public DateTimeOffset? ReleasedOn { get; private set; }
    public string? StartedBy { get; private set; }
    public DateTimeOffset? StartedOn { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTimeOffset? CompletedOn { get; private set; }
    public string? ClosedBy { get; private set; }
    public DateTimeOffset? ClosedOn { get; private set; }
    public string? CancelledBy { get; private set; }
    public DateTimeOffset? CancelledOn { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<WorkOrderOperation> Operations => _operations;
    public IReadOnlyCollection<WorkOrderMaterialAllocation> Allocations => _allocations;
    [NotMapped] public DateTimeOffset CreatedOn => CreatedAtUtc;
    [NotMapped] public DateTimeOffset? ModifiedOn => UpdatedAtUtc;
    [NotMapped] public string? ModifiedBy => UpdatedBy;

    public void InitializeSalesOrderPlan(SalesOrder order, SalesOrderLine line, decimal quantity, DateOnly? plannedStartDate,
        DateOnly requiredDate, int priority, string? planner, FulfilmentStrategy strategy, decimal inventoryQuantity,
        decimal productionQuantity, ProductionRoute route, string? remarks)
    {
        if (order.Status != SalesOrderStatus.Confirmed) throw new InvalidOperationException("Only confirmed Sales Orders may generate Work Orders.");
        if (quantity <= 0) throw new ArgumentException("Required Quantity must be greater than zero.");
        SourceType = WorkOrderSourceType.SalesOrder; SalesOrderId = order.Id; SalesOrder = order;
        SalesOrderLineId = line.Id; SalesOrderLine = line; SalesOrderNumber = order.SalesOrderNumber;
        SalesOrderLineNumber = line.LineNumber; CustomerId = order.CustomerId; Customer = order.Customer;
        CustomerCode = order.CustomerCode; CustomerName = order.CustomerName; CustomerPONumber = order.CustomerPONumber;
        ProductType = (WorkOrderProductType)line.ProductType; Description = line.Description; GradeId = line.GradeId;
        GradeCode = line.GradeCode; Thickness = line.Thickness ?? 0; Category = line.Category ?? string.Empty;
        CoreLossPerKg = line.CoreLossPerKg ?? 0; RequiredWidth = line.Width; Length = line.Length;
        DrawingNumber = line.DrawingNumber; DrawingRevision = line.DrawingRevision; OEMJobNumber = line.OEMJobNumber;
        TransformerRating = line.TransformerRating; PlanningRequiredQuantity = quantity; QuantityUnit = line.QuantityUnit;
        RequiredWeight = line.QuantityUnit == QuantityUnit.Kg ? quantity : null;
        RequiredQuantity = line.QuantityUnit == QuantityUnit.Kg ? null : checked((int)quantity);
        SalesOrderReference = order.SalesOrderNumber;
        RequiredDate = requiredDate; PlannedStartDate = plannedStartDate; Priority = priority; Planner = Normalize(planner); Remarks = Normalize(remarks);
        ConfigureProductFulfilment();
    }

    public void ConfigureFulfilment(FulfilmentStrategy strategy, decimal inventoryQuantity, decimal productionQuantity, ProductionRoute route)
    {
        if (Status != WorkOrderStatus.Draft) throw new InvalidOperationException("Only draft Work Orders can be edited.");
        ConfigureProductFulfilment();
    }

    public void ConfigureProductFulfilment()
    {
        if (ProductType is WorkOrderProductType.MotherCoil or WorkOrderProductType.SlitCoil)
        {
            FulfilmentStrategy = FulfilmentStrategy.ExistingInventoryOnly;
            PlannedInventoryQuantity = PlanningRequiredQuantity;
            PlannedProductionQuantity = 0;
            ProductionRoute = ProductionRoute.None;
            return;
        }
        if (ProductType == WorkOrderProductType.Lamination)
        {
            FulfilmentStrategy = FulfilmentStrategy.ProductionOnly;
            PlannedInventoryQuantity = 0;
            PlannedProductionQuantity = PlanningRequiredQuantity;
            ProductionRoute = ProductionRoute.LaminationOnly;
            return;
        }
        throw new NotSupportedException("Core Frame Assembly is coming soon.");
    }

    public static void ValidateRoute(SalesOrderProductType productType, decimal productionQuantity, ProductionRoute route)
    {
        if (productionQuantity == 0 && route != ProductionRoute.None) throw new ArgumentException("Production Route must be None when no production is planned.");
        if (productionQuantity > 0 && productType == SalesOrderProductType.SlitCoil && route != ProductionRoute.SlittingOnly) throw new ArgumentException("Slit Coil production requires the Slitting Only route.");
        if (productionQuantity > 0 && productType == SalesOrderProductType.Lamination && route is not (ProductionRoute.LaminationOnly or ProductionRoute.SlittingAndLamination)) throw new ArgumentException("Lamination production requires Lamination Only or Slitting and Lamination.");
        if (productionQuantity > 0 && productType == SalesOrderProductType.MotherCoil && route != ProductionRoute.None) throw new ArgumentException("Mother Coil does not support a production route in this sprint.");
        if (productType == SalesOrderProductType.CoreFrameAssembly) throw new NotSupportedException("Core Frame Assembly route planning is not available yet.");
    }
    public void Update(WorkOrderType type, WorkOrderProductType productType, string? customerName,
        string? salesOrderReference, DateOnly workOrderDate, DateOnly? requiredDate, int priority,
        Guid? gradeId, decimal thickness, string category, decimal coreLossPerKg, string? drawingNumber,
        decimal? requiredWidth, decimal? requiredWeight, int? requiredQuantity, string? remarks)
    {
        if (Status != WorkOrderStatus.Draft) throw new InvalidOperationException("Only draft Work Orders can be edited.");
        bool routeChanged = ProductType != productType;
        Apply(type, productType, customerName, salesOrderReference, workOrderDate, requiredDate, priority,
            gradeId, thickness, category, coreLossPerKg, drawingNumber, requiredWidth, requiredWeight, requiredQuantity, remarks);
        if (routeChanged) ConfigureRouting(productType);
    }

    public void Release(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Draft); if (PlanningRequiredQuantity > 0 && CoveragePercentage != 100) throw new InvalidOperationException("Work Order requires 100% coverage before release."); Status = WorkOrderStatus.Released; ReleasedBy = actor; ReleasedOn = at; }
    public void Start(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Released); Status = WorkOrderStatus.InProduction; StartedBy = actor; StartedOn = at; }
    public void Complete(string actor, DateTimeOffset at)
    {
        Require(WorkOrderStatus.InProduction);
        if (_operations.Any(x => x.IsRequired && x.Status != WorkOrderOperationStatus.Completed))
            throw new InvalidOperationException("Work Order cannot be completed while required operations are pending.");
        Status = WorkOrderStatus.Completed; CompletedBy = actor; CompletedOn = at;
    }
    public void RecordDispatch(decimal totalDispatched, string actor, DateTimeOffset at)
    {
        if (Status is not (WorkOrderStatus.Ready or WorkOrderStatus.PartiallyDispatched)) throw new InvalidOperationException("Only Ready or Partially Dispatched Work Orders can be dispatched.");
        if (totalDispatched < 0 || totalDispatched > PlanningRequiredQuantity) throw new InvalidOperationException("Dispatched quantity is outside the Work Order requirement.");
        DispatchedQuantity = totalDispatched;
        if (totalDispatched >= PlanningRequiredQuantity)
        {
            Status = WorkOrderStatus.Completed; CompletedBy = actor; CompletedOn = at;
            WorkOrderOperation dispatch = _operations.Single(x => x.OperationType == WorkOrderOperationType.Dispatch);
            if (dispatch.Status is WorkOrderOperationStatus.Pending or WorkOrderOperationStatus.InProgress) dispatch.Complete(at);
        }
        else Status = WorkOrderStatus.PartiallyDispatched;
    }
    public void Close(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Completed); Status = WorkOrderStatus.Closed; ClosedBy = actor; ClosedOn = at; }
    public void Cancel(string actor, DateTimeOffset at) => Cancel("Cancelled", actor, at);
    public void Cancel(string reason, string actor, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancellation reason is required.");
        if (Status != WorkOrderStatus.Draft && Status != WorkOrderStatus.Released)
            throw new InvalidOperationException("Only draft or unstarted released Work Orders can be cancelled.");
        if (Status == WorkOrderStatus.Released && _operations.Any(x => x.Status == WorkOrderOperationStatus.InProgress))
            throw new InvalidOperationException("Released Work Order cannot be cancelled after production has started.");
        Status = WorkOrderStatus.Cancelled; CancellationReason = reason.Trim(); CancelledBy = actor; CancelledOn = at;
        foreach (var operation in _operations) operation.Cancel();
        foreach (var allocation in _allocations.Where(x => x.IsActive)) allocation.Release(at, actor);
    }
    public void AddAllocation(WorkOrderMaterialAllocation allocation) => _allocations.Add(allocation);

    public void RecalculateFulfilment(decimal reservedInventoryQuantity, decimal producedQuantity, bool executionExists)
    {
        ReservedInventoryQuantity = Math.Max(0, reservedInventoryQuantity);
        ProducedQuantity = Math.Max(0, producedQuantity);
        ReadyQuantity = Math.Min(PlanningRequiredQuantity, ReservedInventoryQuantity + ProducedQuantity);
        if (Status is WorkOrderStatus.Draft or WorkOrderStatus.Cancelled or WorkOrderStatus.Completed or WorkOrderStatus.PartiallyDispatched) return;
        Status = ReadyQuantity >= PlanningRequiredQuantity && PlanningRequiredQuantity > 0
            ? WorkOrderStatus.Ready
            : ReadyQuantity > 0
                ? WorkOrderStatus.PartiallyReady
                : executionExists ? WorkOrderStatus.InFulfilment : WorkOrderStatus.Released;
    }

    private void ConfigureRouting(WorkOrderProductType productType)
    {
        if (productType == WorkOrderProductType.CoreFrameAssembly) throw new NotSupportedException("Core Frame Assembly is coming soon.");
        _operations.Clear();
        _operations.Add(new(WorkOrderOperationType.Slitting, 1, false));
        _operations.Add(new(WorkOrderOperationType.Lamination, 2, productType == WorkOrderProductType.Lamination));
        _operations.Add(new(WorkOrderOperationType.Dispatch, 3, true));
    }

    private void Apply(WorkOrderType type, WorkOrderProductType productType, string? customerName,
        string? salesOrderReference, DateOnly workOrderDate, DateOnly? requiredDate, int priority,
        Guid? gradeId, decimal thickness, string category, decimal coreLossPerKg, string? drawingNumber,
        decimal? requiredWidth, decimal? requiredWeight, int? requiredQuantity, string? remarks)
    {
        WorkOrderType = type; ProductType = productType; CustomerName = Normalize(customerName);
        SalesOrderReference = Normalize(salesOrderReference); WorkOrderDate = workOrderDate; RequiredDate = requiredDate;
        Priority = priority; GradeId = gradeId; Thickness = thickness; Category = category.Trim(); CoreLossPerKg = coreLossPerKg;
        DrawingNumber = Normalize(drawingNumber); RequiredWidth = requiredWidth; RequiredWeight = requiredWeight;
        RequiredQuantity = requiredQuantity; PlanningRequiredQuantity = requiredWeight ?? requiredQuantity ?? 0; QuantityUnit = requiredWeight.HasValue ? QuantityUnit.Kg : QuantityUnit.Pieces; Remarks = Normalize(remarks); ConfigureProductFulfilment();
    }
    private void Require(WorkOrderStatus expected) { if (Status != expected) throw new InvalidOperationException($"Invalid Work Order transition from {Status}; expected {expected}."); }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
