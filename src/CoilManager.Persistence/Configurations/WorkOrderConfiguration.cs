using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoilManager.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> b)
    {
        b.ToTable("WorkOrders", "app"); b.HasKey(x => x.Id);
        b.Property(x => x.WorkOrderNumber).HasMaxLength(20).IsRequired(); b.HasIndex(x => x.WorkOrderNumber).IsUnique();
        b.HasIndex(x => x.Status); b.HasIndex(x => x.RequiredDate); b.HasIndex(x => new { x.WorkOrderType, x.ProductType });
        b.HasIndex(x => x.SourceType); b.HasIndex(x => x.SalesOrderId); b.HasIndex(x => x.SalesOrderLineId);
        b.HasIndex(x => x.CustomerId); b.HasIndex(x => x.ProductType); b.HasIndex(x => x.FulfilmentStrategy); b.HasIndex(x => x.ProductionRoute);
        b.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.FulfilmentStrategy).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ProductionRoute).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.QuantityUnit).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.SalesOrderNumber).HasMaxLength(30); b.Property(x => x.CustomerCode).HasMaxLength(30); b.Property(x => x.CustomerPONumber).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500); b.Property(x => x.GradeCode).HasMaxLength(50); b.Property(x => x.DrawingRevision).HasMaxLength(50);
        b.Property(x => x.OEMJobNumber).HasMaxLength(100); b.Property(x => x.TransformerRating).HasMaxLength(100); b.Property(x => x.Planner).HasMaxLength(100); b.Property(x => x.CancellationReason).HasMaxLength(500);
        foreach (var property in new[] { nameof(WorkOrder.Length), nameof(WorkOrder.PlanningRequiredQuantity), nameof(WorkOrder.PlannedInventoryQuantity), nameof(WorkOrder.PlannedProductionQuantity), nameof(WorkOrder.ReservedInventoryQuantity), nameof(WorkOrder.ProducedQuantity), nameof(WorkOrder.ReadyQuantity), nameof(WorkOrder.DispatchedQuantity) }) b.Property(property).HasPrecision(18, 3);
        b.Property(x => x.WorkOrderType).HasConversion<string>().HasMaxLength(30); b.Property(x => x.ProductType).HasConversion<string>().HasMaxLength(30); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.CustomerName).HasMaxLength(200); b.Property(x => x.SalesOrderReference).HasMaxLength(100);
        b.Property(x => x.Category).HasMaxLength(30).IsRequired(); b.Property(x => x.DrawingNumber).HasMaxLength(100); b.Property(x => x.Remarks).HasMaxLength(1000);
        b.Property(x => x.Thickness).HasPrecision(18,3); b.Property(x => x.CoreLossPerKg).HasPrecision(18,4); b.Property(x => x.RequiredWidth).HasPrecision(18,3); b.Property(x => x.RequiredWeight).HasPrecision(18,3);
        foreach (var name in new[] { nameof(WorkOrder.ReleasedBy), nameof(WorkOrder.StartedBy), nameof(WorkOrder.CompletedBy), nameof(WorkOrder.ClosedBy), nameof(WorkOrder.CancelledBy), nameof(WorkOrder.CreatedBy), nameof(WorkOrder.UpdatedBy) }) b.Property(name).HasMaxLength(100);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Grade).WithMany().HasForeignKey(x => x.GradeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SalesOrder).WithMany().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SalesOrderLine).WithMany().HasForeignKey(x => x.SalesOrderLineId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Operations).WithOne(x => x.WorkOrder).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Allocations).WithOne(x => x.WorkOrder).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class WorkOrderOperationConfiguration : IEntityTypeConfiguration<WorkOrderOperation>
{
    public void Configure(EntityTypeBuilder<WorkOrderOperation> b)
    {
        b.ToTable("WorkOrderOperations", "app"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.WorkOrderId, x.OperationType }).IsUnique();
        b.Property(x => x.OperationType).HasConversion<string>().HasMaxLength(30); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.RelatedDocumentNumber).HasMaxLength(50); b.Property(x => x.Remarks).HasMaxLength(500);
    }
}

public sealed class WorkOrderMaterialAllocationConfiguration : IEntityTypeConfiguration<WorkOrderMaterialAllocation>
{
    public void Configure(EntityTypeBuilder<WorkOrderMaterialAllocation> b)
    {
        b.ToTable("WorkOrderMaterialAllocations", "app"); b.HasKey(x => x.Id); b.HasIndex(x => x.WorkOrderId); b.HasIndex(x => x.MotherCoilId); b.HasIndex(x => x.SlitCoilId);
        b.Property(x => x.CoilType).HasConversion<string>().HasMaxLength(20); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30); b.Property(x => x.CoilNumber).HasMaxLength(60);
        b.Property(x => x.AllocatedWeight).HasPrecision(18,3); b.Property(x => x.IssuedWeight).HasPrecision(18,3); b.Property(x => x.ConsumedWeight).HasPrecision(18,3); b.Property(x => x.RemainingWeightAfterAllocation).HasPrecision(18,3);
        b.Property(x => x.ReservedBy).HasMaxLength(100); b.Property(x => x.ReleasedBy).HasMaxLength(100); b.Property(x => x.CreatedBy).HasMaxLength(100); b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasOne(x => x.MotherCoil).WithMany().HasForeignKey(x => x.MotherCoilId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SlitCoil).WithMany().HasForeignKey(x => x.SlitCoilId).OnDelete(DeleteBehavior.Restrict);
    }
}
