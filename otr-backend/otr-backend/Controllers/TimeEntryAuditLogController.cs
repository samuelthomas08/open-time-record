using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

// Read-only: audit log entries are written internally whenever a TimeEntry
// changes, never created directly through the API.
[Route("api/[controller]")]
[ApiController]
public class TimeEntryAuditLogController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TimeEntryAuditLogController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeEntryAuditLog>>> GetTimeEntryAuditLogs()
    {
        return Ok(await _context.TimeEntryAuditLogs.ToListAsync());
    }

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
