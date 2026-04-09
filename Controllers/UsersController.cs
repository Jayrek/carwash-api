using CarwashApi.Data;
using CarwashApi.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarwashApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public UsersController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ActiveUserWithDeviceTokensResponseDto>>> GetActiveUsersAsync()
    {
        var users = await _appDbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive && u.Role == "user")
            .OrderBy(u => u.CreatedAt)
            .Select(u => new ActiveUserWithDeviceTokensResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                ProfileImageUrl = u.ProfileImageUrl
            })
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var tokensByUser = await _appDbContext.UserDevices
            .AsNoTracking()
            .Where(d => d.IsActive && userIds.Contains(d.UserId))
            .GroupBy(d => d.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                DeviceTokens = g.Select(d => d.DeviceToken).Distinct().ToList()
            })
            .ToDictionaryAsync(x => x.UserId, x => x.DeviceTokens);

        foreach (var user in users)
        {
            user.DeviceTokens = tokensByUser.TryGetValue(user.Id, out var tokens) ? tokens : [];
        }

        return Ok(users);
    }
}
