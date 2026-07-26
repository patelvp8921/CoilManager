using CoilManager.Application.DTOs.Dashboard;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Application.DTOs.WorkOrders;
using CoilManager.Application.DTOs.LaminationJobs;

namespace CoilManager.Application.Services;

public sealed class OperationsDashboardService(
    IRawCoilRepository rawCoilRepository,
    ISlittingJobRepository slittingJobRepository,
    ISlitCoilRepository? slitCoilRepository = null,
    ISlitCoilLabelPrintHistoryRepository? labelHistoryRepository = null,
    IWorkOrderRepository? workOrderRepository = null,
    ILaminationJobService? laminationJobService = null) : IOperationsDashboardService
{
    private const string ComingSoon = "Coming soon";

    public async Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RawCoil> motherCoils = await rawCoilRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<SlittingJob> slittingJobs = await slittingJobRepository.GetForDashboardAsync(cancellationToken);
        IReadOnlyList<SlitCoil> slitCoils = slitCoilRepository is null ? [] : await slitCoilRepository.GetAllWithDetailsAsync(cancellationToken);
        IReadOnlyList<SlitCoilLabelPrintHistory> labelHistory = labelHistoryRepository is null ? [] : await labelHistoryRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<WorkOrder> workOrders = workOrderRepository is null ? [] : await workOrderRepository.GetForDashboardAsync(cancellationToken);
        LaminationMetricsDto? laminationMetrics = laminationJobService is null ? null : await laminationJobService.MetricsAsync(cancellationToken);
        RawCoil[] coils = motherCoils.ToArray();
        SlittingJob[] jobs = slittingJobs.ToArray();

        int totalMotherCoils = coils.Length;
        int availableMotherCoils = CountByStatus(coils, CoilStatus.Available);
        int consumedMotherCoils = CountByStatus(coils, CoilStatus.Consumed);
        int inProcessMotherCoils = CountByStatus(coils, CoilStatus.Reserved);
        int holdMotherCoils = CountByStatus(coils, CoilStatus.OnHold);
        int rejectedMotherCoils = CountByStatus(coils, CoilStatus.Rejected);
        decimal totalWeight = coils.Sum(coil => coil.Weight);
        decimal availableWeight = coils
            .Where(coil => coil.Status == CoilStatus.Available)
            .Sum(coil => coil.Weight);

        SlitCoil[] slitInventory = slitCoils.ToArray();
        int availableSlitCoils = slitInventory.Count(coil => coil.Status == CoilStatus.Available);
        int consumedSlitCoils = slitInventory.Count(coil => coil.Status == CoilStatus.Consumed);
        int inProcessSlitCoils = slitInventory.Count(coil => coil.Status == CoilStatus.InProcess);
        decimal totalSlitWeight = slitInventory.Sum(coil => coil.Weight);
        decimal availableSlitWeight = slitInventory.Where(coil => coil.Status == CoilStatus.Available).Sum(coil => coil.Weight);

        InventorySummaryDto inventory = new(
            totalMotherCoils,
            availableMotherCoils,
            consumedMotherCoils,
            inProcessMotherCoils,
            holdMotherCoils,
            rejectedMotherCoils,
            totalWeight,
            availableWeight,
            slitInventory.Length,
            availableSlitCoils,
            consumedSlitCoils,
            inProcessSlitCoils,
            totalSlitWeight,
            availableSlitWeight,
            BuildSlitGradeWiseStock(slitInventory),
            BuildGradeWiseStock(coils),
            BuildSupplierWiseStock(coils),
            BuildRecentReceivedCoils(coils));

        WorkOrderMetricsDto workOrderMetrics = BuildWorkOrderMetrics(workOrders);
        ProductionSummaryDto production = new(workOrders.Count, 0, workOrders.Sum(x => x.RequiredWeight ?? 0), 0, workOrders.Count > 0 ? "Live" : "Ready");
        SlittingJobMetricsDto slittingMetrics = BuildSlittingJobMetrics(jobs);
        SlittingSummaryDto slitting = new(slitCoils.Count, jobs.Length, slitCoils.Sum(coil => coil.Weight), BuildSlittingStatus(slittingMetrics));
        QualitySummaryDto quality = new(0, holdMotherCoils, rejectedMotherCoils, ComingSoon);
        ProcurementSummaryDto procurement = new(0, coils.Select(coil => coil.SupplierId).Distinct().Count(), 0, ComingSoon);
        DispatchSummaryDto dispatch = new(0, 0, 0, ComingSoon);
        AnalyticsSummaryDto analytics = new(BuildAnalyticsPlaceholders());

        return new OperationsDashboardDto(
            DashboardRole: GetDashboardRole(),
            GeneratedAt: DateTimeOffset.UtcNow,
            Kpis: BuildKpis(coils, slitCoils, jobs, slittingMetrics, laminationMetrics, dispatch),
            Inventory: inventory,
            Production: production,
            WorkOrders: workOrderMetrics,
            Slitting: slitting,
            SlittingJobMetrics: slittingMetrics,
            ProductionQueue: BuildProductionQueue(jobs),
            LaminationProductionQueue: laminationMetrics?.Queue ?? [],
            Quality: quality,
            Procurement: procurement,
            Dispatch: dispatch,
            Analytics: analytics,
            QuickActions: BuildQuickActions(),
            RecentActivities: BuildRecentActivities(inventory.RecentReceivedCoils),
            Notifications: BuildNotifications(holdMotherCoils, rejectedMotherCoils));
    }

    public static WorkOrderMetricsDto BuildWorkOrderMetrics(IReadOnlyList<WorkOrder> rows)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new(rows.Count(x => x.Status == WorkOrderStatus.Draft), rows.Count(x => x.Status == WorkOrderStatus.Released), rows.Count(x => x.Status == WorkOrderStatus.InProduction),
            rows.Count(x => x.Status == WorkOrderStatus.Completed && x.CompletedOn?.UtcDateTime.Date == DateTime.UtcNow.Date),
            rows.Count(x => x.Status == WorkOrderStatus.Completed),
            rows.Count(x => x.RequiredDate < today && x.Status is not (WorkOrderStatus.Completed or WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)),
            rows.Count(x => x.WorkOrderType == WorkOrderType.CustomerOrder), rows.Count(x => x.WorkOrderType == WorkOrderType.InventoryProduction),
            rows.Where(x => x.Status is WorkOrderStatus.Released or WorkOrderStatus.InProduction).OrderBy(x => x.RequiredDate).ThenByDescending(x => x.Priority)
                .Select(x => new WorkOrderQueueItemDto(x.Id, x.WorkOrderNumber, x.ProductType, x.RequiredDate, x.Priority, x.Status,
                    x.RequiredWeight > 0 ? Math.Min(100, 100 * x.Allocations.Where(a => a.IsActive).Sum(a => a.AllocatedWeight) / x.RequiredWeight.Value) : 0,
                    x.Operations.Count(o => o.IsRequired) == 0 ? 100 : 100m * x.Operations.Count(o => o.IsRequired && o.Status == WorkOrderOperationStatus.Completed) / x.Operations.Count(o => o.IsRequired))).ToArray());
    }

    private static int CountByStatus(IEnumerable<RawCoil> coils, CoilStatus status)
    {
        return coils.Count(coil => coil.Status == status);
    }

    private static IReadOnlyList<DashboardKpiDto> BuildKpis(
        IReadOnlyList<RawCoil> motherCoils,
        IReadOnlyList<SlitCoil> slitCoils,
        IReadOnlyList<SlittingJob> slittingJobs,
        SlittingJobMetricsDto slittingMetrics,
        LaminationMetricsDto? laminationMetrics,
        DispatchSummaryDto dispatch)
    {
        RawCoil[] availableMotherCoils = motherCoils.Where(coil => coil.Status == CoilStatus.Available).ToArray();
        RawCoil[] consumedMotherCoils = motherCoils.Where(coil => coil.Status == CoilStatus.Consumed).ToArray();
        RawCoil[] inProcessMotherCoils = motherCoils.Where(coil => coil.Status == CoilStatus.InProcess).ToArray();
        SlitCoil[] availableSlitCoils = slitCoils.Where(coil => coil.Status == CoilStatus.Available).ToArray();
        SlitCoil[] consumedSlitCoils = slitCoils.Where(coil => coil.Status == CoilStatus.Consumed).ToArray();
        SlitCoil[] inProcessSlitCoils = slitCoils.Where(coil => coil.Status == CoilStatus.InProcess).ToArray();
        int completedSlittingJobs = slittingJobs.Count(job => job.Status == SlittingJobStatus.Completed);

        return
        [
            new("Mother Coils", motherCoils.Count.ToString("N0"), "inventory_2", "primary", $"Total Weight: {motherCoils.Sum(coil => coil.Weight):N0} kg",
            [
                new("Available", availableMotherCoils.Length.ToString("N0"), $"{availableMotherCoils.Sum(coil => coil.Weight):N0} kg"),
                new("Consumed", consumedMotherCoils.Length.ToString("N0"), $"{consumedMotherCoils.Sum(coil => coil.Weight):N0} kg"),
                new("In Process", inProcessMotherCoils.Length.ToString("N0"), $"{inProcessMotherCoils.Sum(coil => coil.Weight):N0} kg")
            ]),
            new("Slit Coils", slitCoils.Count.ToString("N0"), "splitscreen", "primary", $"Total Weight: {slitCoils.Sum(coil => coil.Weight):N0} kg",
            [
                new("Available", availableSlitCoils.Length.ToString("N0"), $"{availableSlitCoils.Sum(coil => coil.Weight):N0} kg"),
                new("Consumed", consumedSlitCoils.Length.ToString("N0"), $"{consumedSlitCoils.Sum(coil => coil.Weight):N0} kg"),
                new("In Process", inProcessSlitCoils.Length.ToString("N0"), $"{inProcessSlitCoils.Sum(coil => coil.Weight):N0} kg")
            ]),
            new("Slitting Jobs", slittingJobs.Count.ToString("N0"), "precision_manufacturing", "neutral", $"In Progress: {slittingMetrics.InProgressJobs:N0}",
            [
                new("Draft", slittingMetrics.DraftJobs.ToString("N0")),
                new("Released", slittingMetrics.ReleasedJobs.ToString("N0")),
                new("In Progress", slittingMetrics.InProgressJobs.ToString("N0")),
                new("Completed", completedSlittingJobs.ToString("N0"))
            ]),
            new("CTL Jobs", (laminationMetrics?.Total ?? 0).ToString("N0"), "layers", "neutral", $"In Progress: {(laminationMetrics?.InProgress ?? 0):N0}",
            [
                new("Draft", (laminationMetrics?.Draft ?? 0).ToString("N0")),
                new("Released", (laminationMetrics?.ReleasedAwaitingAllocation ?? 0).ToString("N0")),
                new("Allocated", (laminationMetrics?.AllocatedReadyForCompletion ?? 0).ToString("N0")),
                new("In Progress", (laminationMetrics?.InProgress ?? 0).ToString("N0")),
                new("Completed", (laminationMetrics?.Completed ?? 0).ToString("N0"))
            ]),
            new("Dispatches", dispatch.Dispatches.ToString("N0"), "local_shipping", "neutral", $"Pending: {dispatch.PendingDispatches:N0}",
            [
                new("Pending", dispatch.PendingDispatches.ToString("N0")),
                new("Completed", dispatch.Dispatches.ToString("N0"))
            ])
        ];
    }
    private static SlittingJobMetricsDto BuildSlittingJobMetrics(IReadOnlyList<SlittingJob> jobs)
    {
        DateTime today = DateTime.UtcNow.Date;
        decimal averageWaiting = AverageMinutes(jobs
            .Where(job => job.ReleasedOn.HasValue && job.StartedOn.HasValue)
            .Select(job => job.StartedOn!.Value - job.ReleasedOn!.Value));
        decimal averageProcessing = AverageMinutes(jobs
            .Where(job => job.StartedOn.HasValue && job.CompletedOn.HasValue)
            .Select(job => job.CompletedOn!.Value - job.StartedOn!.Value));

        return new SlittingJobMetricsDto(
            jobs.Count(job => job.Status == SlittingJobStatus.Draft),
            jobs.Count(job => job.Status == SlittingJobStatus.Released),
            jobs.Count(job => job.Status == SlittingJobStatus.InProgress),
            jobs.Count(job => job.Status == SlittingJobStatus.Completed && job.CompletedOn?.UtcDateTime.Date == today),
            jobs.Count(job => job.Status == SlittingJobStatus.Cancelled),
            averageWaiting,
            averageProcessing);
    }

    private static IReadOnlyList<ProductionQueueItemDto> BuildProductionQueue(IReadOnlyList<SlittingJob> jobs)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return jobs
            .Where(job => job.Status is SlittingJobStatus.InProgress or SlittingJobStatus.Released)
            .OrderBy(job => job.Status == SlittingJobStatus.InProgress ? 0 : 1)
            .ThenBy(job => job.ReleasedOn ?? DateTimeOffset.MaxValue)
            .Select(job => new ProductionQueueItemDto(
                job.Id,
                job.SlittingJobNo,
                job.MotherCoil?.RawCoilNumber ?? "-",
                job.Status == SlittingJobStatus.Released ? "Waiting to Start" : "Running",
                job.ReleasedOn,
                job.StartedOn,
                CalculateWaitingMinutes(job, now),
                job.MachineId,
                job.Shift,
                job.Status == SlittingJobStatus.InProgress
                    ? $"/slitting-jobs/{job.Id}/complete"
                    : $"/slitting-jobs/{job.Id}/edit"))
            .ToArray();
    }

    private static decimal AverageMinutes(IEnumerable<TimeSpan> durations)
    {
        TimeSpan[] values = durations.ToArray();
        return values.Length == 0
            ? 0
            : Math.Round((decimal)values.Average(duration => duration.TotalMinutes), 1);
    }

    private static decimal CalculateWaitingMinutes(SlittingJob job, DateTimeOffset now)
    {
        DateTimeOffset? start = job.Status == SlittingJobStatus.InProgress
            ? job.StartedOn
            : job.ReleasedOn;

        return start.HasValue
            ? Math.Max(0, Math.Round((decimal)(now - start.Value).TotalMinutes, 1))
            : 0;
    }

    private static string BuildSlittingStatus(SlittingJobMetricsDto metrics)
    {
        return metrics.InProgressJobs > 0
            ? "Running"
            : metrics.ReleasedJobs > 0
                ? "Waiting to Start"
                : "Ready";
    }

    private static IReadOnlyList<InventoryBreakdownDto> BuildGradeWiseStock(IEnumerable<RawCoil> coils)
    {
        return coils
            .Where(coil => coil.Status == CoilStatus.Available)
            .GroupBy(coil => string.IsNullOrWhiteSpace(coil.Grade?.Code) ? "Unassigned" : coil.Grade.Code)
            .OrderBy(group => group.Key)
            .Select(group => new InventoryBreakdownDto(group.Key, group.Count(), group.Sum(coil => coil.Weight)))
            .ToArray();
    }

    private static IReadOnlyList<InventoryBreakdownDto> BuildSlitGradeWiseStock(IEnumerable<SlitCoil> coils)
    {
        return coils
            .Where(coil => coil.Status == CoilStatus.Available)
            .GroupBy(coil => string.IsNullOrWhiteSpace(coil.Grade?.Code) ? "Unassigned" : coil.Grade.Code)
            .OrderBy(group => group.Key)
            .Select(group => new InventoryBreakdownDto(group.Key, group.Count(), group.Sum(coil => coil.Weight)))
            .ToArray();
    }
    private static IReadOnlyList<InventoryBreakdownDto> BuildSupplierWiseStock(IEnumerable<RawCoil> coils)
    {
        return coils
            .GroupBy(coil => string.IsNullOrWhiteSpace(coil.Supplier?.Name) ? "Unassigned" : coil.Supplier.Name)
            .OrderBy(group => group.Key)
            .Select(group => new InventoryBreakdownDto(group.Key, group.Count(), group.Sum(coil => coil.Weight)))
            .ToArray();
    }

    private static IReadOnlyList<RecentMotherCoilDto> BuildRecentReceivedCoils(IEnumerable<RawCoil> coils)
    {
        return coils
            .OrderByDescending(coil => coil.ReceivedDate)
            .ThenByDescending(coil => coil.CreatedAtUtc)
            .Take(6)
            .Select(coil => new RecentMotherCoilDto(
                coil.Id,
                coil.RawCoilNumber,
                coil.Grade?.Code ?? "Unassigned",
                coil.Supplier?.Name ?? "Unassigned",
                coil.Weight,
                coil.ReceivedDate,
                StatusLabel(coil.Status)))
            .ToArray();
    }

    private static IReadOnlyList<AnalyticsPlaceholderDto> BuildAnalyticsPlaceholders()
    {
        return
        [
            new("Grade-wise Inventory", "Chart placeholder for grade stock distribution.", "bar_chart"),
            new("Supplier-wise Inventory", "Chart placeholder for supplier stock distribution.", "donut_large"),
            new("Monthly Receipts", "Chart placeholder for monthly received weight.", "calendar_month"),
            new("Production Trend", "Chart placeholder for finished coil production.", "trending_up"),
            new("Scrap Trend", "Chart placeholder for scrap movement.", "stacked_line_chart"),
            new("Yield Trend", "Chart placeholder for process yield.", "show_chart")
        ];
    }

    private static IReadOnlyList<QuickActionDto> BuildQuickActions()
    {
        return
        [
            new("Receive Mother Coil", "add_circle", "/mother-coils/create", true),
            new("View Mother Coil Inventory", "inventory_2", "/mother-coils", true),
            new("Manage Grades", "category", "/admin/grades", true),
            new("Manage Suppliers", "storefront", "/admin/suppliers", true),
            new("Manage Manufacturers", "factory", "/admin/manufacturers", true),
            new("Create Work Order", "assignment_add", "/work-orders/create", true),
            new("Create Slitting Job", "precision_manufacturing", null, false, ComingSoon),
            new("Print Pending Slit Coil Labels", "print", "/slit-coils/labels/batch", true)
        ];
    }

    private static IReadOnlyList<RecentActivityDto> BuildRecentActivities(IReadOnlyList<RecentMotherCoilDto> recentCoils)
    {
        if (recentCoils.Count == 0)
        {
            return
            [
                new("No recent receipts", "Mother coil receipts will appear here once inventory is received.", DateTimeOffset.UtcNow, "inventory_2", "neutral"),
                new("Production module", "Work order activity will appear here when the module is enabled.", DateTimeOffset.UtcNow, "assignment", "neutral"),
                new("Dispatch module", "Dispatch activity will appear here when the module is enabled.", DateTimeOffset.UtcNow, "local_shipping", "neutral")
            ];
        }

        return recentCoils
            .Select(coil => new RecentActivityDto(
                "Mother coil received",
                $"{coil.CoilId} - {coil.Grade} - {coil.Weight:N3} kg from {coil.Supplier}",
                coil.ReceivedDate.ToDateTime(TimeOnly.MinValue),
                "inventory_2",
                "success",
                $"/mother-coils/{coil.Id}/details"))
            .ToArray();
    }

    private static IReadOnlyList<NotificationDto> BuildNotifications(int holdCoils, int rejectedCoils)
    {
        List<NotificationDto> notifications = [];

        if (rejectedCoils > 0)
        {
            notifications.Add(new("Rejected coils", $"{rejectedCoils:N0} mother coil(s) are rejected.", "critical", "error", "/mother-coils"));
        }

        if (holdCoils > 0)
        {
            notifications.Add(new("Hold coils", $"{holdCoils:N0} mother coil(s) are on hold or reserved.", "warning", "pause_circle", "/mother-coils"));
        }

        notifications.Add(new("Low inventory", "Low inventory thresholds are not configured yet.", "info", "inventory"));
        notifications.Add(new("Pending QA", "QA workflow is coming soon.", "info", "rule"));
        notifications.Add(new("Overdue work orders", "Work order tracking is coming soon.", "info", "assignment_late"));

        return notifications;
    }

    private static string StatusLabel(CoilStatus status)
    {
        return status switch
        {
            CoilStatus.Draft => "Draft",
            CoilStatus.Available => "Available",
            CoilStatus.Reserved => "Reserved",
            CoilStatus.InProcess => "In Process",
            CoilStatus.Rejected => "Rejected",
            CoilStatus.Consumed => "Consumed",
            CoilStatus.Dispatched => "Dispatched",
            CoilStatus.UnderInspection => "Under Inspection",
            _ => "Unknown"
        };
    }

    private static string GetDashboardRole()
    {
        return "Operations";
    }
}


