using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;

namespace otr_backend.Controllers;

[Authorize(Roles = "Superadmin")]
[Route("api/[controller]")]
[ApiController]
public class SmtpSettingsController : ControllerBase
{
    private readonly OtrDbContext _context;
    private readonly IDataProtector _protector;

    public SmtpSettingsController(OtrDbContext context, IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _protector = dataProtectionProvider.CreateProtector("SmtpSettings.Password");
    }

    [HttpGet]
    public async Task<ActionResult<SmtpSettingsResponse>> GetSettings()
    {
        Models.SmtpSettings? settings = await _context.SmtpSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            return Ok(new SmtpSettingsResponse { IsConfigured = false });
        }

        return Ok(ToResponse(settings));
    }

    [HttpPut]
    public async Task<ActionResult<SmtpSettingsResponse>> UpdateSettings(SmtpSettingsRequest request)
    {
        Models.SmtpSettings? settings = await _context.SmtpSettings.FirstOrDefaultAsync();
        bool isNew = settings == null;

        if (isNew && string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Passwort ist beim erstmaligen Einrichten erforderlich.");
        }

        settings ??= new Models.SmtpSettings();
        if (isNew)
        {
            _context.SmtpSettings.Add(settings);
        }

        settings.Host = request.Host;
        settings.Port = request.Port;
        settings.Username = request.Username;
        settings.FromAddress = request.FromAddress;
        settings.FromName = request.FromName;
        settings.UseSsl = request.UseSsl;
        settings.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Password))
        {
            settings.EncryptedPassword = _protector.Protect(request.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(ToResponse(settings));
    }

    private static SmtpSettingsResponse ToResponse(Models.SmtpSettings settings)
    {
        return new SmtpSettingsResponse
        {
            IsConfigured = true,
            Host = settings.Host,
            Port = settings.Port,
            Username = settings.Username,
            FromAddress = settings.FromAddress,
            FromName = settings.FromName,
            UseSsl = settings.UseSsl,
        };
    }
}
