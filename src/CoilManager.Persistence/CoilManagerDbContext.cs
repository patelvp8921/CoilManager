using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence;

public sealed class CoilManagerDbContext(DbContextOptions<CoilManagerDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoilManagerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
