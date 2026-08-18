using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly OtrDbContext _context;

    public NotificationController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetNotifications()
    {
        return Ok(await _context.Notifications.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotificationById(uint id)
    {
        Notification? notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult<Notification>> AddNotification(Notification notification)
    {
        if (notification == null)
        {
            return BadRequest();
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetNotificationById), new { id = notification.Id }, notification);
    }
}
