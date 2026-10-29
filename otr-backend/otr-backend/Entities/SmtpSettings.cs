namespace otr_backend.Models;

/// <summary>Outgoing mail server configuration. Stored as a single row.</summary>
public class SmtpSettings
{
    public uint Id { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>SMTP password, encrypted with ASP.NET Core Data Protection.</summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;

    /// <summary>Use implicit TLS (SSL-on-connect) instead of negotiating the security mode.</summary>
    public bool UseSsl { get; set; }

    public DateTime UpdatedAt { get; set; }
}
