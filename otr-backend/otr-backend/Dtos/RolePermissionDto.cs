using otr_backend.Enums;

namespace otr_backend.Dtos;

public class RolePermissionDto
{
    public PermissionResource Resource { get; set; }
    public PermissionLevel Level { get; set; }
}
