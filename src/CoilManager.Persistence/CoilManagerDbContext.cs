using CoilManager.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence;

public sealed class CoilManagerDbContext(DbContextOptions<CoilManagerDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
