using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

/// <summary>Manages public holidays.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PublicHolidayController : ControllerBase
{
    private readonly OtrDbContext _context;

    public PublicHolidayController(OtrDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns all public holidays.</summary>
    [HttpGet]
    public async Task<ActionResult<List<PublicHoliday>>> GetPublicHolidays()
    {
        return Ok(await _context.PublicHolidays.ToListAsync());
    }

    /// <summary>Returns a single public holiday or 404.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PublicHoliday>> GetPublicHolidayById(uint id)
    {
        PublicHoliday? publicHoliday = await _context.PublicHolidays.FindAsync(id);
        if (publicHoliday == null)
        {
            return NotFound();
        }

        return Ok(publicHoliday);
    }

    /// <summary>Creates a new public holiday.</summary>
    [HttpPost]
    public async Task<ActionResult<PublicHoliday>> AddPublicHoliday(PublicHoliday publicHoliday)
    {
        if (publicHoliday == null)
        {
            return BadRequest();
        }

        _context.PublicHolidays.Add(publicHoliday);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPublicHolidayById), new { id = publicHoliday.Id }, publicHoliday);
    }
}
