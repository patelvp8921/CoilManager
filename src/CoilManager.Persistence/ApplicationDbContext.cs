using System.Linq.Expressions;
using CoilManager.Application.Abstractions.Persistence;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<RawCoil> RawCoils => Set<RawCoil>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<SlittingJob> SlittingJobs => Set<SlittingJob>();
    public DbSet<SlittingJobItem> SlittingJobItems => Set<SlittingJobItem>();
    public DbSet<SlitCoil> SlitCoils => Set<SlitCoil>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<SlitCoilLabelPrintHistory> SlitCoilLabelPrintHistories => Set<SlitCoilLabelPrintHistory>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderOperation> WorkOrderOperations => Set<WorkOrderOperation>();
    public DbSet<WorkOrderMaterialAllocation> WorkOrderMaterialAllocations => Set<WorkOrderMaterialAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyEntityStateRules();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityStateRules();

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            Type clrType = entityType.ClrType;
            if (!typeof(SoftDeletableEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            ParameterExpression parameter = Expression.Parameter(clrType, "entity");
            MemberExpression property = Expression.Property(parameter, nameof(SoftDeletableEntity.IsDeleted));
            BinaryExpression condition = Expression.Equal(property, Expression.Constant(false));
            LambdaExpression lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    private void ApplyEntityStateRules()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.SetCreatedAudit(userId: null, now);
                }

                if (entry.State == EntityState.Modified)
                {
                    auditableEntity.SetUpdatedAudit(userId: null, now);
                }
            }

            if (entry is { State: EntityState.Deleted, Entity: SoftDeletableEntity softDeletableEntity })
            {
                entry.State = EntityState.Modified;
                softDeletableEntity.MarkDeleted(userId: null, now);
            }
        }
    }
}
