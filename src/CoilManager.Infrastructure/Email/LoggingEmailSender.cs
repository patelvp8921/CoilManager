using CoilManager.Application.Security;
using Microsoft.Extensions.Logging;

namespace CoilManager.Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken ct)
    {
        logger.LogInformation("Email queued for {Recipient} with subject {Subject}. Body is intentionally excluded from logs.", recipient, subject);
        return Task.CompletedTask;
    }
}
