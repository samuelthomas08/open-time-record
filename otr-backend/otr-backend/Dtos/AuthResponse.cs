namespace otr_backend.Dtos;

public class AuthResponse
{
    public string Token { get; set; }
    public uint UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; } = new();
}
