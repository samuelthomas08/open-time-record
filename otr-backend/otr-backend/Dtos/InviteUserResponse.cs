namespace otr_backend.Dtos;

public class InviteUserResponse
{
    public string Message { get; set; }

    /// <summary>Only set when the invitation was created without sending an email.</summary>
    public string? InvitationCode { get; set; }
}
