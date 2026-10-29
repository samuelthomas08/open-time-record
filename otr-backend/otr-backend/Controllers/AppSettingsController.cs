using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using otr_backend.Data;
using otr_backend.Dtos;

namespace otr_backend.Controllers;

/// <summary>Reads and updates the instance-wide feature toggles.</summary>
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

    /// <summary>Returns the feature toggles; both are off if nothing was saved yet.</summary>
    [HttpGet]
    public async Task<ActionResult<AppSettingsResponse>> GetSettings()
    {
        Models.AppSettings? settings = await _context.AppSettings.FirstOrDefaultAsync();
        return Ok(ToResponse(settings));
    }

    /// <summary>Saves the feature toggles, creating the settings row on first use.</summary>
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

    /// <summary>Maps the stored settings to the API response, defaulting missing values to <c>false</c>.</summary>
    private static AppSettingsResponse ToResponse(Models.AppSettings? settings)
    {
        return new AppSettingsResponse
        {
            ProfilePicturesEnabled = settings?.ProfilePicturesEnabled ?? false,
            BreakReasonRequired = settings?.BreakReasonRequired ?? false,
        };
    }
}
