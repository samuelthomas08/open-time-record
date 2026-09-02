using Microsoft.EntityFrameworkCore;

namespace otr_backend.Models;

[Index(nameof(RoleName), IsUnique = true)]
public class Role
{
    public uint Id { get; set; }
    public string RoleName { get; set; }
    public string RoleDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}