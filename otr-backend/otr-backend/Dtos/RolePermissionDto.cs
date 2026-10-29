using otr_backend.Enums;

namespace otr_backend.Dtos;

/// <summary>The access level a role (or user) holds on one resource.</summary>
public class RolePermissionDto
{
    public PermissionResource Resource { get; set; }
    public PermissionLevel Level { get; set; }
}
