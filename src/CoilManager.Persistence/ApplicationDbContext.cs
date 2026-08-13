using System.Linq.Expressions;
using CoilManager.Application.Abstractions.Persistence;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CoilManager.Persistence.Identity;

namespace CoilManager.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
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
    public DbSet<User> LegacyUsers => Set<User>();
    public DbSet<Role> LegacyRoles => Set<Role>();
    public DbSet<UserRole> LegacyUserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<LoginOtp> LoginOtps => Set<LoginOtp>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderOperation> WorkOrderOperations => Set<WorkOrderOperation>();
    public DbSet<WorkOrderMaterialAllocation> WorkOrderMaterialAllocations => Set<WorkOrderMaterialAllocation>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<DispatchPackage> DispatchPackages => Set<DispatchPackage>();
    public DbSet<DispatchInventorySource> DispatchInventorySources => Set<DispatchInventorySource>();
    public DbSet<LaminationJob> LaminationJobs => Set<LaminationJob>();
    public DbSet<LaminationJobStep> LaminationJobSteps => Set<LaminationJobStep>();
    public DbSet<LaminationJobPlate> LaminationJobPlates => Set<LaminationJobPlate>();
    public DbSet<LaminationPlateDimension> LaminationPlateDimensions => Set<LaminationPlateDimension>();
    public DbSet<LaminationJobMaterialAllocation> LaminationJobMaterialAllocations => Set<LaminationJobMaterialAllocation>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
        ConfigureSecurity(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    private static void ConfigureSecurity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("IdentityUsers", "auth");
        builder.Entity<ApplicationRole>().ToTable("IdentityRoles", "auth");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("IdentityUserRoles", "auth");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("IdentityUserClaims", "auth");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("IdentityUserLogins", "auth");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("IdentityRoleClaims", "auth");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("IdentityUserTokens", "auth");
        builder.Entity<Permission>().ToTable("Permissions", "auth").HasIndex(x => x.Name).IsUnique();
        builder.Entity<RolePermission>().ToTable("RolePermissions", "auth").HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Entity<LoginOtp>().ToTable("LoginOtps", "auth").HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        builder.Entity<UserSession>().ToTable("UserSessions", "auth").HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.Entity<AuditLog>().ToTable("AuditLogs", "audit").HasIndex(x => x.TimestampUtc);
        builder.Entity<CompanyProfile>().ToTable("CompanyProfile", "config");
        builder.Entity<ApplicationUser>().Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
        builder.Entity<Permission>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Entity<LoginOtp>().Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
        builder.Entity<UserSession>().Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
        builder.Entity<AuditLog>().Property(x => x.Details).HasMaxLength(4000);
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
