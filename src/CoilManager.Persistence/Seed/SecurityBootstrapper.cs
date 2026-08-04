using CoilManager.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoilManager.Persistence.Seed;

public static class SecurityBootstrapper
{
    public static async Task ProvisionFirstAdministratorAsync(IServiceProvider services, CancellationToken ct = default)
    {
        ApplicationDbContext db = services.GetRequiredService<ApplicationDbContext>();
        if (await db.Users.AnyAsync(ct)) return;
        IConfiguration config = services.GetRequiredService<IConfiguration>();
        string? email = config["BootstrapAdmin:Email"], password = config["BootstrapAdmin:Password"];
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SecurityBootstrapper");
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No Identity users exist. Configure BootstrapAdmin__Email and BootstrapAdmin__Password as deployment secrets to provision the first administrator.");
            return;
        }
        UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser user = new() { Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true,
            DisplayName = "System Administrator", IsActive = true, MustChangePassword = true };
        IdentityResult created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded) throw new InvalidOperationException("Bootstrap administrator could not be created: " + string.Join("; ", created.Errors.Select(x => x.Description)));
        IdentityResult assigned = await userManager.AddToRoleAsync(user, "Administrator");
        if (!assigned.Succeeded) throw new InvalidOperationException("Bootstrap administrator role assignment failed.");
        logger.LogInformation("The first administrator was securely provisioned for {Email}; password value was not logged.", email);
    }
}
