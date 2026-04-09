using CarwashApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarwashApi.Services;

public class JwtTokenService {
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) {
        _configuration = configuration;
    }

    public string GenerateToken(User user) {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var secretKey = jwtSection["SecretKey"];
        var tokenExpirationMinutes = jwtSection["TokenExpirationMinutes"];

        if (string.IsNullOrEmpty(issuer) || 
        string.IsNullOrEmpty(audience) || 
        string.IsNullOrEmpty(secretKey)) {
            throw new InvalidOperationException("Jwt configuration is missing in appsettings.json.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var minutes = double.TryParse(tokenExpirationMinutes, out var expirationMinutes) ? expirationMinutes : 60;

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
        
    }
    
}