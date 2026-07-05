namespace CoilManager.Persistence.Seed;

public static class DatabaseSeeder
{
    public static Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return Task.CompletedTask;
    }
}
