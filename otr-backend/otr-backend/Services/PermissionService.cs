using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Enums;
using otr_backend.Models;

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

    /// <summary>
    /// The user's own highest permission level per resource, across all of their roles.
    /// Resources the user has no role-grant for at all are simply absent from the result.
    /// </summary>
    public async Task<Dictionary<PermissionResource, PermissionLevel>> GetEffectivePermissionsAsync(uint userId)
    {
        List<RolePermissionMapping> mappings = await _context.UserRoleMappings
            .Where(m => m.UserId == userId)
            .SelectMany(m => _context.RolePermissionMappings.Where(p => p.RoleId == m.RoleId))
            .ToListAsync();

        Dictionary<PermissionResource, PermissionLevel> result = new();
        foreach (RolePermissionMapping mapping in mappings)
        {
            if (!result.TryGetValue(mapping.Resource, out PermissionLevel existing) || mapping.Level > existing)
            {
                result[mapping.Resource] = mapping.Level;
            }
        }

        return result;
    }
}
