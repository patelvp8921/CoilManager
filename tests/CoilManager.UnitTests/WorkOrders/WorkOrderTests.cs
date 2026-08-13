using CoilManager.Application.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using CoilManager.Shared.Exceptions;

namespace CoilManager.UnitTests.WorkOrders;

public sealed class WorkOrderTests
{
    [Fact] public void Number_format_is_stable() => Assert.Equal("WO/2026/00001", WorkOrderService.FormatNumber(2026, 1));

    [Theory]
    [InlineData(WorkOrderProductType.MotherCoil, WorkOrderOperationStatus.NotRequired, WorkOrderOperationStatus.NotRequired)]
    [InlineData(WorkOrderProductType.SlitCoil, WorkOrderOperationStatus.NotRequired, WorkOrderOperationStatus.NotRequired)]
    [InlineData(WorkOrderProductType.Lamination, WorkOrderOperationStatus.NotRequired, WorkOrderOperationStatus.Pending)]
    public void Product_routing_is_configured(WorkOrderProductType product, WorkOrderOperationStatus slitting, WorkOrderOperationStatus lamination)
    {
        WorkOrder wo = Create(product);
        Assert.Equal(slitting, wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Slitting).Status);
        Assert.Equal(lamination, wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Lamination).Status);
        Assert.Equal(WorkOrderOperationStatus.Pending, wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Dispatch).Status);
    }

    [Fact] public void Draft_can_be_released()
    {
        WorkOrder wo = Create(); wo.Release("planner", DateTimeOffset.UtcNow); Assert.Equal(WorkOrderStatus.Released, wo.Status);
    }

    [Fact] public void Invalid_transition_is_rejected()
    {
        WorkOrder wo = Create(); Assert.Throws<InvalidOperationException>(() => wo.Start("planner", DateTimeOffset.UtcNow));
    }

    [Fact] public void Work_order_cannot_complete_with_pending_required_operations()
    {
        WorkOrder wo = Create(); wo.Release("planner", DateTimeOffset.UtcNow); wo.Start("operator", DateTimeOffset.UtcNow);
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => wo.Complete("operator", DateTimeOffset.UtcNow)); Assert.Contains("required operations are pending", error.Message);
    }

    [Fact] public void Lamination_slitting_is_not_a_work_order_operation_in_the_mvp()
    {
        WorkOrder wo = Create(WorkOrderProductType.Lamination);
        WorkOrderOperation operation = wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Slitting);
        Assert.Equal(WorkOrderOperationStatus.NotRequired, operation.Status); Assert.False(operation.IsRequired);
    }

    [Fact] public void Allocation_supports_partial_weight_and_release()
    {
        var allocation = new WorkOrderMaterialAllocation(Guid.NewGuid(), CoilType.MotherCoil, Guid.NewGuid(), "MC-001", 250, 750, DateTimeOffset.UtcNow, "planner", null);
        Assert.Equal(250, allocation.AllocatedWeight); Assert.Equal(750, allocation.RemainingWeightAfterAllocation); Assert.True(allocation.IsActive);
        allocation.Release(DateTimeOffset.UtcNow, "planner"); Assert.Equal(AllocationStatus.Released, allocation.Status); Assert.False(allocation.IsActive);
    }

    [Fact] public void Allocation_weight_must_be_positive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WorkOrderMaterialAllocation(Guid.NewGuid(), CoilType.MotherCoil, Guid.NewGuid(), "MC-001", 0, 1000, DateTimeOffset.UtcNow, "planner", null));
    }

    [Fact] public void Manual_allocation_succeeds_within_available_weight()
        => Assert.Equal(600, MaterialAllocationCalculator.ValidateAndCalculateRemaining(1000, 100, 300));

    [Fact] public void Allocation_above_available_weight_fails()
        => Assert.Throws<BusinessRuleException>(() => MaterialAllocationCalculator.ValidateAndCalculateRemaining(1000, 800, 201));

    [Fact] public void Multiple_and_partial_allocations_have_correct_reserved_and_available_weight()
    {
        decimal current = 1000; decimal first = 250; decimal second = 300;
        Assert.Equal(450, MaterialAllocationCalculator.AvailableWeight(current, first + second));
    }

    [Fact] public void Released_allocation_restores_availability_and_prevents_double_allocation_while_active()
    {
        decimal current = 500; var allocation = new WorkOrderMaterialAllocation(Guid.NewGuid(), CoilType.SlitCoil, Guid.NewGuid(), "SC-01", 400, 100, DateTimeOffset.UtcNow, "planner", null);
        Assert.Throws<BusinessRuleException>(() => MaterialAllocationCalculator.ValidateAndCalculateRemaining(current, allocation.AllocatedWeight, 101));
        allocation.Release(DateTimeOffset.UtcNow, "planner");
        Assert.Equal(500, MaterialAllocationCalculator.AvailableWeight(current, allocation.IsActive ? allocation.AllocatedWeight : 0));
    }

    [Fact] public void Cancelling_marks_unfinished_required_operations_cancelled()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); wo.Cancel("planner", DateTimeOffset.UtcNow);
        Assert.Equal(WorkOrderStatus.Cancelled, wo.Status);
        Assert.All(wo.Operations.Where(x => x.IsRequired), x => Assert.Equal(WorkOrderOperationStatus.Cancelled, x.Status));
    }

    [Fact] public void Cancelled_work_order_releases_active_allocations()
    {
        WorkOrder wo = Create(); var allocation = new WorkOrderMaterialAllocation(wo.Id, CoilType.MotherCoil, Guid.NewGuid(), "MC-01", 100, 900, DateTimeOffset.UtcNow, "planner", null);
        wo.AddAllocation(allocation); wo.Cancel("planner", DateTimeOffset.UtcNow); Assert.Equal(AllocationStatus.Released, allocation.Status);
    }

    [Fact] public void Dashboard_work_order_metrics_are_live()
    {
        WorkOrder draft = Create(); WorkOrder released = Create(); released.Release("planner", DateTimeOffset.UtcNow);
        var metrics = OperationsDashboardService.BuildWorkOrderMetrics([draft, released]);
        Assert.Equal(1, metrics.Draft); Assert.Equal(1, metrics.Released); Assert.Single(metrics.Queue);
    }

    [Fact] public void Standalone_slitting_job_remains_inventory_production()
    {
        var job = new SlittingJob("AE/S/2026/0001", DateOnly.FromDateTime(DateTime.Today), null, Guid.NewGuid(), null, null, .2m, 5, 5, null);
        Assert.Equal(SlittingJobProductionType.Inventory, job.ProductionType); Assert.Null(job.WorkOrderId);
    }

    [Fact] public void Slit_coil_work_order_does_not_activate_slitting_when_a_job_changes()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); WorkOrderOperation op = wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Slitting);
        op.SynchronizeSlittingJob(SlittingJobStatus.Released, DateTimeOffset.UtcNow);
        Assert.Equal(WorkOrderOperationStatus.NotRequired, op.Status);
    }

    [Theory]
    [InlineData(WorkOrderProductType.MotherCoil)]
    [InlineData(WorkOrderProductType.SlitCoil)]
    public void Inventory_product_derives_full_existing_inventory_plan(WorkOrderProductType product)
    {
        WorkOrder wo = Create(product);
        Assert.Equal(FulfilmentStrategy.ExistingInventoryOnly, wo.FulfilmentStrategy);
        Assert.Equal(1000, wo.PlannedInventoryQuantity);
        Assert.Equal(0, wo.PlannedProductionQuantity);
        Assert.Equal(ProductionRoute.None, wo.ProductionRoute);
    }

    [Fact] public void Slit_coil_cannot_be_switched_to_mixed_or_slitting_production_in_the_mvp()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil);
        wo.ConfigureFulfilment(FulfilmentStrategy.InventoryAndProduction, 400, 600, ProductionRoute.SlittingOnly);
        Assert.Equal(FulfilmentStrategy.ExistingInventoryOnly, wo.FulfilmentStrategy);
        Assert.Equal(1000, wo.PlannedInventoryQuantity);
        Assert.Equal(0, wo.PlannedProductionQuantity);
        Assert.Equal(ProductionRoute.None, wo.ProductionRoute);
    }

    [Fact] public void Lamination_derives_lamination_only_production_plan()
    {
        WorkOrder wo = Create(WorkOrderProductType.Lamination);
        Assert.Equal(FulfilmentStrategy.ProductionOnly, wo.FulfilmentStrategy);
        Assert.Equal(0, wo.PlannedInventoryQuantity);
        Assert.Equal(1000, wo.PlannedProductionQuantity);
        Assert.Equal(ProductionRoute.LaminationOnly, wo.ProductionRoute);
    }

    [Fact] public void Cancellation_reason_is_required()
    {
        WorkOrder wo = Create();
        Assert.Throws<ArgumentException>(() => wo.Cancel("", "planner", DateTimeOffset.UtcNow));
    }

    [Fact] public void Fulfilment_recalculation_derives_operational_status()
    {
        WorkOrder wo = Create(); wo.Release("planner", DateTimeOffset.UtcNow);
        wo.RecalculateFulfilment(0, 0, true); Assert.Equal(WorkOrderStatus.InFulfilment, wo.Status);
        wo.RecalculateFulfilment(250, 0, true); Assert.Equal(WorkOrderStatus.PartiallyReady, wo.Status); Assert.Equal(250, wo.ReadyQuantity);
        wo.RecalculateFulfilment(1000, 0, true); Assert.Equal(WorkOrderStatus.Ready, wo.Status); Assert.Equal(0, wo.UnplannedQuantity);
    }

    [Fact] public void Planned_production_does_not_count_as_ready()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); wo.Release("planner", DateTimeOffset.UtcNow);
        wo.RecalculateFulfilment(0, 0, true);
        Assert.Equal(0, wo.ReadyQuantity); Assert.Equal(WorkOrderStatus.InFulfilment, wo.Status);
    }

    [Fact] public void Ready_work_order_cannot_be_completed_without_dispatch()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); wo.Release("planner", DateTimeOffset.UtcNow);
        wo.RecalculateFulfilment(1000, 0, true);
        Assert.Throws<InvalidOperationException>(() => wo.Complete("planner", DateTimeOffset.UtcNow));
        Assert.Equal(WorkOrderStatus.Ready, wo.Status);
    }

    [Fact] public void Partial_dispatch_moves_ready_work_order_to_partially_dispatched()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); wo.Release("planner", DateTimeOffset.UtcNow);
        wo.RecalculateFulfilment(1000, 0, true); wo.RecordDispatch(400, "dispatcher", DateTimeOffset.UtcNow);
        Assert.Equal(WorkOrderStatus.PartiallyDispatched, wo.Status);
        Assert.Equal(400, wo.DispatchedQuantity);
    }

    [Fact] public void Final_dispatch_completes_work_order_and_dispatch_operation()
    {
        WorkOrder wo = Create(WorkOrderProductType.SlitCoil); wo.Release("planner", DateTimeOffset.UtcNow);
        wo.RecalculateFulfilment(1000, 0, true); wo.RecordDispatch(400, "dispatcher", DateTimeOffset.UtcNow);
        wo.RecordDispatch(1000, "dispatcher", DateTimeOffset.UtcNow);
        Assert.Equal(WorkOrderStatus.Completed, wo.Status);
        Assert.Equal(WorkOrderOperationStatus.Completed, wo.Operations.Single(x => x.OperationType == WorkOrderOperationType.Dispatch).Status);
    }

    [Fact] public void Reserved_allocation_can_be_adjusted_and_releases_the_difference()
    {
        var allocation = new WorkOrderMaterialAllocation(Guid.NewGuid(), CoilType.MotherCoil, Guid.NewGuid(), "MC-ADJ", 300, 200, DateTimeOffset.UtcNow, "planner", null);
        allocation.Adjust(220, 280, "Reduced requirement");
        Assert.Equal(220, allocation.AllocatedWeight); Assert.Equal(280, allocation.RemainingWeightAfterAllocation); Assert.True(allocation.IsActive);
    }
    private static WorkOrder Create(WorkOrderProductType product = WorkOrderProductType.MotherCoil) => new("WO-2026-00001", WorkOrderType.InventoryProduction, product, null, null,
        new DateOnly(2026, 7, 13), new DateOnly(2026, 7, 20), 3, null, .23m, "CRGO", .9m, null, 1000, 1000, null, null);
}
