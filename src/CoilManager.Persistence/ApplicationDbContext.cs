using CoilManager.Application.Abstractions.Persistence;
using CoilManager.Domain.Common;
using CoilManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<RawCoil> RawCoils => Set<RawCoil>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityStateRules();

        return base.SaveChangesAsync(cancellationToken);
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
