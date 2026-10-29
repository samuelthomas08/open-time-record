namespace otr_backend.Dtos;

/// <summary>Request body for inviting a person by e-mail address.</summary>
public class InviteUserRequest
{
    public string Email { get; set; } = string.Empty;
    public uint RoleId { get; set; }
    public uint? TeamId { get; set; }
    public uint? ManagerId { get; set; }

    /// <summary>When <c>false</c>, no e-mail is sent and the code is returned to the caller instead.</summary>
    public bool SendEmail { get; set; } = true;
}
