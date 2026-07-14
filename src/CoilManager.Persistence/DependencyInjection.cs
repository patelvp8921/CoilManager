using CoilManager.Application.Abstractions.Persistence;
using CoilManager.Application.Interfaces.Persistence;
using CoilManager.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoilManager.Persistence.Repositories;

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

        return services;
    }
}
