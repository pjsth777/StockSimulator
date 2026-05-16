using Microsoft.AspNetCore.Mvc;
using StockSimulator.Infrastructure.Services.IServices;

namespace StockSimulator.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        var profile = await _profileService.GetProfileByUsernameAsync(username);

        if (profile == null)
        {
            return NotFound(new { Message = $"Profile for user '{username}' not found." });
        }

        return Ok(profile);
    }
}
