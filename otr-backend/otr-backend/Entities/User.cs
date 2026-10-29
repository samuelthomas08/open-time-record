using System.Text.Json.Serialization;

namespace otr_backend.Models;

/// <summary>An account that can log in and track time.</summary>
public class User
{
    public uint Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>ASP.NET Core Identity password hash. Never serialized into API responses.</summary>
    [JsonIgnore]
    public string HashedPassword { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }

    /// <summary>Deactivated users cannot log in.</summary>
    public bool IsActive { get; set; }

    /// <summary>Whether the user has proven ownership of the e-mail address.</summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>Relative URL of the uploaded profile picture, if any.</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>Id of the user's direct manager; <c>null</c> if nobody manages them.</summary>
    public uint? ManagerId { get; set; }
}
