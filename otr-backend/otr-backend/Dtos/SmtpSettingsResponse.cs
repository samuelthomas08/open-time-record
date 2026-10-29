namespace otr_backend.Dtos;

/// <summary>The stored mail server configuration. The password is never returned.</summary>
public class SmtpSettingsResponse
{
    /// <summary>Whether a configuration exists at all; all other fields are <c>null</c> if not.</summary>
    public bool IsConfigured { get; set; }

    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public bool? UseSsl { get; set; }
}
