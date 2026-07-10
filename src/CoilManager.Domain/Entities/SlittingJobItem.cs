using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class SlittingJobItem : BaseEntity
{
    private SlittingJobItem()
    {
    }

    public SlittingJobItem(
        int sequenceNo,
        string slitCoilId,
        decimal width,
        decimal estimatedWeight,
        string? remarks = null)
    {
        SequenceNo = sequenceNo;
        SlitCoilId = slitCoilId;
        Width = width;
        EstimatedWeight = estimatedWeight;
        Remarks = Normalize(remarks);
        Status = SlittingJobStatus.Draft;
    }

    public Guid SlittingJobId { get; private set; }
    public SlittingJob? SlittingJob { get; private set; }
    public int SequenceNo { get; private set; }
    public string SlitCoilId { get; private set; } = string.Empty;
    public decimal Width { get; private set; }
    public decimal EstimatedWeight { get; private set; }
    public SlittingJobStatus Status { get; private set; }
    public string? Remarks { get; private set; }

    public void Update(decimal width, decimal estimatedWeight, string? remarks)
    {
        Width = width;
        EstimatedWeight = estimatedWeight;
        Remarks = Normalize(remarks);
    }

    public void UpdatePlanning(int sequenceNo, string slitCoilId, decimal width, decimal estimatedWeight, string? remarks)
    {
        SequenceNo = sequenceNo;
        SlitCoilId = slitCoilId;
        Update(width, estimatedWeight, remarks);
    }

    public void SetStatus(SlittingJobStatus status)
    {
        Status = status;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
