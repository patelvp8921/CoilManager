using CoilManager.API.Configuration;
using CoilManager.Application;
using CoilManager.Infrastructure;
using CoilManager.Persistence;

namespace CoilManager.API.Extensions;

public static class ServiceCollectionExtensions
{
    public const string AllowAngularClientPolicy = "AllowAngularClient";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddCors(options =>
        {
            options.AddPolicy(AllowAngularClientPolicy, policy =>
            {
                string[] allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddPersistence(configuration);

        return services;
    }
}
