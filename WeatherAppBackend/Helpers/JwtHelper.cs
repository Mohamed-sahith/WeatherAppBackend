using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WeatherAppBackend.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtHelper> _logger;

    public JwtHelper(IConfiguration config, ILogger<JwtHelper> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string GenerateToken(string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, email) // Add name claim for User.Identity.Name
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("Generated JWT for {Email}", email);
        return tokenString;
    }
}