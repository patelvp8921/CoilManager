using CoilManager.Application.Abstractions.Persistence;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using CoilManager.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoilManager.Persistence.Repositories;
using CoilManager.Persistence.Services;
using CoilManager.Persistence.DemoData;
using CoilManager.Persistence.Identity;
using CoilManager.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace CoilManager.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        services.AddScoped<CoilManagerDbContext>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IRawCoilRepository, RawCoilRepository>();
        services.AddScoped<ISlittingJobRepository, SlittingJobRepository>();
        services.AddScoped<ISlitCoilRepository, SlitCoilRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<ISlitCoilLabelPrintHistoryRepository, SlitCoilLabelPrintHistoryRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<LaminationJobService>();
        services.AddScoped<ILaminationJobService>(p => p.GetRequiredService<LaminationJobService>());
        services.AddScoped<IMaterialAllocationService>(p => p.GetRequiredService<LaminationJobService>());
        services.AddScoped<IDemoDataSeeder, DemoDataSeeder>();
        services.AddDataProtection();
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = configuration.GetValue("Security:MaximumFailedLoginAttempts", 5);
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(configuration.GetValue("Security:LockoutMinutes", 15));
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        services.AddScoped<ISecurityPlatformService, SecurityPlatformService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
