using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly OtrDbContext _context;

    public ProjectController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetProjects()
    {
        return Ok(await _context.Projects.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProjectById(uint id)
    {
        Project? project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPost]
    public async Task<ActionResult<Project>> AddProject(Project project)
    {
        if (project == null)
        {
            return BadRequest();
        }

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Project>> UpdateProject(uint id, Project project)
    {
        Project? existing = await _context.Projects.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.ProjectName = project.ProjectName;
        existing.ProjectDisplayName = project.ProjectDisplayName;
        existing.IsActive = project.IsActive;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}
