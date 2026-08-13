using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.WorkOrders;

public sealed record WorkOrderQueryRequest(int Page = 1, int PageSize = 25, string? Search = null,
    WorkOrderType? WorkOrderType = null, WorkOrderProductType? ProductType = null, WorkOrderStatus? Status = null,
    int? Priority = null, DateOnly? DateFrom = null, DateOnly? DateTo = null)
{
    public int NormalizedPage => Math.Max(Page, 1);
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, 100);
}

public record CreateWorkOrderRequest(WorkOrderType WorkOrderType, WorkOrderProductType ProductType,
    string? CustomerName, string? SalesOrderReference, DateOnly WorkOrderDate, DateOnly? RequiredDate,
    int Priority, Guid? GradeId, decimal Thickness, string Category, decimal CoreLossPerKg,
    string? DrawingNumber, decimal? RequiredWidth, decimal? RequiredWeight, int? RequiredQuantity, string? Remarks,
    FulfilmentStrategy? FulfilmentStrategy = null, decimal? PlannedInventoryQuantity = null,
    decimal? PlannedProductionQuantity = null, ProductionRoute? ProductionRoute = null,
    Guid? SalesOrderId = null, Guid? SalesOrderLineId = null);

public sealed record UpdateWorkOrderRequest(WorkOrderType WorkOrderType, WorkOrderProductType ProductType,
    string? CustomerName, string? SalesOrderReference, DateOnly WorkOrderDate, DateOnly? RequiredDate,
    int Priority, Guid? GradeId, decimal Thickness, string Category, decimal CoreLossPerKg,
    string? DrawingNumber, decimal? RequiredWidth, decimal? RequiredWeight, int? RequiredQuantity, string? Remarks,
    FulfilmentStrategy? FulfilmentStrategy, decimal? PlannedInventoryQuantity, decimal? PlannedProductionQuantity,
    ProductionRoute? ProductionRoute, string RowVersion, Guid? SalesOrderId = null, Guid? SalesOrderLineId = null) : CreateWorkOrderRequest(WorkOrderType, ProductType, CustomerName, SalesOrderReference,
        WorkOrderDate, RequiredDate, Priority, GradeId, Thickness, Category, CoreLossPerKg, DrawingNumber,
        RequiredWidth, RequiredWeight, RequiredQuantity, Remarks, FulfilmentStrategy, PlannedInventoryQuantity, PlannedProductionQuantity, ProductionRoute, SalesOrderId, SalesOrderLineId);

public sealed record WorkOrderListItemDto(Guid Id, string WorkOrderNumber, WorkOrderType WorkOrderType,
    WorkOrderProductType ProductType, string? CustomerName, string? SalesOrderReference, DateOnly? RequiredDate,
    int Priority, WorkOrderStatus Status, decimal Progress, DateTimeOffset CreatedOn,
    decimal RequiredQuantity, QuantityUnit QuantityUnit, decimal ReservedQuantity,
    decimal RemainingAllocationQuantity, decimal AllocationPercentage,
    Guid? LinkedLaminationJobId, string? LinkedLaminationJobNumber, LaminationJobStatus? LinkedLaminationJobStatus,
    string NextActionType, string NextActionLabel, string? NextActionRoute,
    bool HasInventoryShortage = false, decimal? ShortageQuantity = null);

public sealed record WorkOrderOperationDto(Guid Id, WorkOrderOperationType OperationType, int Sequence,
    bool IsRequired, WorkOrderOperationStatus Status, Guid? RelatedDocumentId, string? RelatedDocumentNumber,
    DateTimeOffset? StartedOn, DateTimeOffset? CompletedOn, string? Remarks);

public sealed record WorkOrderMaterialAllocationDto(Guid Id, CoilType CoilType, Guid CoilId, string CoilNumber,
    decimal AllocatedWeight, decimal? IssuedWeight, decimal? ConsumedWeight, decimal RemainingWeightAfterAllocation,
    AllocationStatus Status, DateTimeOffset ReservedOn, string ReservedBy, DateTimeOffset? ReleasedOn,
    string? ReleasedBy, string? Remarks);

public sealed record WorkOrderDetailsDto(Guid Id, string WorkOrderNumber, WorkOrderType WorkOrderType,
    WorkOrderProductType ProductType, Guid? SalesOrderId, Guid? SalesOrderLineId,
    string? CustomerName, string? SalesOrderReference, DateOnly WorkOrderDate,
    DateOnly? RequiredDate, int Priority, Guid? GradeId, string? Grade, decimal Thickness, string Category,
    decimal CoreLossPerKg, string? DrawingNumber, decimal? RequiredWidth, decimal? RequiredWeight,
    int? RequiredQuantity, WorkOrderStatus Status, string? Remarks, string? ReleasedBy, DateTimeOffset? ReleasedOn,
    string? StartedBy, DateTimeOffset? StartedOn, string? CompletedBy, DateTimeOffset? CompletedOn,
    string? ClosedBy, DateTimeOffset? ClosedOn, string? CancelledBy, DateTimeOffset? CancelledOn,
    string? CreatedBy, DateTimeOffset CreatedOn, string? ModifiedBy, DateTimeOffset? ModifiedOn, string RowVersion,
    decimal AllocatedWeight, decimal IssuedWeight, decimal ConsumedWeight,
    IReadOnlyList<WorkOrderOperationDto> Operations, IReadOnlyList<WorkOrderMaterialAllocationDto> Allocations,
    IReadOnlyList<WorkOrderSlittingJobDto> RelatedSlittingJobs, FulfilmentStrategy FulfilmentStrategy,
    decimal PlanningRequiredQuantity, decimal PlannedInventoryQuantity, decimal PlannedProductionQuantity,
    decimal ReservedInventoryQuantity, decimal ProducedQuantity, decimal ReadyQuantity, decimal DispatchedQuantity,
    decimal UnplannedQuantity, decimal CoveragePercentage, ProductionRoute ProductionRoute, WorkOrderLaminationJobDto? LinkedLaminationJob);

public sealed record CreateMaterialAllocationRequest(CoilType CoilType, Guid CoilId, decimal AllocatedWeight, string? Remarks);
public sealed record ReleaseMaterialAllocationRequest(string? Remarks);
public sealed record SetSlittingRequirementRequest(bool IsRequired, string? Remarks);

public sealed record AvailableCoilDto(Guid Id, CoilType CoilType, string CoilNumber, string? MotherCoilNumber,
    string? Grade, decimal Thickness, decimal Width, decimal CurrentWeight, decimal ReservedWeight,
    decimal AvailableWeight, CoilStatus Status);

public sealed record WorkOrderSlittingJobDto(Guid Id, string SlittingJobNumber, SlittingJobStatus Status);
public sealed record WorkOrderLaminationJobDto(Guid Id, string LaminationJobNumber, string? DrawingNumber,
    decimal RequiredQuantity, LaminationJobStatus Status, decimal MaterialAllocationPercentage,
    DateTimeOffset CreatedOn, DateTimeOffset? ReleasedOn, DateTimeOffset? CompletedOn);

public sealed record WorkOrderMetricsDto(int Draft, int Released, int InProduction, int CompletedToday, int Completed,
    int Overdue, int CustomerWorkOrders, int InventoryProductionWorkOrders, IReadOnlyList<WorkOrderQueueItemDto> Queue);
public sealed record WorkOrderQueueItemDto(Guid Id, string WorkOrderNumber, WorkOrderProductType ProductType,
    DateOnly? RequiredDate, int Priority, WorkOrderStatus Status, decimal AllocationPercentage, decimal OperationProgress);

public sealed record WorkOrderFulfilmentDto(decimal RequiredQuantity, QuantityUnit QuantityUnit,
    decimal PlannedInventoryQuantity, decimal ReservedInventoryQuantity, decimal PlannedProductionQuantity,
    decimal ProducedQuantity, decimal ReadyQuantity, decimal DispatchedQuantity, decimal ShortageQuantity,
    decimal CoveragePercentage, decimal ReadyPercentage, WorkOrderStatus Status);

public sealed record WorkOrderNextActionDto(string Key, string Type, string Title, string Description,
    decimal? PlannedQuantity, decimal? CompletedQuantity, decimal? RemainingQuantity, QuantityUnit? QuantityUnit,
    string Status, string Severity, bool IsEnabled, string? DisabledReason, string? ActionLabel,
    string? Route, string? ActionCode, int Order);

public sealed record CreateWorkOrderInventoryAllocationRequest(CoilType InventoryType, Guid InventoryId,
    decimal Quantity, string? Remarks, string RowVersion);
public sealed record UpdateWorkOrderInventoryAllocationRequest(decimal Quantity, string? Remarks, string RowVersion);
