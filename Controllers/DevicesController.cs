using CarwashApi.Data;
using CarwashApi.DTO.Request;
using CarwashApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarwashApi.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _appDbcontext;

    public DevicesController(AppDbContext appDbcontext)
    {
        _appDbcontext = appDbcontext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterDeviceRequest request)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId)) {
            return Unauthorized(new { message = "Unauthorized." });
        }

        var existing = await _appDbcontext.UserDevices
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceId == request.DeviceId);

        if (existing != null)
        {
            existing.DeviceToken = request.DeviceToken;
            existing.Platform = request.Platform;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _appDbcontext.UserDevices.Add(new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceToken = request.DeviceToken,
                Platform = request.Platform,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _appDbcontext.SaveChangesAsync();
        return Ok();
    }
}