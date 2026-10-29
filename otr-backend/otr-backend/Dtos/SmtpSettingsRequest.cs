namespace otr_backend.Dtos;

/// <summary>Request body for configuring the outgoing mail server.</summary>
public class SmtpSettingsRequest
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>Plain-text password. Leave empty on later updates to keep the stored one.</summary>
    public string? Password { get; set; }

    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
}
