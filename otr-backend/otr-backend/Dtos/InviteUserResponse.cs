namespace otr_backend.Dtos;

/// <summary>Result of creating an invitation.</summary>
public class InviteUserResponse
{
    public string Message { get; set; } = string.Empty;

    /// <summary>Only set when the invitation was created without sending an email.</summary>
    public string? InvitationCode { get; set; }
}
