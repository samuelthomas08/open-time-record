namespace otr_backend.Models;

/// <summary>A single-use token that allows a user to choose a new password.</summary>
public class PasswordResetToken
{
    public uint Id { get; set; }
    public uint UserId { get; set; }

    /// <summary>Hash of the token; the plain token is only ever sent to the user.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set once the token has been redeemed; <c>null</c> while it is still usable.</summary>
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}
