namespace otr_backend.Dtos;

/// <summary>Result of a registration: either a session or an instruction to verify the e-mail address.</summary>
public class RegisterResponse
{
    /// <summary>Set when the account is already verified and the user is logged in right away.</summary>
    public AuthResponse? Auth { get; set; }

    /// <summary>Set when a verification code was sent and the user still has to confirm it.</summary>
    public string? Message { get; set; }
}
