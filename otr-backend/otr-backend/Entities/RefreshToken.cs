namespace otr_backend.Models;

public class RefreshToken
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
