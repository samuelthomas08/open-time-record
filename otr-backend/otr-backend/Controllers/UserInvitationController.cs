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
public class UserInvitationController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly IEmailSender _emailSender;

    public UserInvitationController(OtrDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserInvitation>>> GetInvitations()
    {
        List<UserInvitation> invitations = await _context.UserInvitations
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(invitations);
    }

    [HttpPost]
    public async Task<ActionResult<InviteUserResponse>> InviteUser(InviteUserRequest request)
    {
        bool alreadyUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (alreadyUser)
        {
            return Conflict("Diese E-Mail-Adresse ist bereits registriert.");
        }

        bool roleExists = await _context.Roles.AnyAsync(r => r.Id == request.RoleId);
        if (!roleExists)
        {
            return BadRequest("Die ausgewählte Rolle existiert nicht.");
        }

        if (request.TeamId.HasValue && !await _context.Teams.AnyAsync(t => t.Id == request.TeamId))
        {
            return BadRequest("Das ausgewählte Team existiert nicht.");
        }

        if (request.ManagerId.HasValue && !await _context.Users.AnyAsync(u => u.Id == request.ManagerId))
        {
            return BadRequest("Der ausgewählte Vorgesetzte existiert nicht.");
        }

        var invitation = new UserInvitation
        {
            Email = request.Email,
            Code = VerificationCodeGenerator.Generate(),
            InvitedByUserId = User.GetUserId(),
            RoleId = request.RoleId,
            TeamId = request.TeamId,
            ManagerId = request.ManagerId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        _context.UserInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        if (!request.SendEmail)
        {
            return Ok(new InviteUserResponse { Message = "Einladungslink wurde erstellt.", InvitationCode = invitation.Code });
        }

        try
        {
            await _emailSender.SendAsync(
                request.Email,
                "Einladung zu Open Time Record",
                $"Du wurdest zu Open Time Record eingeladen. Registriere dich mit deinem Einladungscode: {invitation.Code}\n\nDer Code ist 7 Tage gültig.");
        }
        catch (Exception)
        {
            _context.UserInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
            return StatusCode(503, "Die Einladung konnte nicht versendet werden. Ist SMTP konfiguriert?");
        }

        return Ok(new InviteUserResponse { Message = "Einladung wurde versendet." });
    }
}
