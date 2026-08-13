using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class WorkOrderMaterialAllocation : BaseEntity
{
    private WorkOrderMaterialAllocation() { }

    public WorkOrderMaterialAllocation(Guid workOrderId, CoilType coilType, Guid coilId, string coilNumber,
        decimal allocatedWeight, decimal remainingWeightAfterAllocation, DateTimeOffset reservedOn,
        string reservedBy, string? remarks)
    {
        if (allocatedWeight <= 0) throw new ArgumentOutOfRangeException(nameof(allocatedWeight));
        WorkOrderId = workOrderId;
        CoilType = coilType;
        MotherCoilId = coilType == CoilType.MotherCoil ? coilId : null;
        SlitCoilId = coilType == CoilType.SlitCoil ? coilId : null;
        CoilNumber = coilNumber;
        AllocatedWeight = allocatedWeight;
        RemainingWeightAfterAllocation = remainingWeightAfterAllocation;
        Status = AllocationStatus.Reserved;
        ReservedOn = reservedOn;
        ReservedBy = reservedBy;
        Remarks = Normalize(remarks);
        CreatedOn = reservedOn;
        CreatedBy = reservedBy;
    }

    public Guid WorkOrderId { get; private set; }
    public WorkOrder? WorkOrder { get; private set; }
    public CoilType CoilType { get; private set; }
    public Guid? MotherCoilId { get; private set; }
    public RawCoil? MotherCoil { get; private set; }
    public Guid? SlitCoilId { get; private set; }
    public SlitCoil? SlitCoil { get; private set; }
    public string CoilNumber { get; private set; } = string.Empty;
    public decimal AllocatedWeight { get; private set; }
    public decimal? IssuedWeight { get; private set; }
    public decimal? ConsumedWeight { get; private set; }
    public decimal RemainingWeightAfterAllocation { get; private set; }
    public AllocationStatus Status { get; private set; }
    public DateTimeOffset ReservedOn { get; private set; }
    public string ReservedBy { get; private set; } = string.Empty;
    public DateTimeOffset? ReleasedOn { get; private set; }
    public string? ReleasedBy { get; private set; }
    public string? Remarks { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public bool IsActive => Status is AllocationStatus.Reserved or AllocationStatus.Issued or AllocationStatus.PartiallyConsumed;

    public void Adjust(decimal quantity, decimal remainingAfterAllocation, string? remarks)
    {
        if (!IsActive || Status != AllocationStatus.Reserved) throw new InvalidOperationException("Only a reserved allocation can be edited.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        AllocatedWeight = quantity; RemainingWeightAfterAllocation = remainingAfterAllocation;
        if (!string.IsNullOrWhiteSpace(remarks)) Remarks = remarks.Trim();
    }
    public void Release(DateTimeOffset at, string actor)
    {
        if (!IsActive) throw new InvalidOperationException("Only an active allocation can be released.");
        Status = AllocationStatus.Released;
        ReleasedOn = at;
        ReleasedBy = actor;
    }
    public void RecordDispatch(decimal quantity)
    {
        decimal dispatched = ConsumedWeight ?? 0;
        if (quantity <= 0 || dispatched + quantity > AllocatedWeight) throw new InvalidOperationException("Dispatch exceeds the allocated quantity.");
        ConsumedWeight = dispatched + quantity;
        Status = ConsumedWeight >= AllocatedWeight ? AllocationStatus.Consumed : AllocationStatus.PartiallyConsumed;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
