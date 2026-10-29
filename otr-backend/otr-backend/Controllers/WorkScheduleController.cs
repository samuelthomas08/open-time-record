using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Extensions;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

/// <summary>Manages the target working hours of users. Callers may manage the schedules of the users they are responsible for.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class WorkScheduleController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly ManagedUsersService _managedUsersService;

    public WorkScheduleController(OtrDbContext context, ManagedUsersService managedUsersService)
    {
        _context = context;
        _managedUsersService = managedUsersService;
    }

    /// <summary>Returns the schedules of all users the caller manages (everyone but themselves for Superadmins), so the frontend can group them client-side. Mirrors the "team" endpoint of the time entry controller.</summary>
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

    /// <summary>Returns the caller's own schedule periods. No manager check is needed; everyone may see their own.</summary>
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

    /// <summary>Creates a schedule period for a user the caller is allowed to manage.</summary>
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

        WorkSchedule workSchedule = new WorkSchedule
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

    /// <summary>Replaces the hours and validity period of a schedule.</summary>
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

    /// <summary>Deletes a schedule period.</summary>
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

    /// <summary>Checks that every weekday value lies between 0 and 24 hours.</summary>
    private static bool HasValidDayHours(WorkScheduleUpsertRequest request)
    {
        decimal[] hours =
        [
            request.MondayHours, request.TuesdayHours, request.WednesdayHours, request.ThursdayHours,
            request.FridayHours, request.SaturdayHours, request.SundayHours,
        ];
        return hours.All(h => h is >= 0 and <= 24);
    }

    /// <summary>Whether the caller may change the schedule of <paramref name="targetUserId"/>: Superadmins, managers of that user, and users who have no manager themselves.</summary>
    private async Task<bool> CanManageAsync(uint targetUserId)
    {
        if (User.IsInRole("Superadmin"))
        {
            return true;
        }

        uint currentUserId = User.GetUserId();

        // Nobody manages this person, so nobody but themselves could ever set their schedule.
        if (targetUserId == currentUserId)
        {
            uint? managerId = await _context.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.ManagerId)
                .FirstOrDefaultAsync();
            if (managerId == null)
            {
                return true;
            }
        }

        HashSet<uint> managedIds = await _managedUsersService.GetManagedUserIdsAsync(currentUserId);
        return managedIds.Contains(targetUserId);
    }
}
