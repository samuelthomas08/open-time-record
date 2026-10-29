namespace otr_backend.Dtos;

/// <summary>Request body for confirming an e-mail address with the emailed code.</summary>
public class ActivateRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
