using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Read-only access to the time entry audit log. Entries are written internally whenever a time entry changes, never created through the API.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TimeEntryAuditLogController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TimeEntryAuditLogController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all audit log entries.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TimeEntryAuditLog>>> GetTimeEntryAuditLogs()
    {
        return Ok(await _context.TimeEntryAuditLogs.ToListAsync());
    }

    /// <summary>Returns a single audit log entry.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntryAuditLog>> GetTimeEntryAuditLogById(uint id)
    {
        TimeEntryAuditLog? auditLog = await _context.TimeEntryAuditLogs.FindAsync(id);
        if (auditLog == null)
        {
            return NotFound();
        }

        return Ok(auditLog);
    }
}
