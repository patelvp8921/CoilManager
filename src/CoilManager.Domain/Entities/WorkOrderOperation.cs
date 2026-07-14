using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class WorkOrderOperation : BaseEntity
{
    private WorkOrderOperation() { }

    public WorkOrderOperation(WorkOrderOperationType operationType, int sequence, bool isRequired)
    {
        OperationType = operationType;
        Sequence = sequence;
        IsRequired = isRequired;
        Status = isRequired ? WorkOrderOperationStatus.Pending : WorkOrderOperationStatus.NotRequired;
    }

    public Guid WorkOrderId { get; private set; }
    public WorkOrder? WorkOrder { get; private set; }
    public WorkOrderOperationType OperationType { get; private set; }
    public int Sequence { get; private set; }
    public bool IsRequired { get; private set; }
    public WorkOrderOperationStatus Status { get; private set; }
    public Guid? RelatedDocumentId { get; private set; }
    public string? RelatedDocumentNumber { get; private set; }
    public DateTimeOffset? StartedOn { get; private set; }
    public DateTimeOffset? CompletedOn { get; private set; }
    public string? Remarks { get; private set; }

    public void MarkNotRequired(string? remarks = null)
    {
        if (OperationType != WorkOrderOperationType.Slitting || Status != WorkOrderOperationStatus.Pending)
            throw new InvalidOperationException("Only a pending Slitting operation may be marked not required.");
        IsRequired = false;
        Status = WorkOrderOperationStatus.NotRequired;
        Remarks = Normalize(remarks);
    }

    public void LinkDocument(Guid id, string number)
    {
        RelatedDocumentId = id;
        RelatedDocumentNumber = number;
    }

    public void Start(DateTimeOffset at)
    {
        if (!IsRequired || Status is not WorkOrderOperationStatus.Pending)
            throw new InvalidOperationException("Only a required pending operation can be started.");
        Status = WorkOrderOperationStatus.InProgress;
        StartedOn ??= at;
    }

    public void Complete(DateTimeOffset at)
    {
        if (Status is not (WorkOrderOperationStatus.Pending or WorkOrderOperationStatus.InProgress))
            throw new InvalidOperationException("Only a pending or in-progress operation can be completed.");
        Status = WorkOrderOperationStatus.Completed;
        StartedOn ??= at;
        CompletedOn = at;
    }

    public void Cancel() { if (IsRequired && Status != WorkOrderOperationStatus.Completed) Status = WorkOrderOperationStatus.Cancelled; }
    public void SynchronizeSlittingJob(SlittingJobStatus jobStatus, DateTimeOffset at)
    {
        if (OperationType != WorkOrderOperationType.Slitting || !IsRequired) return;
        if (jobStatus is SlittingJobStatus.Released or SlittingJobStatus.InProgress)
        {
            if (Status == WorkOrderOperationStatus.Pending) Start(at);
        }
        else if (jobStatus == SlittingJobStatus.Completed && Status is WorkOrderOperationStatus.Pending or WorkOrderOperationStatus.InProgress)
        {
            Complete(at);
        }
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
