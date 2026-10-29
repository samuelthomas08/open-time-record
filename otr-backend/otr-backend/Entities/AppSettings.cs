namespace otr_backend.Models;

/// <summary>
/// Instance-wide feature toggles. Stored as a single row; the row is created lazily
/// the first time a Superadmin saves the settings.
/// </summary>
public class AppSettings
{
    public uint Id { get; set; }

    /// <summary>Whether users may upload a profile picture.</summary>
    public bool ProfilePicturesEnabled { get; set; }

    /// <summary>Whether a reason must be given when a break is started.</summary>
    public bool BreakReasonRequired { get; set; }
}
