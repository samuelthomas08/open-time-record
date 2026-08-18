using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RuleController : ControllerBase
{
    private readonly OtrDbContext _context;

    public RuleController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Rule>>> GetRules()
    {
        return Ok(await _context.Rules.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Rule>> GetRuleById(uint id)
    {
        Rule? rule = await _context.Rules.FindAsync(id);
        if (rule == null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    [HttpPost]
    public async Task<ActionResult<Rule>> AddRule(Rule rule)
    {
        if (rule == null)
        {
            return BadRequest();
        }

        _context.Rules.Add(rule);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }
}
