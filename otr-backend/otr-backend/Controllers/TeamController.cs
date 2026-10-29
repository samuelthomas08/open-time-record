using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Manages teams.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TeamController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all teams.</summary>
    [HttpGet]
    public async Task<ActionResult<List<Team>>> GetTeams()
    {
        return Ok(await _context.Teams.ToListAsync());
    }

    /// <summary>Returns a single team or 404.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetTeamById(uint id)
    {
        Team? team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>Looks up a team by its technical name.</summary>
    [HttpGet("by-name/{teamName}")]
    public async Task<ActionResult<Team>> GetTeamByTeamName(string teamName)
    {
        Team? team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamName == teamName);
        if (team == null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    /// <summary>Creates a new team.</summary>
    [Authorize(Roles = "Superadmin")]
    [HttpPost]
    public async Task<ActionResult<Team>> AddTeam(Team team)
    {
        if (team == null)
        {
            return BadRequest();
        }

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTeamById), new { id = team.Id }, team);
    }

    /// <summary>Updates name, display name, manager and active flag of a team.</summary>
    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> UpdateTeam(uint id, Team team)
    {
        Team? existing = await _context.Teams.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.TeamName = team.TeamName;
        existing.TeamDisplayName = team.TeamDisplayName;
        existing.ManagerId = team.ManagerId;
        existing.IsActive = team.IsActive;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}
