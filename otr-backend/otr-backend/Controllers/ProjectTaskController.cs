using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Manages tasks within projects.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProjectTaskController : ControllerBase
{
    private readonly OtrDbContext _context;

    public ProjectTaskController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all project tasks.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProjectTask>>> GetProjectTasks()
    {
        return Ok(await _context.ProjectTasks.ToListAsync());
    }

    /// <summary>Returns a single project task or 404.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectTask>> GetProjectTaskById(uint id)
    {
        ProjectTask? projectTask = await _context.ProjectTasks.FindAsync(id);
        if (projectTask == null)
        {
            return NotFound();
        }

        return Ok(projectTask);
    }

    /// <summary>Creates a new project task.</summary>
    [HttpPost]
    public async Task<ActionResult<ProjectTask>> AddProjectTask(ProjectTask projectTask)
    {
        if (projectTask == null)
        {
            return BadRequest();
        }

        _context.ProjectTasks.Add(projectTask);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProjectTaskById), new { id = projectTask.Id }, projectTask);
    }
}
