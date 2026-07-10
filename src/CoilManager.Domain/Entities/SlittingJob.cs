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

    public void Release()
    {
        if (Status != SlittingJobStatus.Draft)
        {
            throw new InvalidOperationException("Only draft slitting jobs can be released.");
        }

        Status = SlittingJobStatus.Released;
        foreach (SlittingJobItem item in _items)
        {
            item.SetStatus(SlittingJobStatus.Released);
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
