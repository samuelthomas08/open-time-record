using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using otr_backend.Data;

namespace otr_backend.Services;

/// <summary>
/// <see cref="IEmailSender"/> that delivers mail through the SMTP server configured by a Superadmin.
/// The stored password is decrypted with Data Protection just before connecting.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly OtrDbContext _context;
    private readonly IDataProtector _protector;

    public SmtpEmailSender(OtrDbContext context, IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _protector = dataProtectionProvider.CreateProtector("SmtpSettings.Password");
    }

    /// <inheritdoc />
    public async Task SendAsync(string toEmail, string subject, string body)
    {
        Models.SmtpSettings? settings = await _context.SmtpSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            throw new InvalidOperationException("SMTP has not been configured yet.");
        }

        MimeMessage message = new();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using SmtpClient client = new();
        SecureSocketOptions socketOptions = settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
        await client.ConnectAsync(settings.Host, settings.Port, socketOptions);
        await client.AuthenticateAsync(settings.Username, _protector.Unprotect(settings.EncryptedPassword));
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
