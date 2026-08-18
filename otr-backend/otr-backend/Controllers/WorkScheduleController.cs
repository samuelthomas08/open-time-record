using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkScheduleController : ControllerBase
{
    private readonly OtrDbContext _context;

    public WorkScheduleController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkSchedule>>> GetWorkSchedules()
    {
        return Ok(await _context.WorkSchedules.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkSchedule>> GetWorkScheduleById(uint id)
    {
        WorkSchedule? workSchedule = await _context.WorkSchedules.FindAsync(id);
        if (workSchedule == null)
        {
            return NotFound();
        }

        return Ok(workSchedule);
    }

    [HttpPost]
    public async Task<ActionResult<WorkSchedule>> AddWorkSchedule(WorkSchedule workSchedule)
    {
        if (workSchedule == null)
        {
            return BadRequest();
        }

        _context.WorkSchedules.Add(workSchedule);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWorkScheduleById), new { id = workSchedule.Id }, workSchedule);
    }
}
