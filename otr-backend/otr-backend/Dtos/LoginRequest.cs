namespace otr_backend.Dtos;

/// <summary>Request body for logging in with e-mail address and password.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
