using Microsoft.EntityFrameworkCore;
using otr_backend.Data;

namespace otr_backend.Services;

public class ManagedUsersService
{
    private readonly OtrDbContext _context;

    public ManagedUsersService(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Direct reports (User.ManagerId) plus members of teams this user manages, deduplicated.
    /// Does not include the manager themselves.
    /// </summary>
    public async Task<HashSet<uint>> GetManagedUserIdsAsync(uint userId)
    {
        List<uint> managedTeamIds = await _context.Teams
            .Where(t => t.ManagerId == userId)
            .Select(t => t.Id)
            .ToListAsync();

        List<uint> teamMemberIds = await _context.UserTeamMappings
            .Where(m => managedTeamIds.Contains(m.TeamId))
            .Select(m => m.UserId)
            .ToListAsync();

        List<uint> directReportIds = await _context.Users
            .Where(u => u.ManagerId == userId)
            .Select(u => u.Id)
            .ToListAsync();

        HashSet<uint> managedIds = teamMemberIds.Concat(directReportIds).ToHashSet();
        managedIds.Remove(userId);
        return managedIds;
    }
}
