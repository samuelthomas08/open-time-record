namespace otr_backend.Dtos;

public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public uint RoleId { get; set; }
    public uint? TeamId { get; set; }
    public uint? ManagerId { get; set; }
}
