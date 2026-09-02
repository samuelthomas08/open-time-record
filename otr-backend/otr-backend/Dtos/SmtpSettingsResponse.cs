namespace otr_backend.Dtos;

public class SmtpSettingsResponse
{
    public bool IsConfigured { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? FromAddress { get; set; }
    public string? FromName { get; set; }
    public bool? UseSsl { get; set; }
}
