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

    public void Release(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Draft); Status = WorkOrderStatus.Released; ReleasedBy = actor; ReleasedOn = at; }
    public void Start(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Released); Status = WorkOrderStatus.InProduction; StartedBy = actor; StartedOn = at; }
    public void Complete(string actor, DateTimeOffset at)
    {
        Require(WorkOrderStatus.InProduction);
        if (_operations.Any(x => x.IsRequired && x.Status != WorkOrderOperationStatus.Completed))
            throw new InvalidOperationException("Work Order cannot be completed while required operations are pending.");
        Status = WorkOrderStatus.Completed; CompletedBy = actor; CompletedOn = at;
    }
    public void Close(string actor, DateTimeOffset at) { Require(WorkOrderStatus.Completed); Status = WorkOrderStatus.Closed; ClosedBy = actor; ClosedOn = at; }
    public void Cancel(string actor, DateTimeOffset at)
    {
        if (Status != WorkOrderStatus.Draft && Status != WorkOrderStatus.Released)
            throw new InvalidOperationException("Only draft or unstarted released Work Orders can be cancelled.");
        if (Status == WorkOrderStatus.Released && _operations.Any(x => x.Status == WorkOrderOperationStatus.InProgress))
            throw new InvalidOperationException("Released Work Order cannot be cancelled after production has started.");
        Status = WorkOrderStatus.Cancelled; CancelledBy = actor; CancelledOn = at;
        foreach (var operation in _operations) operation.Cancel();
        foreach (var allocation in _allocations.Where(x => x.IsActive)) allocation.Release(at, actor);
    }
    public void AddAllocation(WorkOrderMaterialAllocation allocation) => _allocations.Add(allocation);

    private void ConfigureRouting(WorkOrderProductType productType)
    {
        if (productType == WorkOrderProductType.CoreFrameAssembly) throw new NotSupportedException("Core Frame Assembly is coming soon.");
        _operations.Clear();
        _operations.Add(new(WorkOrderOperationType.Slitting, 1, productType is WorkOrderProductType.SlitCoil or WorkOrderProductType.Lamination));
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
        RequiredQuantity = requiredQuantity; Remarks = Normalize(remarks);
    }
    private void Require(WorkOrderStatus expected) { if (Status != expected) throw new InvalidOperationException($"Invalid Work Order transition from {Status}; expected {expected}."); }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
