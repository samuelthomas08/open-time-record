using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;
using otr_backend.Models;
using otr_backend.Services;

namespace otr_backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IEmailSender _emailSender;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(OtrDbContext context, JwtTokenService jwtTokenService, IEmailSender emailSender)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailSender = emailSender;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        bool emailTaken = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            return Conflict("Diese E-Mail-Adresse wird bereits verwendet.");
        }

        bool isFirstUser = !await _context.Users.AnyAsync();

        UserInvitation? invitation = null;
        if (!string.IsNullOrWhiteSpace(request.InvitationCode))
        {
            invitation = await _context.UserInvitations.FirstOrDefaultAsync(i =>
                i.Email == request.Email && i.Code == request.InvitationCode && i.AcceptedAt == null);
            if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("Der Einladungscode ist ungültig oder abgelaufen.");
            }
        }

        bool autoVerified = isFirstUser || invitation != null;

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            HashedPassword = "",
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = autoVerified,
            ManagerId = invitation?.ManagerId,
        };
        user.HashedPassword = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (isFirstUser)
        {
            Role? superadminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Superadmin");
            if (superadminRole != null)
            {
                _context.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, RoleId = superadminRole.Id });
            }
        }

        if (invitation != null)
        {
            invitation.AcceptedAt = DateTime.UtcNow;
            _context.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, RoleId = invitation.RoleId });

            if (invitation.TeamId.HasValue)
            {
                _context.UserTeamMappings.Add(new UserTeamMapping { UserId = user.Id, TeamId = invitation.TeamId.Value });
            }
        }

        await _context.SaveChangesAsync();

        if (autoVerified)
        {
            return Ok(new RegisterResponse { Auth = await BuildAuthResponse(user) });
        }

        string code = VerificationCodeGenerator.Generate();
        _context.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            UserId = user.Id,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        });
        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendAsync(
                user.Email,
                "Bestätige deine E-Mail-Adresse",
                $"Dein Bestätigungscode für Open Time Record lautet: {code}\n\nDer Code ist 24 Stunden gültig.");
        }
        catch (Exception)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return StatusCode(503,
                "Die Bestätigungs-E-Mail konnte nicht versendet werden. Bitte wende dich an einen Administrator, um SMTP einzurichten.");
        }

        return Ok(new RegisterResponse
        {
            Message = "Registrierung erfolgreich. Bitte bestätige deine E-Mail-Adresse mit dem zugesendeten Code.",
        });
    }

    [HttpPost("activate")]
    public async Task<ActionResult<AuthResponse>> Activate(ActivateRequest request)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return NotFound("Kein Konto mit dieser E-Mail-Adresse gefunden.");
        }

        if (user.IsEmailVerified)
        {
            return Ok(await BuildAuthResponse(user));
        }

        EmailVerificationCode? verification = await _context.EmailVerificationCodes
            .Where(c => c.UserId == user.Id && c.Code == request.Code)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (verification == null || verification.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("Der Code ist ungültig oder abgelaufen.");
        }

        user.IsEmailVerified = true;
        _context.EmailVerificationCodes.Remove(verification);
        await _context.SaveChangesAsync();

        return Ok(await BuildAuthResponse(user));
    }

    [HttpPost("resend-activation")]
    public async Task<ActionResult<MessageResponse>> ResendActivation(ResendActivationRequest request)
    {
        var genericResponse = new MessageResponse
        {
            Message = "Falls ein Konto mit dieser E-Mail existiert und noch nicht bestätigt ist, wurde ein neuer Code versendet.",
        };

        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || user.IsEmailVerified)
        {
            return Ok(genericResponse);
        }

        string code = VerificationCodeGenerator.Generate();
        _context.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            UserId = user.Id,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        });
        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendAsync(
                user.Email,
                "Neuer Bestätigungscode",
                $"Dein neuer Bestätigungscode lautet: {code}\n\nDer Code ist 24 Stunden gültig.");
        }
        catch (Exception)
        {
            return StatusCode(503, "Die E-Mail konnte nicht versendet werden.");
        }

        return Ok(genericResponse);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !user.IsActive)
        {
            return Unauthorized("E-Mail oder Passwort ist falsch.");
        }

        PasswordVerificationResult result =
            _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("E-Mail oder Passwort ist falsch.");
        }

        if (!user.IsEmailVerified)
        {
            return StatusCode(403, "Bitte bestätige zuerst deine E-Mail-Adresse.");
        }

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(await BuildAuthResponse(user));
    }

    private async Task<AuthResponse> BuildAuthResponse(User user)
    {
        List<string> roles = await _context.UserRoleMappings
            .Where(m => m.UserId == user.Id)
            .Select(m => m.Role.RoleName)
            .ToListAsync();

        return new AuthResponse
        {
            Token = _jwtTokenService.GenerateToken(user, roles),
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = roles,
        };
    }
}
