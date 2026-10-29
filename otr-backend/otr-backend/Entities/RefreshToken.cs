namespace otr_backend.Models;

/// <summary>A long-lived token that can be exchanged for a new access token.</summary>
public class RefreshToken
{
    public uint Id { get; set; }
    public uint UserId { get; set; }

    /// <summary>Hash of the token; the plain token is never stored.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Set when the token was revoked before its expiry.</summary>
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
