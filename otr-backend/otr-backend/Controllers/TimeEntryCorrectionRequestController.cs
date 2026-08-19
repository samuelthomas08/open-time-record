using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Enums;
using otr_backend.Extensions;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TimeEntryCorrectionRequestController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TimeEntryCorrectionRequestController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeEntryCorrectionRequest>>> GetTimeEntryCorrectionRequests()
    {
        return Ok(await _context.TimeEntryCorrectionRequests.ToListAsync());
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<CorrectionRequestDto>>> GetMyCorrectionRequests()
    {
        uint userId = User.GetUserId();
        List<CorrectionRequestDto> requests = await _context.TimeEntryCorrectionRequests
            .Where(r => r.RequestedByUserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(ProjectToDto)
            .ToListAsync();

        return Ok(requests);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpGet("pending")]
    public async Task<ActionResult<List<CorrectionRequestDto>>> GetPendingCorrectionRequests()
    {
        List<CorrectionRequestDto> requests = await _context.TimeEntryCorrectionRequests
            .Where(r => r.Status == ApprovalStatus.Pending)
            .OrderByDescending(r => r.RequestedAt)
            .Select(ProjectToDto)
            .ToListAsync();

        return Ok(requests);
    }

    private static readonly System.Linq.Expressions.Expression<Func<TimeEntryCorrectionRequest, CorrectionRequestDto>> ProjectToDto =
        r => new CorrectionRequestDto
        {
            Id = r.Id,
            TimeEntryId = r.TimeEntryId,
            TimeEntryStartTime = r.TimeEntry.StartTime,
            TimeEntryEndTime = r.TimeEntry.EndTime,
            RequestedByUserId = r.RequestedByUserId,
            RequestedByUserName = r.RequestedByUser.FirstName + " " + r.RequestedByUser.LastName,
            Reason = r.Reason,
            ProposedStartTime = r.ProposedStartTime,
            ProposedEndTime = r.ProposedEndTime,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            ReviewNote = r.ReviewNote,
        };

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntryCorrectionRequest>> GetTimeEntryCorrectionRequestById(uint id)
    {
        TimeEntryCorrectionRequest? correctionRequest = await _context.TimeEntryCorrectionRequests.FindAsync(id);
        if (correctionRequest == null)
        {
            return NotFound();
        }

        return Ok(correctionRequest);
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryCorrectionRequest>> AddTimeEntryCorrectionRequest(CorrectionRequestCreateRequest request)
    {
        TimeEntry? timeEntry = await _context.TimeEntries.FindAsync(request.TimeEntryId);
        if (timeEntry == null)
        {
            return NotFound("Zeiteintrag nicht gefunden.");
        }

        uint userId = User.GetUserId();
        if (timeEntry.UserId != userId)
        {
            return Forbid();
        }

        bool alreadyPending = await _context.TimeEntryCorrectionRequests
            .AnyAsync(r => r.TimeEntryId == request.TimeEntryId && r.Status == ApprovalStatus.Pending);
        if (alreadyPending)
        {
            return Conflict("Für diesen Eintrag läuft bereits ein Korrekturantrag.");
        }

        var correctionRequest = new TimeEntryCorrectionRequest
        {
            TimeEntryId = request.TimeEntryId,
            RequestedByUserId = userId,
            Status = ApprovalStatus.Pending,
            Reason = request.Reason,
            ProposedStartTime = request.ProposedStartTime,
            ProposedEndTime = request.ProposedEndTime,
            RequestedAt = DateTime.UtcNow,
        };

        _context.TimeEntryCorrectionRequests.Add(correctionRequest);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTimeEntryCorrectionRequestById), new { id = correctionRequest.Id }, correctionRequest);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}/approve")]
    public Task<ActionResult<TimeEntryCorrectionRequest>> Approve(uint id)
    {
        return Review(id, ApprovalStatus.Approved, applyToTimeEntry: true, note: null);
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut("{id}/reject")]
    public Task<ActionResult<TimeEntryCorrectionRequest>> Reject(uint id, ReviewNoteRequest body)
    {
        return Review(id, ApprovalStatus.Rejected, applyToTimeEntry: false, note: body.Note);
    }

    private async Task<ActionResult<TimeEntryCorrectionRequest>> Review(uint id, ApprovalStatus status, bool applyToTimeEntry, string? note)
    {
        TimeEntryCorrectionRequest? request = await _context.TimeEntryCorrectionRequests
            .Include(r => r.TimeEntry)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null)
        {
            return NotFound();
        }

        if (request.Status != ApprovalStatus.Pending)
        {
            return Conflict("Dieser Antrag wurde bereits bearbeitet.");
        }

        request.Status = status;
        request.ReviewedByUserId = User.GetUserId();
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNote = string.IsNullOrWhiteSpace(note) ? null : note;

        if (applyToTimeEntry)
        {
            if (request.ProposedStartTime.HasValue)
            {
                request.TimeEntry.StartTime = request.ProposedStartTime.Value;
            }

            if (request.ProposedEndTime.HasValue)
            {
                request.TimeEntry.EndTime = request.ProposedEndTime.Value;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(request);
    }
}
