namespace otr_backend.Dtos;

/// <summary>Request body for self-registration.</summary>
public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Code from an invitation; skips e-mail verification and applies the invited role.</summary>
    public string? InvitationCode { get; set; }
}
