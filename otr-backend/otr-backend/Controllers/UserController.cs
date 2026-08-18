using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Enums;
using otr_backend.Extensions;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private const long MaxProfilePictureBytes = 2 * 1024 * 1024;

    private readonly OtrDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly PermissionService _permissionService;
    private readonly ManagedUsersService _managedUsersService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserController(
        OtrDbContext context,
        IWebHostEnvironment environment,
        PermissionService permissionService,
        ManagedUsersService managedUsersService)
    {
        this._context = context;
        this._environment = environment;
        this._permissionService = permissionService;
        this._managedUsersService = managedUsersService;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        bool allowed = await _permissionService.HasPermissionAsync(User.GetUserId(), PermissionResource.Users, PermissionLevel.Admin);
        if (!allowed)
        {
            return Forbid();
        }

        bool emailTaken = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            return Conflict("Diese E-Mail-Adresse wird bereits verwendet.");
        }

        bool roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
        if (!roleExists)
        {
            return BadRequest("Die ausgewählte Rolle existiert nicht.");
        }

        if (request.TeamId.HasValue && !await _context.Teams.AnyAsync(t => t.Id == request.TeamId))
        {
            return BadRequest("Das ausgewählte Team existiert nicht.");
        }

        if (request.ManagerId.HasValue && !await _context.Users.AnyAsync(u => u.Id == request.ManagerId))
        {
            return BadRequest("Der ausgewählte Vorgesetzte existiert nicht.");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            HashedPassword = "",
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = true,
            ManagerId = request.ManagerId,
        };
        user.HashedPassword = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, RoleId = request.RoleId });
        if (request.TeamId.HasValue)
        {
            _context.UserTeamMappings.Add(new UserTeamMapping { UserId = user.Id, TeamId = request.TeamId.Value });
        }
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(uint id)
    {
        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("{id}/enriched")]
    public async Task<ActionResult<EnrichedUserDto>> GetEnrichedUser(uint id)
    {
        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        List<Role> roles = await _context.UserRoleMappings
            .Where(m => m.UserId == id)
            .Select(m => m.Role)
            .ToListAsync();

        List<Team> teams = await _context.UserTeamMappings
            .Where(m => m.UserId == id)
            .Select(m => m.Team)
            .ToListAsync();

        List<Project> projects = await _context.ProjectMembers
            .Where(m => m.UserId == id)
            .Select(m => m.Project)
            .ToListAsync();

        List<Team> managedTeams = await _context.Teams
            .Where(t => t.ManagerId == id)
            .ToListAsync();

        List<TimeEntry> timeEntries = await _context.TimeEntries
            .Where(t => t.UserId == id)
            .ToListAsync();

        List<WorkSchedule> workSchedules = await _context.WorkSchedules
            .Where(w => w.UserId == id)
            .ToListAsync();

        List<LeaveBalance> leaveBalances = await _context.LeaveBalances
            .Where(b => b.UserId == id)
            .Include(b => b.LeaveType)
            .ToListAsync();

        List<Notification> notifications = await _context.Notifications
            .Where(n => n.UserId == id)
            .ToListAsync();

        List<TimeEntryAuditLog> auditLogsMade = await _context.TimeEntryAuditLogs
            .Where(l => l.ChangedByUserId == id)
            .ToListAsync();

        List<TimeEntryCorrectionRequest> correctionRequestsSubmitted = await _context.TimeEntryCorrectionRequests
            .Where(r => r.RequestedByUserId == id)
            .ToListAsync();

        List<TimeEntryCorrectionRequest> correctionRequestsReviewed = await _context.TimeEntryCorrectionRequests
            .Where(r => r.ReviewedByUserId == id)
            .ToListAsync();

        List<LeaveRequest> leaveRequestsSubmitted = await _context.LeaveRequests
            .Where(r => r.RequestedByUserId == id)
            .Include(r => r.LeaveType)
            .ToListAsync();

        List<LeaveRequest> leaveRequestsReviewed = await _context.LeaveRequests
            .Where(r => r.ReviewedByUserId == id)
            .Include(r => r.LeaveType)
            .ToListAsync();

        List<UserInvitation> invitationsSent = await _context.UserInvitations
            .Where(i => i.InvitedByUserId == id)
            .ToListAsync();

        return Ok(new EnrichedUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            Roles = roles,
            Teams = teams,
            Projects = projects,
            ManagedTeams = managedTeams,
            TimeEntries = timeEntries,
            WorkSchedules = workSchedules,
            LeaveBalances = leaveBalances,
            Notifications = notifications,
            AuditLogsMade = auditLogsMade,
            CorrectionRequestsSubmitted = correctionRequestsSubmitted,
            CorrectionRequestsReviewed = correctionRequestsReviewed,
            LeaveRequestsSubmitted = leaveRequestsSubmitted,
            LeaveRequestsReviewed = leaveRequestsReviewed,
            InvitationsSent = invitationsSent,
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(uint id, User updatedUser)
    {
        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.IsActive = updatedUser.IsActive;

        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<Role>>> GetUserRoles(uint id)
    {
        List<Role> roles = await _context.UserRoleMappings
            .Where(m => m.UserId == id)
            .Select(m => m.Role)
            .ToListAsync();

        return Ok(roles);
    }

    // Gated to Superadmin for now as a stand-in until real hierarchy-aware
    // permissions (see MVP plan, Stufe 2) replace this simple role check.
    [Authorize(Roles = "Superadmin")]
    [HttpPost("{id}/roles/{roleId}")]
    public async Task<ActionResult> AssignRole(uint id, uint roleId)
    {
        bool userExists = await _context.Users.AnyAsync(u => u.Id == id);
        bool roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
        if (!userExists || !roleExists)
        {
            return NotFound();
        }

        bool alreadyAssigned = await _context.UserRoleMappings.AnyAsync(m => m.UserId == id && m.RoleId == roleId);
        if (alreadyAssigned)
        {
            return Conflict("Rolle ist bereits zugewiesen.");
        }

        _context.UserRoleMappings.Add(new UserRoleMapping { UserId = id, RoleId = roleId });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Superadmin")]
    [HttpDelete("{id}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRole(uint id, uint roleId)
    {
        UserRoleMapping? mapping = await _context.UserRoleMappings
            .FirstOrDefaultAsync(m => m.UserId == id && m.RoleId == roleId);
        if (mapping == null)
        {
            return NotFound();
        }

        _context.UserRoleMappings.Remove(mapping);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/teams")]
    public async Task<ActionResult<List<Team>>> GetUserTeams(uint id)
    {
        List<Team> teams = await _context.UserTeamMappings
            .Where(m => m.UserId == id)
            .Select(m => m.Team)
            .ToListAsync();

        return Ok(teams);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPost("{id}/teams/{teamId}")]
    public async Task<ActionResult> AssignTeam(uint id, uint teamId)
    {
        bool userExists = await _context.Users.AnyAsync(u => u.Id == id);
        bool teamExists = await _context.Teams.AnyAsync(t => t.Id == teamId);
        if (!userExists || !teamExists)
        {
            return NotFound();
        }

        bool alreadyAssigned = await _context.UserTeamMappings.AnyAsync(m => m.UserId == id && m.TeamId == teamId);
        if (alreadyAssigned)
        {
            return Conflict("Team ist bereits zugewiesen.");
        }

        _context.UserTeamMappings.Add(new UserTeamMapping { UserId = id, TeamId = teamId });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Superadmin")]
    [HttpDelete("{id}/teams/{teamId}")]
    public async Task<IActionResult> RemoveTeam(uint id, uint teamId)
    {
        UserTeamMapping? mapping = await _context.UserTeamMappings
            .FirstOrDefaultAsync(m => m.UserId == id && m.TeamId == teamId);
        if (mapping == null)
        {
            return NotFound();
        }

        _context.UserTeamMappings.Remove(mapping);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("managed")]
    public async Task<ActionResult<List<User>>> GetManagedUsers()
    {
        uint userId = User.GetUserId();

        if (User.IsInRole("Superadmin"))
        {
            return Ok(await _context.Users.Where(u => u.Id != userId).ToListAsync());
        }

        HashSet<uint> managedIds = await _managedUsersService.GetManagedUserIdsAsync(userId);
        return Ok(await _context.Users.Where(u => managedIds.Contains(u.Id)).ToListAsync());
    }

    [HttpGet("me/permissions")]
    public async Task<ActionResult<List<RolePermissionDto>>> GetMyPermissions()
    {
        Dictionary<PermissionResource, PermissionLevel> effective = await _permissionService.GetEffectivePermissionsAsync(User.GetUserId());
        return Ok(effective.Select(kv => new RolePermissionDto { Resource = kv.Key, Level = kv.Value }).ToList());
    }

    [HttpPost("{id}/profile-picture")]
    public async Task<ActionResult<User>> UploadProfilePicture(uint id, IFormFile file)
    {
        if (id != User.GetUserId())
        {
            return Forbid();
        }

        bool allowed = await _permissionService.HasPermissionAsync(id, PermissionResource.ProfilePicture, PermissionLevel.Write);
        if (!allowed)
        {
            return Forbid();
        }

        Models.AppSettings? appSettings = await _context.AppSettings.FirstOrDefaultAsync();
        if (appSettings is not { ProfilePicturesEnabled: true })
        {
            return BadRequest("Profilbilder sind derzeit deaktiviert.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("Keine Datei übermittelt.");
        }
        if (file.Length > MaxProfilePictureBytes)
        {
            return BadRequest("Datei ist zu groß (max. 2 MB).");
        }

        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        byte[] header = new byte[12];
        await using (Stream stream = file.OpenReadStream())
        {
            _ = await stream.ReadAsync(header.AsMemory(0, 12));
        }

        string? extension = DetectImageExtension(header);
        if (extension == null)
        {
            return BadRequest("Nur PNG, JPEG, GIF oder WEBP werden unterstützt.");
        }

        string uploadsDir = Path.Combine(_environment.WebRootPath, "profile-pictures");
        Directory.CreateDirectory(uploadsDir);

        // Clear out any previous picture saved under a different extension.
        foreach (string existing in Directory.GetFiles(uploadsDir, $"{id}.*"))
        {
            System.IO.File.Delete(existing);
        }

        string filePath = Path.Combine(uploadsDir, $"{id}{extension}");
        await using (FileStream output = System.IO.File.Create(filePath))
        await using (Stream input = file.OpenReadStream())
        {
            await input.CopyToAsync(output);
        }

        // Cache-busts the browser after a re-upload replaces the file at the same URL.
        user.ProfilePictureUrl = $"/profile-pictures/{id}{extension}?v={DateTime.UtcNow.Ticks}";
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete("{id}/profile-picture")]
    public async Task<ActionResult<User>> DeleteProfilePicture(uint id)
    {
        if (id != User.GetUserId())
        {
            return Forbid();
        }

        bool allowed = await _permissionService.HasPermissionAsync(id, PermissionResource.ProfilePicture, PermissionLevel.Write);
        if (!allowed)
        {
            return Forbid();
        }

        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        string uploadsDir = Path.Combine(_environment.WebRootPath, "profile-pictures");
        if (Directory.Exists(uploadsDir))
        {
            foreach (string existing in Directory.GetFiles(uploadsDir, $"{id}.*"))
            {
                System.IO.File.Delete(existing);
            }
        }

        user.ProfilePictureUrl = null;
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    /// <summary>
    /// Sniffs the file's own magic bytes rather than trusting the client's content-type or
    /// filename, so the extension we write to disk (and therefore serve back) always matches
    /// what the bytes actually are.
    /// </summary>
    private static string? DetectImageExtension(byte[] header)
    {
        if (header.Length >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
        {
            return ".png";
        }
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return ".jpg";
        }
        if (header.Length >= 4 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
        {
            return ".gif";
        }
        if (header.Length >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
            header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return ".webp";
        }

        return null;
    }
}
