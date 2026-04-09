using CarwashApi.DTO.Request;
using CarwashApi.DTO.Response;
using CarwashApi.Services;
using CarwashApi.Data;
using CarwashApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarwashApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _appDbcontext;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        AppDbContext appDbcontext, 
        PasswordService passwordService, 
        JwtTokenService jwtTokenService) 
        {
            _appDbcontext = appDbcontext;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
        }

    [HttpPost("sign-up")]
    public async Task<ActionResult<AuthResponseDto>> SignUpAsync([FromBody] SignUpRequestDto request) 
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await _appDbcontext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null) {
            return BadRequest(new { message = "User already exists." });
        }

        var (hash, salt) = _passwordService.CreatePasswordHash(request.Password);

        var user = new User {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = string.Empty,
            LastName = string.Empty,
            Phone = string.Empty,
            Role = "user",
            ProfileImageUrl = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            PasswordHash = hash,
            PasswordSalt = salt,
        };

        _appDbcontext.Users.Add(user);
        await _appDbcontext.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto {
            Token = token,
            Email = user.Email,
            Role = user.Role,
        });
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<AuthResponseDto>> SignInAsync([FromBody] SignInRequestDto request) {
        var email = request.Email.Trim().ToLower();

        var user = await _appDbcontext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !user.IsActive) {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var passwordMatch = _passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!passwordMatch) {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto {
            Token = token,
            Email = user.Email,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponseDto>> GetMeAsync() {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId)) {
            return Unauthorized(new { message = "Unauthorized." });
        }

        var user = await _appDbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { message = "User not found." });

        return Ok(new UserResponseDto {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
        });
    }

    [Authorize]
    [HttpGet("user/{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetUserByIdAsync(Guid id) {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId)) {
            return Unauthorized(new { message = "Unauthorized." });
        }

        var user = await _appDbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound(new { message = "User not found." });

        return Ok(new UserResponseDto {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
        });
    }

    
    [Authorize]
    [HttpPatch("update-profile")]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfileAsync([FromBody] UpdateProfileRequestDto request) {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if(string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId)) {
            return Unauthorized(new { message = "Unauthorized." });
        }
       
        var user = await _appDbcontext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return NotFound(new { message = "User not found." });

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.ProfileImageUrl = request.ProfileImageUrl;

        await _appDbcontext.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully." });
    }
        
    [Authorize]
    [HttpPatch("update-password")]
    public async Task<ActionResult<AuthResponseDto>> UpdatePasswordAsync() {
        return Ok(new { message = "Password updated successfully." });
    }
        
    [HttpPost("sign-out")]
    public async Task<ActionResult<AuthResponseDto>> SignOutAsync() {
        return Ok(new { message = "Signed out successfully." });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshTokenAsync() {
        return Ok(new { message = "Refresh token successfully." });
    }
}