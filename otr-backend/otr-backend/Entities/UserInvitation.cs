namespace otr_backend.Models;

/// <summary>An invitation that lets a specific e-mail address register with a preassigned role.</summary>
public class UserInvitation
{
    public uint Id { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Six-digit code the invitee enters during registration.</summary>
    public string Code { get; set; } = string.Empty;

    public uint InvitedByUserId { get; set; }

    /// <summary>Role assigned to the invitee on registration.</summary>
    public uint RoleId { get; set; }

    /// <summary>Optional team the invitee joins on registration.</summary>
    public uint? TeamId { get; set; }

    /// <summary>Optional direct manager of the invitee.</summary>
    public uint? ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set once the invitee has registered; <c>null</c> while the invitation is open.</summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>Archived invitations are hidden from the list of open invitations.</summary>
    public bool IsArchived { get; set; }

    public User InvitedByUser { get; set; } = null!;
}
