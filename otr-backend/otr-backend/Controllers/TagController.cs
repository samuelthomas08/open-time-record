using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly OtrDbContext _context;

    public TagController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetTags()
    {
        return Ok(await _context.Tags.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTagById(uint id)
    {
        Tag? tag = await _context.Tags.FindAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> AddTag(Tag tag)
    {
        if (tag == null)
        {
            return BadRequest();
        }

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
    }
}
