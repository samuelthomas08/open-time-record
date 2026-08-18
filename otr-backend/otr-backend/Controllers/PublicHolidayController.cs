using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PublicHolidayController : ControllerBase
{
    private readonly OtrDbContext _context;

    public PublicHolidayController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PublicHoliday>>> GetPublicHolidays()
    {
        return Ok(await _context.PublicHolidays.ToListAsync());
    }

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
