using CoilManager.Application.DTOs.Dashboard;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;

namespace CoilManager.Application.Services;

public sealed class OperationsDashboardService(
    IRawCoilRepository rawCoilRepository,
    ISlittingJobRepository slittingJobRepository) : IOperationsDashboardService
{
    private const string ComingSoon = "Coming soon";

    public async Task<OperationsDashboardDto> GetOperationsDashboardAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RawCoil> motherCoils = await rawCoilRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<SlittingJob> slittingJobs = await slittingJobRepository.GetForDashboardAsync(cancellationToken);
        RawCoil[] coils = motherCoils.ToArray();
        SlittingJob[] jobs = slittingJobs.ToArray();

        int totalMotherCoils = coils.Length;
        int availableMotherCoils = CountByStatus(coils, CoilStatus.Available);
        int reservedMotherCoils = CountByStatus(coils, CoilStatus.Reserved);
        int holdMotherCoils = CountByStatus(coils, CoilStatus.OnHold);
        int rejectedMotherCoils = CountByStatus(coils, CoilStatus.Rejected);
        decimal totalWeight = coils.Sum(coil => coil.Weight);
        decimal availableWeight = coils
            .Where(coil => coil.Status == CoilStatus.Available)
            .Sum(coil => coil.Weight);

        InventorySummaryDto inventory = new(
            totalMotherCoils,
            availableMotherCoils,
            reservedMotherCoils,
            holdMotherCoils,
            rejectedMotherCoils,
            totalWeight,
            availableWeight,
            BuildGradeWiseStock(coils),
            BuildSupplierWiseStock(coils),
            BuildRecentReceivedCoils(coils));

        ProductionSummaryDto production = new(0, 0, 0, 0, ComingSoon);
        SlittingJobMetricsDto slittingMetrics = BuildSlittingJobMetrics(jobs);
        SlittingSummaryDto slitting = new(0, jobs.Length, 0, BuildSlittingStatus(slittingMetrics));
        QualitySummaryDto quality = new(0, holdMotherCoils, rejectedMotherCoils, ComingSoon);
        ProcurementSummaryDto procurement = new(0, coils.Select(coil => coil.SupplierId).Distinct().Count(), 0, ComingSoon);
        DispatchSummaryDto dispatch = new(0, 0, 0, ComingSoon);
        AnalyticsSummaryDto analytics = new(BuildAnalyticsPlaceholders());

        return new OperationsDashboardDto(
            DashboardRole: GetDashboardRole(),
            GeneratedAt: DateTimeOffset.UtcNow,
            Kpis: BuildKpis(totalMotherCoils, slittingMetrics),
            Inventory: inventory,
            Production: production,
            Slitting: slitting,
            SlittingJobMetrics: slittingMetrics,
            ProductionQueue: BuildProductionQueue(jobs),
            Quality: quality,
            Procurement: procurement,
            Dispatch: dispatch,
            Analytics: analytics,
            QuickActions: BuildQuickActions(),
            RecentActivities: BuildRecentActivities(inventory.RecentReceivedCoils),
            Notifications: BuildNotifications(holdMotherCoils, rejectedMotherCoils));
    }

    private static int CountByStatus(IEnumerable<RawCoil> coils, CoilStatus status)
    {
        return coils.Count(coil => coil.Status == status);
    }

    private static IReadOnlyList<DashboardKpiDto> BuildKpis(int totalMotherCoils, SlittingJobMetricsDto slittingMetrics)
    {
        return
        [
            new("Mother Coils", totalMotherCoils.ToString("N0"), "inventory_2", "primary", "Live inventory"),
            new("Slit Coils", "0", "splitscreen", "neutral", ComingSoon),
            new("Finished Coils", "0", "task_alt", "neutral", ComingSoon),
            new("Waiting to Start", slittingMetrics.ReleasedJobs.ToString("N0"), "pending_actions", "warning", "Released slitting jobs"),
            new("Running Jobs", slittingMetrics.InProgressJobs.ToString("N0"), "precision_manufacturing", "success", "In progress slitting jobs"),
            new("Completed Today", slittingMetrics.CompletedToday.ToString("N0"), "task_alt", "primary", "Slitting jobs completed today"),
            new("Dispatches", "0", "local_shipping", "neutral", ComingSoon)
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
            new("Create Work Order", "assignment_add", null, false, ComingSoon),
            new("Create Slitting Job", "precision_manufacturing", null, false, ComingSoon),
            new("Print QR Labels", "qr_code_2", null, false, ComingSoon)
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
