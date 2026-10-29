namespace otr_backend.Models;

/// <summary>A one-time code that proves ownership of a newly registered e-mail address.</summary>
public class EmailVerificationCode
{
    public uint Id { get; set; }
    public uint UserId { get; set; }

    /// <summary>Six-digit numeric code sent to the user by e-mail.</summary>
    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    /// <summary>UTC point in time after which the code is no longer accepted.</summary>
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}
