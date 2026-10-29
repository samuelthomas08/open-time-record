namespace otr_backend.Dtos;

/// <summary>Returned after a successful login or registration.</summary>
public class AuthResponse
{
    /// <summary>JWT to send as <c>Authorization: Bearer</c> header on later requests.</summary>
    public string Token { get; set; } = string.Empty;

    public uint UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
