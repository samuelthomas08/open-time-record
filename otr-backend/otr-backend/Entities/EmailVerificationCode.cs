namespace otr_backend.Models;

public class EmailVerificationCode
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}
