namespace otr_backend.Services;

/// <summary>Sends plain-text e-mails on behalf of the application.</summary>
public interface IEmailSender
{
    /// <summary>Sends a plain-text message to a single recipient.</summary>
    /// <param name="toEmail">Recipient address.</param>
    /// <param name="subject">Subject line.</param>
    /// <param name="body">Plain-text message body.</param>
    /// <exception cref="InvalidOperationException">SMTP has not been configured yet.</exception>
    Task SendAsync(string toEmail, string subject, string body);
}
