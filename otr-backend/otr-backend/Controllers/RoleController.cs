using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController : Controller
{
    private readonly OtrDbContext _context;

    public RoleController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Role>>> GetRoles()
    {
        return Ok(await _context.Roles.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRoleById(uint id)
    {
        Role? role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [HttpGet("by-name/{roleName}")]
    public async Task<ActionResult<Role>> GetRoleByRoleName(string roleName)
    {
        Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPost]
    public async Task<ActionResult<Role>> AddRole(Role role)
    {
        if (role == null)
        {
            return BadRequest();
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Role>> UpdateRole(uint id, Role role)
    {
        Role? existing = await _context.Roles.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.RoleName = role.RoleName;
        existing.RoleDisplayName = role.RoleDisplayName;
        existing.IsActive = role.IsActive;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<List<RolePermissionDto>>> GetRolePermissions(uint id)
    {
        List<RolePermissionDto> permissions = await _context.RolePermissionMappings
            .Where(m => m.RoleId == id)
            .Select(m => new RolePermissionDto { Resource = m.Resource, Level = m.Level })
            .ToListAsync();

        return Ok(permissions);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}/permissions")]
    public async Task<ActionResult<List<RolePermissionDto>>> UpdateRolePermissions(uint id, List<RolePermissionDto> permissions)
    {
        bool roleExists = await _context.Roles.AnyAsync(r => r.Id == id);
        if (!roleExists)
        {
            return NotFound();
        }

        List<RolePermissionMapping> existing = await _context.RolePermissionMappings
            .Where(m => m.RoleId == id)
            .ToListAsync();
        _context.RolePermissionMappings.RemoveRange(existing);

        foreach (RolePermissionDto permission in permissions.Where(p => p.Level != Enums.PermissionLevel.None))
        {
            _context.RolePermissionMappings.Add(new RolePermissionMapping
            {
                RoleId = id,
                Resource = permission.Resource,
                Level = permission.Level,
            });
        }

        await _context.SaveChangesAsync();

        List<RolePermissionDto> result = await _context.RolePermissionMappings
            .Where(m => m.RoleId == id)
            .Select(m => new RolePermissionDto { Resource = m.Resource, Level = m.Level })
            .ToListAsync();

        return Ok(result);
    }
}
