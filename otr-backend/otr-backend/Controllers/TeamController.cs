using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TeamController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Team>>> GetTeams()
    {
        return Ok(await _context.Teams.ToListAsync());
    }

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
