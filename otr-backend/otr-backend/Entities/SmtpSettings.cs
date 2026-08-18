namespace otr_backend.Models;

public class SmtpSettings
{
    public uint Id { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string EncryptedPassword { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public bool UseSsl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
