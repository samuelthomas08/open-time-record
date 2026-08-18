namespace otr_backend.Models;

public class UserRoleMapping
{
    public uint UserId { get; set; }
    public uint RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}