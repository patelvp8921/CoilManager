using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoilManager.Domain.Entities;

public sealed class SlittingJob : AuditableEntity
{
    private readonly List<SlittingJobItem> _items = [];

    private SlittingJob()
    {
    }

    public SlittingJob(
        string slittingJobNo,
        DateOnly planningDate,
        string? plannerId,
        Guid motherCoilId,
        Guid? machineId,
        string? shift,
        decimal knifeThickness,
        decimal leftEdgeTrim,
        decimal rightEdgeTrim,
        string? remarks)
    {
        SlittingJobNo = slittingJobNo;
        PlanningDate = planningDate;
        PlannerId = Normalize(plannerId);
        MotherCoilId = motherCoilId;
        MachineId = machineId;
        Shift = Normalize(shift);
        KnifeThickness = knifeThickness;
        LeftEdgeTrim = leftEdgeTrim;
        RightEdgeTrim = rightEdgeTrim;
        Remarks = Normalize(remarks);
        Status = SlittingJobStatus.Draft;
    }

    public string SlittingJobNo { get; private set; } = string.Empty;
    public DateOnly PlanningDate { get; private set; }
    public string? PlannerId { get; private set; }
    public Guid MotherCoilId { get; private set; }
    public RawCoil? MotherCoil { get; private set; }
    public Guid? MachineId { get; private set; }
    public string? Shift { get; private set; }
    public SlittingJobStatus Status { get; private set; }
    public decimal KnifeThickness { get; private set; }
    public decimal LeftEdgeTrim { get; private set; }
    public decimal RightEdgeTrim { get; private set; }
    public string? Remarks { get; private set; }
    public string? ReleasedBy { get; private set; }
    public DateTimeOffset? ReleasedOn { get; private set; }
    public string? StartedBy { get; private set; }
    public DateTimeOffset? StartedOn { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTimeOffset? CompletedOn { get; private set; }
    public string? CancelledBy { get; private set; }
    public DateTimeOffset? CancelledOn { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<SlittingJobItem> Items => _items;

    [NotMapped]
    public DateTimeOffset CreatedOn => CreatedAtUtc;

    [NotMapped]
    public DateTimeOffset? ModifiedOn => UpdatedAtUtc;

    [NotMapped]
    public string? ModifiedBy => UpdatedBy;

    public void UpdatePlanning(
        DateOnly planningDate,
        string? plannerId,
        Guid motherCoilId,
        Guid? machineId,
        string? shift,
        decimal knifeThickness,
        decimal leftEdgeTrim,
        decimal rightEdgeTrim,
        string? remarks,
        IEnumerable<SlittingJobItem> items)
    {
        UpdatePlanningDetails(
            planningDate,
            plannerId,
            motherCoilId,
            machineId,
            shift,
            knifeThickness,
            leftEdgeTrim,
            rightEdgeTrim,
            remarks);

        ReplaceItems(items);
    }

    public void UpdatePlanningDetails(
        DateOnly planningDate,
        string? plannerId,
        Guid motherCoilId,
        Guid? machineId,
        string? shift,
        decimal knifeThickness,
        decimal leftEdgeTrim,
        decimal rightEdgeTrim,
        string? remarks)
    {
        if (Status != SlittingJobStatus.Draft)
        {
            throw new InvalidOperationException("Only draft slitting jobs can be edited.");
        }

        PlanningDate = planningDate;
        PlannerId = Normalize(plannerId);
        MotherCoilId = motherCoilId;
        MachineId = machineId;
        Shift = Normalize(shift);
        KnifeThickness = knifeThickness;
        LeftEdgeTrim = leftEdgeTrim;
        RightEdgeTrim = rightEdgeTrim;
        Remarks = Normalize(remarks);
    }

    public void ReplaceItems(IEnumerable<SlittingJobItem> items)
    {
        SlittingJobItem[] requestedItems = items
            .OrderBy(item => item.SequenceNo)
            .ToArray();

        if (_items.Count == 0)
        {
            _items.AddRange(requestedItems);
            return;
        }

        HashSet<int> requestedSequences = requestedItems
            .Select(item => item.SequenceNo)
            .ToHashSet();

        _items.RemoveAll(item => !requestedSequences.Contains(item.SequenceNo));

        foreach (SlittingJobItem requestedItem in requestedItems)
        {
            SlittingJobItem? existingItem = _items.FirstOrDefault(item => item.SequenceNo == requestedItem.SequenceNo);
            if (existingItem is null)
            {
                _items.Add(requestedItem);
                continue;
            }

            existingItem.UpdatePlanning(
                requestedItem.SequenceNo,
                requestedItem.SlitCoilId,
                requestedItem.Width,
                requestedItem.EstimatedWeight,
                requestedItem.Remarks);
        }
    }

    public void RebuildItems(IEnumerable<SlittingJobItem> items)
    {
        SlittingJobItem[] requestedItems = items
            .OrderBy(item => item.SequenceNo)
            .ToArray();

        _items.Clear();
        _items.AddRange(requestedItems);
    }

    public void Release(string? releasedBy, DateTimeOffset releasedOn)
    {
        if (Status != SlittingJobStatus.Draft)
        {
            throw new InvalidOperationException("Only draft slitting jobs can be released.");
        }

        Status = SlittingJobStatus.Released;
        ReleasedBy = Normalize(releasedBy);
        ReleasedOn = releasedOn;
        foreach (SlittingJobItem item in _items)
        {
            item.SetStatus(SlittingJobStatus.Released);
        }
    }

    public void Start(string? startedBy, DateTimeOffset startedOn, Guid? machineId, string? shift, string? remarks)
    {
        if (Status != SlittingJobStatus.Released)
        {
            throw new InvalidOperationException("Only released slitting jobs can be started.");
        }

        if (StartedOn.HasValue)
        {
            throw new InvalidOperationException("Slitting job is already started.");
        }

        Status = SlittingJobStatus.InProgress;
        StartedBy = Normalize(startedBy);
        StartedOn = startedOn;
        MachineId = machineId ?? MachineId;
        Shift = Normalize(shift) ?? Shift;
        Remarks = Normalize(remarks) ?? Remarks;
        foreach (SlittingJobItem item in _items)
        {
            item.SetStatus(SlittingJobStatus.InProgress);
        }
    }

    public void Complete(string? completedBy, DateTimeOffset completedOn)
    {
        if (Status != SlittingJobStatus.InProgress)
        {
            throw new InvalidOperationException("Only in progress slitting jobs can be completed.");
        }

        Status = SlittingJobStatus.Completed;
        CompletedBy = Normalize(completedBy);
        CompletedOn = completedOn;
    }

    public void Cancel(string? cancelledBy, DateTimeOffset cancelledOn)
    {
        if (Status != SlittingJobStatus.Released)
        {
            throw new InvalidOperationException("Only released slitting jobs can be cancelled.");
        }

        Status = SlittingJobStatus.Cancelled;
        CancelledBy = Normalize(cancelledBy);
        CancelledOn = cancelledOn;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
