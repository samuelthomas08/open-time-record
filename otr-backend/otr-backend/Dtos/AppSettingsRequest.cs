namespace otr_backend.Dtos;

/// <summary>Request body for updating the instance-wide feature toggles.</summary>
public class AppSettingsRequest
{
    public bool ProfilePicturesEnabled { get; set; }
    public bool BreakReasonRequired { get; set; }
}
