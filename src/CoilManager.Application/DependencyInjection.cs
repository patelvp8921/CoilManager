using FluentValidation;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoilManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAutoMapper(_ => { }, typeof(AssemblyReference).Assembly);
        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);
        services.AddScoped<IRawCoilService, RawCoilService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();
        services.AddScoped<ISlittingJobService, SlittingJobService>();

        return services;
    }
}
