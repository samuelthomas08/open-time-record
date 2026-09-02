using otr_backend.Enums;

namespace otr_backend.Models;

public class RolePermissionMapping
{
    public uint RoleId { get; set; }
    public PermissionResource Resource { get; set; }
    public PermissionLevel Level { get; set; }

    public Role Role { get; set; } = null!;
}