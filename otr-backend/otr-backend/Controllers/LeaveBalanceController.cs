using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Manages yearly leave entitlements.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LeaveBalanceController : ControllerBase
{
    private readonly OtrDbContext _context;

    public LeaveBalanceController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all leave balances.</summary>
    [HttpGet]
    public async Task<ActionResult<List<LeaveBalance>>> GetLeaveBalances()
    {
        return Ok(await _context.LeaveBalances.ToListAsync());
    }

    /// <summary>Returns a single leave balance or 404.</summary>
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

    /// <summary>Creates a new leave balance.</summary>
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
