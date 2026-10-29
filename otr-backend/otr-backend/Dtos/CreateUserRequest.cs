namespace otr_backend.Dtos;

/// <summary>Request body for an administrator creating a verified account directly.</summary>
public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Initial plain-text password; only used to compute the stored hash.</summary>
    public string Password { get; set; } = string.Empty;

    public uint RoleId { get; set; }
    public uint? TeamId { get; set; }
    public uint? ManagerId { get; set; }
}
