namespace otr_backend.Models;

public class PasswordResetToken
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}
