using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Enums;
using otr_backend.Extensions;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

/// <summary>Time tracking: starting and stopping timers, breaks, and managing entries. Users may only modify their own entries.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TimeEntryController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly ManagedUsersService _managedUsersService;

    public TimeEntryController(OtrDbContext context, ManagedUsersService managedUsersService)
    {
        _context = context;
        _managedUsersService = managedUsersService;
    }

    /// <summary>Returns all time entries.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TimeEntry>>> GetTimeEntries()
    {
        return Ok(await _context.TimeEntries.ToListAsync());
    }

    /// <summary>Returns the caller's entries including breaks, newest first.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<TimeEntry>>> GetMyTimeEntries()
    {
        uint userId = User.GetUserId();
        List<TimeEntry> entries = await _context.TimeEntries
            .Where(t => t.UserId == userId)
            .Include(t => t.Breaks)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();

        return Ok(entries);
    }

    /// <summary>Returns the entries of all users the caller manages (everyone but themselves for Superadmins), newest first.</summary>
    [HttpGet("team")]
    public async Task<ActionResult<List<TimeEntry>>> GetTeamTimeEntries()
    {
        uint userId = User.GetUserId();

        IQueryable<TimeEntry> query = _context.TimeEntries.Include(t => t.Breaks);

        if (!User.IsInRole("Superadmin"))
        {
            HashSet<uint> managedIds = await _managedUsersService.GetManagedUserIdsAsync(userId);
            query = query.Where(t => managedIds.Contains(t.UserId));
        }
        else
        {
            query = query.Where(t => t.UserId != userId);
        }

        List<TimeEntry> entries = await query.OrderByDescending(t => t.StartTime).ToListAsync();
        return Ok(entries);
    }

    /// <summary>Returns a single entry including its breaks.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntry>> GetTimeEntryById(uint id)
    {
        TimeEntry? timeEntry = await _context.TimeEntries
            .Include(t => t.Breaks)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (timeEntry == null)
        {
            return NotFound();
        }

        return Ok(timeEntry);
    }

    /// <summary>Creates a time entry for the caller from the supplied values.</summary>
    [HttpPost]
    public async Task<ActionResult<TimeEntry>> AddTimeEntry(TimeEntry timeEntry)
    {
        if (timeEntry == null)
        {
            return BadRequest();
        }

        // Entries can only be created for oneself, regardless of the user id in the request body.
        timeEntry.UserId = User.GetUserId();

        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTimeEntryById), new { id = timeEntry.Id }, timeEntry);
    }

    /// <summary>Starts a timer for the caller. Only one timer may run at a time.</summary>
    [HttpPost("start")]
    public async Task<ActionResult<TimeEntry>> StartTimeEntry()
    {
        uint userId = User.GetUserId();

        bool alreadyRunning = await _context.TimeEntries
            .AnyAsync(t => t.UserId == userId && t.Status == TimeEntryStatus.Running);
        if (alreadyRunning)
        {
            return Conflict("Es läuft bereits ein Zeiteintrag.");
        }

        TimeEntry timeEntry = new TimeEntry
        {
            UserId = userId,
            StartTime = DateTime.UtcNow,
            Status = TimeEntryStatus.Running,
            CreatedAt = DateTime.UtcNow,
        };

        _context.TimeEntries.Add(timeEntry);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTimeEntryById), new { id = timeEntry.Id }, timeEntry);
    }

    /// <summary>Stops the caller's running timer and ends a running break, if any.</summary>
    [HttpPost("{id}/stop")]
    public async Task<ActionResult<TimeEntry>> StopTimeEntry(uint id)
    {
        uint userId = User.GetUserId();

        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
        {
            return NotFound();
        }

        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }

        if (timeEntry.Status != TimeEntryStatus.Running)
        {
            return Conflict("Dieser Zeiteintrag läuft nicht.");
        }

        TimeEntryBreak? openBreak = await _context.TimeEntryBreaks
            .FirstOrDefaultAsync(b => b.TimeEntryId == id && b.EndTime == null);
        if (openBreak != null)
        {
            openBreak.EndTime = DateTime.UtcNow;
        }

        timeEntry.EndTime = DateTime.UtcNow;
        timeEntry.Status = TimeEntryStatus.Completed;
        await _context.SaveChangesAsync();

        return Ok(timeEntry);
    }

    /// <summary>Starts a break on a running entry. Only one break may run at a time.</summary>
    [HttpPost("{id}/breaks")]
    public async Task<ActionResult<TimeEntryBreak>> StartBreak(uint id, BreakStartRequest request)
    {
        uint userId = User.GetUserId();

        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
        {
            return NotFound();
        }
        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }
        if (timeEntry.Status != TimeEntryStatus.Running)
        {
            return Conflict("Nur bei einem laufenden Zeiteintrag kann eine Pause gestartet werden.");
        }

        bool hasOpenBreak = await _context.TimeEntryBreaks.AnyAsync(b => b.TimeEntryId == id && b.EndTime == null);
        if (hasOpenBreak)
        {
            return Conflict("Es läuft bereits eine Pause.");
        }

        AppSettings? appSettings = await _context.AppSettings.FirstOrDefaultAsync();
        bool reasonRequired = appSettings?.BreakReasonRequired ?? false;
        if (reasonRequired && string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Bitte einen Grund für die Pause angeben.");
        }

        TimeEntryBreak timeEntryBreak = new TimeEntryBreak
        {
            TimeEntryId = id,
            StartTime = DateTime.UtcNow,
            Reason = request.Reason,
        };
        _context.TimeEntryBreaks.Add(timeEntryBreak);
        await _context.SaveChangesAsync();

        return Ok(timeEntryBreak);
    }

    /// <summary>Ends a running break.</summary>
    [HttpPut("{id}/breaks/{breakId}/stop")]
    public async Task<ActionResult<TimeEntryBreak>> StopBreak(uint id, uint breakId)
    {
        uint userId = User.GetUserId();

        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
        {
            return NotFound();
        }
        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }

        TimeEntryBreak? timeEntryBreak = await _context.TimeEntryBreaks
            .FirstOrDefaultAsync(b => b.Id == breakId && b.TimeEntryId == id);
        if (timeEntryBreak == null)
        {
            return NotFound();
        }
        if (timeEntryBreak.EndTime != null)
        {
            return Conflict("Diese Pause läuft nicht mehr.");
        }

        timeEntryBreak.EndTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(timeEntryBreak);
    }

    /// <summary>Changes project, task and description of one of the caller's entries.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TimeEntry>> UpdateTimeEntry(uint id, TimeEntryUpdateRequest request)
    {
        uint userId = User.GetUserId();

        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
        {
            return NotFound();
        }

        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }

        timeEntry.ProjectId = request.ProjectId;
        timeEntry.ProjectTaskId = request.ProjectTaskId;
        timeEntry.Description = request.Description;

        await _context.SaveChangesAsync();
        return Ok(timeEntry);
    }

    /// <summary>Deletes one of the caller's entries.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTimeEntry(uint id)
    {
        uint userId = User.GetUserId();

        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(id);
        if (timeEntry == null)
        {
            return NotFound();
        }

        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }

        _context.TimeEntries.Remove(timeEntry);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
