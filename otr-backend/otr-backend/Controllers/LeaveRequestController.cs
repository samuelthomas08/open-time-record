using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Manages leave requests.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly OtrDbContext _context;

    public LeaveRequestController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all leave requests.</summary>
    [HttpGet]
    public async Task<ActionResult<List<LeaveRequest>>> GetLeaveRequests()
    {
        return Ok(await _context.LeaveRequests.ToListAsync());
    }

    /// <summary>Returns a single leave request or 404.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequest>> GetLeaveRequestById(uint id)
    {
        LeaveRequest? leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        return Ok(leaveRequest);
    }

    /// <summary>Creates a new leave request.</summary>
    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> AddLeaveRequest(LeaveRequest leaveRequest)
    {
        if (leaveRequest == null)
        {
            return BadRequest();
        }

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLeaveRequestById), new { id = leaveRequest.Id }, leaveRequest);
    }
}
