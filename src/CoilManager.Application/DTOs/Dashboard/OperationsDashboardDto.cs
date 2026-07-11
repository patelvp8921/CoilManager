namespace CoilManager.Application.DTOs.Dashboard;

public sealed record OperationsDashboardDto(
    string DashboardRole,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<DashboardKpiDto> Kpis,
    InventorySummaryDto Inventory,
    ProductionSummaryDto Production,
    SlittingSummaryDto Slitting,
    SlittingJobMetricsDto SlittingJobMetrics,
    IReadOnlyList<ProductionQueueItemDto> ProductionQueue,
    QualitySummaryDto Quality,
    ProcurementSummaryDto Procurement,
    DispatchSummaryDto Dispatch,
    AnalyticsSummaryDto Analytics,
    IReadOnlyList<QuickActionDto> QuickActions,
    IReadOnlyList<RecentActivityDto> RecentActivities,
    IReadOnlyList<NotificationDto> Notifications);

public sealed record DashboardKpiDto(
    string Label,
    string Value,
    string Icon,
    string Tone,
    string? Hint = null);

public sealed record InventorySummaryDto(
    int TotalMotherCoils,
    int AvailableMotherCoils,
    int ReservedMotherCoils,
    int HoldMotherCoils,
    int RejectedMotherCoils,
    decimal TotalWeight,
    decimal AvailableWeight,
    IReadOnlyList<InventoryBreakdownDto> GradeWiseStock,
    IReadOnlyList<InventoryBreakdownDto> SupplierWiseStock,
    IReadOnlyList<RecentMotherCoilDto> RecentReceivedCoils);

public sealed record InventoryBreakdownDto(
    string Name,
    int Count,
    decimal Weight);

public sealed record RecentMotherCoilDto(
    Guid Id,
    string CoilId,
    string Grade,
    string Supplier,
    decimal Weight,
    DateOnly ReceivedDate,
    string Status);

public sealed record ProductionSummaryDto(
    int WorkOrders,
    int FinishedCoils,
    decimal PlannedWeight,
    decimal ProducedWeight,
    string Status);

public sealed record SlittingSummaryDto(
    int SlitCoils,
    int SlittingJobs,
    decimal SlitWeight,
    string Status);

public sealed record SlittingJobMetricsDto(
    int DraftJobs,
    int ReleasedJobs,
    int InProgressJobs,
    int CompletedToday,
    int CancelledJobs,
    decimal AverageWaitingMinutes,
    decimal AverageProcessingMinutes);

public sealed record ProductionQueueItemDto(
    Guid SlittingJobId,
    string SlittingJobNo,
    string MotherCoilNumber,
    string Status,
    DateTimeOffset? ReleasedOn,
    DateTimeOffset? StartedOn,
    decimal WaitingMinutes,
    Guid? MachineId,
    string? Shift,
    string Route);

public sealed record QualitySummaryDto(
    int PendingQa,
    int HoldCoils,
    int RejectedCoils,
    string Status);

public sealed record ProcurementSummaryDto(
    int PendingReceipts,
    int Suppliers,
    decimal IncomingWeight,
    string Status);

public sealed record DispatchSummaryDto(
    int Dispatches,
    decimal DispatchWeight,
    int PendingDispatches,
    string Status);

public sealed record AnalyticsSummaryDto(
    IReadOnlyList<AnalyticsPlaceholderDto> Cards);

public sealed record AnalyticsPlaceholderDto(
    string Title,
    string Description,
    string Icon);

public sealed record QuickActionDto(
    string Label,
    string Icon,
    string? Route,
    bool Enabled,
    string? Badge = null);

public sealed record RecentActivityDto(
    string Title,
    string Description,
    DateTimeOffset Timestamp,
    string Icon,
    string Tone,
    string? Route = null);

public sealed record NotificationDto(
    string Title,
    string Message,
    string Severity,
    string Icon,
    string? Route = null);
