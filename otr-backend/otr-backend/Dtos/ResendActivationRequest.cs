namespace otr_backend.Dtos;

/// <summary>Request body for sending a new verification code.</summary>
public class ResendActivationRequest
{
    public string Email { get; set; } = string.Empty;
}
