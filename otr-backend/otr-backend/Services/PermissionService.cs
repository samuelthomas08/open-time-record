using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Enums;

namespace otr_backend.Services;

public class PermissionService
{
    private readonly OtrDbContext _context;

    public PermissionService(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// True if any role assigned to the user grants at least <paramref name="minLevel"/> on
    /// <paramref name="resource"/>. Superadmin already holds Admin on every resource via the
    /// startup seed, so it needs no separate bypass here.
    /// </summary>
    public async Task<bool> HasPermissionAsync(uint userId, PermissionResource resource, PermissionLevel minLevel)
    {
        List<PermissionLevel> levels = await _context.UserRoleMappings
            .Where(m => m.UserId == userId)
            .SelectMany(m => _context.RolePermissionMappings
                .Where(p => p.RoleId == m.RoleId && p.Resource == resource)
                .Select(p => p.Level))
            .ToListAsync();

        return levels.Any(level => level >= minLevel);
    }
}
