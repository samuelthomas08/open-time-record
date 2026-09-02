using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LeaveBalanceController : ControllerBase
{
    private readonly OtrDbContext _context;

    public LeaveBalanceController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaveBalance>>> GetLeaveBalances()
    {
        return Ok(await _context.LeaveBalances.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveBalance>> GetLeaveBalanceById(uint id)
    {
        LeaveBalance? leaveBalance = await _context.LeaveBalances.FindAsync(id);
        if (leaveBalance == null)
        {
            return NotFound();
        }

        return Ok(leaveBalance);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveBalance>> AddLeaveBalance(LeaveBalance leaveBalance)
    {
        if (leaveBalance == null)
        {
            return BadRequest();
        }

        _context.LeaveBalances.Add(leaveBalance);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLeaveBalanceById), new { id = leaveBalance.Id }, leaveBalance);
    }
}
