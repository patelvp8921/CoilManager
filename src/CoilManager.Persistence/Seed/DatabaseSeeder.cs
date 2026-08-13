using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await SeedSuppliersAsync(dbContext, cancellationToken);
        await SeedManufacturersAsync(dbContext, cancellationToken);
        await SeedGradesAsync(dbContext, cancellationToken);
        await SeedSecurityAsync(dbContext, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSecurityAsync(ApplicationDbContext db, CancellationToken ct)
    {
        string[] names =
        [
            "Dashboard.View", "Inventory.MotherCoils.View", "Inventory.MotherCoils.Create", "Inventory.MotherCoils.Edit", "Inventory.MotherCoils.Delete",
            "Inventory.SlitCoils.View", "Inventory.SlitCoils.Create", "Inventory.SlitCoils.Edit", "Inventory.SlitCoils.Delete",
            "Production.Slitting.View", "Production.Slitting.Create", "Production.Slitting.Edit", "Production.Slitting.Delete",
            "Production.Lamination.View", "Production.Lamination.Create", "Production.Lamination.Edit", "Production.Lamination.Delete",
            "Dispatch.View", "Dispatch.Create", "Dispatch.Edit", "Dispatch.Confirm", "Dispatch.Cancel", "Dispatch.PrintPackingSlip", "Dispatch.Delete", "Reports.View", "Reports.Export",
            "Sales.View", "Sales.Create", "Sales.Edit", "Sales.Delete",
            "Customers.View", "Customers.Create", "Customers.Edit", "Customers.Activate",
            "SalesOrders.View", "SalesOrders.Create", "SalesOrders.Edit", "SalesOrders.Confirm", "SalesOrders.Hold", "SalesOrders.Cancel",
            "WorkOrders.View", "WorkOrders.Create", "WorkOrders.Edit", "WorkOrders.Release", "WorkOrders.Cancel", "WorkOrders.ManageAllocation", "WorkOrders.CreateProductionJob",
            "Administration.Users.View", "Administration.Users.Manage",
            "Administration.Roles.View", "Administration.Roles.Manage", "Administration.Audit.View", "Administration.Company.Manage"
        ];
        HashSet<string> existing = new(await db.Permissions.Select(x => x.Name).ToArrayAsync(ct));
        foreach (string name in names.Where(x => !existing.Contains(x))) db.Permissions.Add(new Identity.Permission { Name = name, Module = name.Split('.')[0] });
        (string Name, string Description)[] roleSeeds =
        [
            ("Administrator", "Full ERP administration"), ("Production Manager", "Production oversight"),
            ("Production Planner", "Production planning"), ("Stores", "Inventory operations"), ("Dispatch", "Dispatch operations"),
            ("Sales", "Sales operations"), ("Operator", "Shop-floor operations"), ("Viewer", "Read-only access")
        ];
        foreach ((string name, string description) in roleSeeds)
            if (!await db.Roles.AnyAsync(x => x.NormalizedName == name.ToUpper(), ct))
                db.Roles.Add(new Identity.ApplicationRole { Id = Guid.NewGuid(), Name = name, NormalizedName = name.ToUpper(), Description = description, IsSystem = true, ConcurrencyStamp = Guid.NewGuid().ToString() });
        await db.SaveChangesAsync(ct);
        Identity.ApplicationRole admin = await db.Roles.SingleAsync(x => x.NormalizedName == "ADMINISTRATOR", ct);
        Guid[] permissionIds = await db.Permissions.Select(x => x.Id).ToArrayAsync(ct);
        HashSet<Guid> assigned = new(await db.RolePermissions.Where(x => x.RoleId == admin.Id).Select(x => x.PermissionId).ToArrayAsync(ct));
        db.RolePermissions.AddRange(permissionIds.Where(x => !assigned.Contains(x)).Select(x => new Identity.RolePermission { RoleId = admin.Id, PermissionId = x }));
        if (!await db.CompanyProfiles.AnyAsync(ct)) db.CompanyProfiles.Add(new Identity.CompanyProfile());
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
        Domain.Entities.Grade[] grades =
        [
            new("23HP85D", 0.23m, 0.85m),
            new("23HP90D", 0.23m, 0.90m),
            new("23HP95D", 0.23m, 0.95m)
        ];

        foreach (Domain.Entities.Grade seedGrade in grades)
        {
            Domain.Entities.Grade? existing = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Grades,
                grade => grade.Code == seedGrade.Code,
                cancellationToken);

            if (existing is null)
            {
                await dbContext.Grades.AddAsync(seedGrade, cancellationToken);
            }
            else
            {
                existing.Update(seedGrade.Code, seedGrade.ThicknessMm, seedGrade.CoreLossPerKg, existing.IsActive);
            }
        }
    }
}
