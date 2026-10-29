using Microsoft.EntityFrameworkCore;

namespace otr_backend.Models;

/// <summary>
/// A named bundle of permissions that can be assigned to users. The permissions themselves
/// live in <see cref="RolePermissionMapping"/>.
/// </summary>
[Index(nameof(RoleName), IsUnique = true)]
public class Role
{
    public uint Id { get; set; }

    /// <summary>Unique technical name, e.g. <c>Superadmin</c>.</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in the UI.</summary>
    public string RoleDisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
