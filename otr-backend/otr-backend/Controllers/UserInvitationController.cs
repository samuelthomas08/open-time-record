using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Enums;
using otr_backend.Extensions;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

/// <summary>Creates and manages invitations for new users.</summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserInvitationController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly PermissionService _permissionService;

    public UserInvitationController(OtrDbContext context, IEmailSender emailSender, PermissionService permissionService)
    {
        _context = context;
        _emailSender = emailSender;
        _permissionService = permissionService;
    }

    /// <summary>Returns open invitations. Accepted invitations already appear as users and archived ones were dismissed on purpose, so neither is listed.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UserInvitation>>> GetInvitations()
    {
        // Invitations contain their redemption code, so listing them requires read permission.
        bool allowed = await _permissionService.HasPermissionAsync(User.GetUserId(), PermissionResource.Invitations, PermissionLevel.Read);
        if (!allowed)
        {
            return Forbid();
        }

        // Accepted invitations already show up as real users, and archived ones were
        // deliberately dismissed — neither belongs in the working list of open invites.
        List<UserInvitation> invitations = await _context.UserInvitations
            .Where(i => i.AcceptedAt == null && !i.IsArchived)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(invitations);
    }

    /// <summary>Hides an invitation from the list of open invitations.</summary>
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveInvitation(uint id)
    {
        bool allowed = await _permissionService.HasPermissionAsync(User.GetUserId(), PermissionResource.Invitations, PermissionLevel.Write);
        if (!allowed)
        {
            return Forbid();
        }

        UserInvitation? invitation = await _context.UserInvitations.FindAsync(id);
        if (invitation == null)
        {
            return NotFound();
        }

        invitation.IsArchived = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Creates an invitation valid for seven days and either e-mails the code or returns it to the caller.</summary>
    [HttpPost]
    public async Task<ActionResult<InviteUserResponse>> InviteUser(InviteUserRequest request)
    {
        // An invitation assigns an arbitrary role, so creating one must not be open to every user.
        bool allowed = await _permissionService.HasPermissionAsync(User.GetUserId(), PermissionResource.Invitations, PermissionLevel.Write);
        if (!allowed)
        {
            return Forbid();
        }

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

        UserInvitation invitation = new UserInvitation
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
