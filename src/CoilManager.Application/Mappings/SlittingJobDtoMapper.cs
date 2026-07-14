using CoilManager.Application.DTOs.SlittingJobs;
using CoilManager.Application.Services;
using CoilManager.Domain.Entities;

namespace CoilManager.Application.Mappings;

public static class SlittingJobDtoMapper
{
    public static SlittingJobDto MapToDto(SlittingJob job)
    {
        RawCoil motherCoil = job.MotherCoil ?? throw new InvalidOperationException("Slitting job mother coil was not loaded.");
        SlittingPlanningSummary summary = SlittingPlanningCalculator.Calculate(
            motherCoil.Width,
            job.Items.Select(item => item.Width),
            job.KnifeThickness,
            job.LeftEdgeTrim,
            job.RightEdgeTrim);

        return new SlittingJobDto(
            job.Id,
            job.SlittingJobNo,
            job.PlanningDate,
            job.PlannerId,
            job.MotherCoilId,
            motherCoil.RawCoilNumber,
            motherCoil.CoilNumber,
            motherCoil.HeatNumber,
            motherCoil.Supplier?.Name,
            motherCoil.Manufacturer?.Name,
            motherCoil.Grade?.Code,
            motherCoil.Thickness,
            motherCoil.Category,
            motherCoil.CoreLossPerKg,
            motherCoil.Width,
            motherCoil.Weight,
            motherCoil.Length,
            motherCoil.WarehouseLocation,
            motherCoil.Status,
            job.MachineId,
            job.Shift,
            job.Status,
            job.KnifeThickness,
            job.LeftEdgeTrim,
            job.RightEdgeTrim,
            job.Remarks,
            summary.TotalPlannedWidth,
            summary.KnifeLoss,
            summary.EdgeTrim,
            summary.RemainingWidth,
            summary.UtilizationPercent,
            job.CreatedAtUtc,
            job.CreatedBy,
            job.UpdatedAtUtc,
            job.UpdatedBy,
            job.ReleasedBy,
            job.ReleasedOn,
            job.StartedBy,
            job.StartedOn,
            job.CompletedBy,
            job.CompletedOn,
            job.CancelledBy,
            job.CancelledOn,
            Convert.ToBase64String(job.RowVersion),
            job.Items
                .OrderBy(item => item.SequenceNo)
                .Select(item => new SlittingJobItemDto(
                    item.Id,
                    item.SequenceNo,
                    item.SlitCoilId,
                    item.Width,
                    item.EstimatedWeight,
                    item.Status,
                    item.Remarks))
                .ToArray(),
            job.ProductionType,
            job.WorkOrderId,
            job.WorkOrderNumber,
            job.WorkOrderOperationId);
    }
}
