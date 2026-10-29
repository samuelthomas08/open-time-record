namespace otr_backend.Dtos;

/// <summary>The current instance-wide feature toggles.</summary>
public class AppSettingsResponse
{
    public bool ProfilePicturesEnabled { get; set; }
    public bool BreakReasonRequired { get; set; }
}
