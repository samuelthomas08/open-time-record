using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Models;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientController : ControllerBase
{
    private readonly OtrDbContext _context;

    public ClientController(OtrDbContext context)
    {
        this._context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Client>>> GetClients()
    {
        return Ok(await _context.Clients.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClientById(uint id)
    {
        Client? client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<Client>> AddClient(Client client)
    {
        if (client == null)
        {
            return BadRequest();
        }

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetClientById), new { id = client.Id }, client);
    }
}
