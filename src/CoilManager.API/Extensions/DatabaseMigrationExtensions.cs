using CoilManager.Persistence;
using CoilManager.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Extensions;

public static class DatabaseMigrationExtensions
{
    private const string ApplyMigrationsConfigurationKey = "Database:ApplyMigrationsOnStartup";

    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        bool applyMigrations = app.Configuration.GetValue<bool>(ApplyMigrationsConfigurationKey);
        if (!applyMigrations)
        {
            return;
        }

        using IServiceScope scope = app.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext);
    }
}
