using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;

namespace otr_backend.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AppSettingsController : ControllerBase
{
    private readonly OtrDbContext _context;

    public AppSettingsController(OtrDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<AppSettingsResponse>> GetSettings()
    {
        Models.AppSettings? settings = await _context.AppSettings.FirstOrDefaultAsync();
        return Ok(ToResponse(settings));
    }

    [Authorize(Roles = "Superadmin")]
    [HttpPut]
    public async Task<ActionResult<AppSettingsResponse>> UpdateSettings(AppSettingsRequest request)
    {
        Models.AppSettings? settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new Models.AppSettings();
            _context.AppSettings.Add(settings);
        }

        settings.ProfilePicturesEnabled = request.ProfilePicturesEnabled;
        settings.BreakReasonRequired = request.BreakReasonRequired;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(settings));
    }

    private static AppSettingsResponse ToResponse(Models.AppSettings? settings)
    {
        return new AppSettingsResponse
        {
            ProfilePicturesEnabled = settings?.ProfilePicturesEnabled ?? false,
            BreakReasonRequired = settings?.BreakReasonRequired ?? false,
        };
    }
}
