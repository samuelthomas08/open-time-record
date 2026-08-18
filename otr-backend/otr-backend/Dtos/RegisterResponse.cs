namespace otr_backend.Dtos;

public class RegisterResponse
{
    public AuthResponse? Auth { get; set; }
    public string? Message { get; set; }
}
