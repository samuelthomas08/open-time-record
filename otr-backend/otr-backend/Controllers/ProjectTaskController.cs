using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectTaskController : ControllerBase
{
    private readonly OtrDbContext _context;

    public ProjectTaskController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectTask>>> GetProjectTasks()
    {
        return Ok(await _context.ProjectTasks.ToListAsync());
    }

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
