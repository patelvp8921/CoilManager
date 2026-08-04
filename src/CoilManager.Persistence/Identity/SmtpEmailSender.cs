using System.Net;
using System.Net.Mail;
using CoilManager.Application.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.Persistence.Identity;

public sealed class SmtpEmailSender(ApplicationDbContext db, IDataProtectionProvider protection) : IEmailSender
{
    private readonly IDataProtector _protector = protection.CreateProtector("CoilManager.Company.Smtp.v1");
    public async Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken ct)
    {
        CompanyProfile company = await db.CompanyProfiles.AsNoTracking().SingleOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Company profile is not configured.");
        if (string.IsNullOrWhiteSpace(company.SmtpHost) || !company.SmtpPort.HasValue || string.IsNullOrWhiteSpace(company.Email))
            throw new InvalidOperationException("SMTP configuration is incomplete.");
        using SmtpClient smtp = new(company.SmtpHost, company.SmtpPort.Value) { EnableSsl = company.SmtpUseTls };
        if (!string.IsNullOrWhiteSpace(company.SmtpUserName))
        {
            string password = string.IsNullOrWhiteSpace(company.SmtpPasswordEncrypted) ? string.Empty : _protector.Unprotect(company.SmtpPasswordEncrypted);
            smtp.Credentials = new NetworkCredential(company.SmtpUserName, password);
        }
        using MailMessage message = new(new MailAddress(company.Email, company.CompanyName), new MailAddress(recipient))
        { Subject = subject, Body = htmlBody, IsBodyHtml = true };
        await smtp.SendMailAsync(message, ct);
    }
}
