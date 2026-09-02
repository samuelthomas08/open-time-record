using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LeaveTypeController : ControllerBase
{
    private readonly OtrDbContext _context;

    public LeaveTypeController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaveType>>> GetLeaveTypes()
    {
        return Ok(await _context.LeaveTypes.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveType>> GetLeaveTypeById(uint id)
    {
        LeaveType? leaveType = await _context.LeaveTypes.FindAsync(id);
        if (leaveType == null)
        {
            return NotFound();
        }

        return Ok(leaveType);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveType>> AddLeaveType(LeaveType leaveType)
    {
        if (leaveType == null)
        {
            return BadRequest();
        }

        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLeaveTypeById), new { id = leaveType.Id }, leaveType);
    }
}
