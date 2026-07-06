namespace CoilManager.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedSuppliersAsync(dbContext, cancellationToken);
        await SeedManufacturersAsync(dbContext, cancellationToken);
        await SeedGradesAsync(dbContext, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSuppliersAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Suppliers, cancellationToken))
        {
            return;
        }

        await dbContext.Suppliers.AddRangeAsync(
            [
                new Domain.Entities.Supplier("Prime Steel Suppliers", "PSS"),
                new Domain.Entities.Supplier("National Coil Traders", "NCT"),
                new Domain.Entities.Supplier("Apex Metals", "APX")
            ],
            cancellationToken);
    }

    private static async Task SeedManufacturersAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Manufacturers, cancellationToken))
        {
            return;
        }

        await dbContext.Manufacturers.AddRangeAsync(
            [
                new Domain.Entities.Manufacturer("Tata Steel", "TATA"),
                new Domain.Entities.Manufacturer("JSW Steel", "JSW"),
                new Domain.Entities.Manufacturer("SAIL", "SAIL")
            ],
            cancellationToken);
    }

    private static async Task SeedGradesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Grades, cancellationToken))
        {
            return;
        }

        await dbContext.Grades.AddRangeAsync(
            [
                new Domain.Entities.Grade("23HP85", "23HP85"),
                new Domain.Entities.Grade("23HP90", "23HP90"),
                new Domain.Entities.Grade("23HP95", "23HP95")
            ],
            cancellationToken);
    }
}
