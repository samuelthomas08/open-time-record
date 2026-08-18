namespace otr_backend.Models;

public class UserInvitation
{
    public uint Id { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public uint InvitedByUserId { get; set; }
    public uint RoleId { get; set; }
    public uint? TeamId { get; set; }
    public uint? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public User InvitedByUser { get; set; } = null!;
}
