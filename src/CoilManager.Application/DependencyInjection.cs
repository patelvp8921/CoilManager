using FluentValidation;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Application.Services;
using CoilManager.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoilManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddAutoMapper(_ => { }, typeof(AssemblyReference).Assembly);
        services.AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly);
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
        services.AddScoped<IRawCoilService, RawCoilService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IOperationsDashboardService, OperationsDashboardService>();
        services.AddScoped<ISlittingJobService, SlittingJobService>();
        services.AddScoped<ISlitCoilService, SlitCoilService>();
        services.AddScoped<ISlitCoilLabelService, SlitCoilLabelService>();
        services.AddScoped<ICoilService, CoilService>();
        services.AddScoped<ICoilNumberingService, CoilNumberingService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.Configure<SlittingSettings>(configuration.GetSection(SlittingSettings.SectionName));
        services.Configure<LabelSettings>(configuration.GetSection(LabelSettings.SectionName));

        return services;
    }
}
