using CoilManager.Application.Interfaces.Auth;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Infrastructure.Audit;
using CoilManager.Infrastructure.Auth;
using CoilManager.Infrastructure.Services;
using CoilManager.Application.Security;
using CoilManager.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoilManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISecurityTokenService, SecurityTokenService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
