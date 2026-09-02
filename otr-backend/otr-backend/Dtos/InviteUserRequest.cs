namespace otr_backend.Dtos;

public class InviteUserRequest
{
    public string Email { get; set; }
    public uint RoleId { get; set; }
    public uint? TeamId { get; set; }
    public uint? ManagerId { get; set; }
    public bool SendEmail { get; set; } = true;
}
