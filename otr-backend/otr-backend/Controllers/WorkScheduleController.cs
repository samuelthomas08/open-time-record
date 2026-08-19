using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Extensions;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class WorkScheduleController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly ManagedUsersService _managedUsersService;

    public WorkScheduleController(OtrDbContext context, ManagedUsersService managedUsersService)
    {
        this._context = context;
        this._managedUsersService = managedUsersService;
    }

    // Every work schedule entry for every user the caller manages (or everyone but themselves, for Superadmins) —
    // mirrors TimeEntryController's "team" endpoint so the frontend can group client-side.
    [HttpGet("managed")]
    public async Task<ActionResult<List<WorkSchedule>>> GetManagedWorkSchedules()
    {
        uint userId = User.GetUserId();

        IQueryable<WorkSchedule> query = _context.WorkSchedules;

        if (!User.IsInRole("Superadmin"))
        {
            HashSet<uint> managedIds = await _managedUsersService.GetManagedUserIdsAsync(userId);
            query = query.Where(w => managedIds.Contains(w.UserId));
        }
        else
        {
            query = query.Where(w => w.UserId != userId);
        }

        List<WorkSchedule> schedules = await query.OrderByDescending(w => w.EffectiveFrom).ToListAsync();
        return Ok(schedules);
    }

    // Every own schedule period — no manager check needed, everyone may see their own.
    [HttpGet("mine")]
    public async Task<ActionResult<List<WorkSchedule>>> GetMyWorkSchedules()
    {
        uint userId = User.GetUserId();
        List<WorkSchedule> schedules = await _context.WorkSchedules
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.EffectiveFrom)
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<WorkSchedule>> AddWorkSchedule(WorkScheduleUpsertRequest request)
    {
        if (!await CanManageAsync(request.UserId))
        {
            return Forbid();
        }

        if (!HasValidDayHours(request))
        {
            return BadRequest("Die Sollstunden je Wochentag müssen zwischen 0 und 24 liegen.");
        }

        var workSchedule = new WorkSchedule
        {
            UserId = request.UserId,
            MondayHours = request.MondayHours,
            TuesdayHours = request.TuesdayHours,
            WednesdayHours = request.WednesdayHours,
            ThursdayHours = request.ThursdayHours,
            FridayHours = request.FridayHours,
            SaturdayHours = request.SaturdayHours,
            SundayHours = request.SundayHours,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
        };

        _context.WorkSchedules.Add(workSchedule);
        await _context.SaveChangesAsync();
        return Ok(workSchedule);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkSchedule>> UpdateWorkSchedule(uint id, WorkScheduleUpsertRequest request)
    {
        WorkSchedule? workSchedule = await _context.WorkSchedules.FindAsync(id);
        if (workSchedule == null)
        {
            return NotFound();
        }

        if (!await CanManageAsync(workSchedule.UserId))
        {
            return Forbid();
        }

        if (!HasValidDayHours(request))
        {
            return BadRequest("Die Sollstunden je Wochentag müssen zwischen 0 und 24 liegen.");
        }

        workSchedule.MondayHours = request.MondayHours;
        workSchedule.TuesdayHours = request.TuesdayHours;
        workSchedule.WednesdayHours = request.WednesdayHours;
        workSchedule.ThursdayHours = request.ThursdayHours;
        workSchedule.FridayHours = request.FridayHours;
        workSchedule.SaturdayHours = request.SaturdayHours;
        workSchedule.SundayHours = request.SundayHours;
        workSchedule.EffectiveFrom = request.EffectiveFrom;
        workSchedule.EffectiveTo = request.EffectiveTo;

        await _context.SaveChangesAsync();
        return Ok(workSchedule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkSchedule(uint id)
    {
        WorkSchedule? workSchedule = await _context.WorkSchedules.FindAsync(id);
        if (workSchedule == null)
        {
            return NotFound();
        }

        if (!await CanManageAsync(workSchedule.UserId))
        {
            return Forbid();
        }

        _context.WorkSchedules.Remove(workSchedule);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static bool HasValidDayHours(WorkScheduleUpsertRequest request)
    {
        decimal[] hours =
        [
            request.MondayHours, request.TuesdayHours, request.WednesdayHours, request.ThursdayHours,
            request.FridayHours, request.SaturdayHours, request.SundayHours,
        ];
        return hours.All(h => h is >= 0 and <= 24);
    }

    private async Task<bool> CanManageAsync(uint targetUserId)
    {
        if (User.IsInRole("Superadmin"))
        {
            return true;
        }

        uint currentUserId = User.GetUserId();
        HashSet<uint> managedIds = await _managedUsersService.GetManagedUserIdsAsync(currentUserId);
        return managedIds.Contains(targetUserId);
    }
}
