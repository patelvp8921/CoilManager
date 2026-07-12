using System.Text;
using CoilManager.API.Configuration;
using CoilManager.Application;
using CoilManager.Infrastructure;
using CoilManager.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        JwtSettings jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>() ?? new JwtSettings();

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

        string secretKey = jwtSettings.EffectiveSecretKey;
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JwtSettings:SecretKey is required.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ProductionUser", policy => policy.RequireRole("Admin", "Production"));
            options.AddPolicy("SlitCoilLabelPrint", policy => policy.RequireRole("Admin", "Production", "Stores"));
            options.AddPolicy("SlitCoilLabelReprint", policy => policy.RequireRole("Admin", "Production", "Stores"));
            options.AddPolicy("SlitCoilLabelVersionIncrement", policy => policy.RequireRole("Admin", "Production"));
            options.AddPolicy("StoresUser", policy => policy.RequireRole("Admin", "Stores"));
            options.AddPolicy("QAUser", policy => policy.RequireRole("Admin", "QA"));
            options.AddPolicy("DispatchUser", policy => policy.RequireRole("Admin", "Dispatch"));
            options.AddPolicy("ManagementUser", policy => policy.RequireRole("Admin", "Management"));
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication(configuration)
            .AddInfrastructure(configuration)
            .AddPersistence(configuration);

        return services;
    }
}
