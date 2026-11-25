using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clevora.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clevora.Infrastructure.Services;

public class JwtTokenService(IConfiguration config)
{
    public string GenerateToken(ClevoraUser user, IEnumerable<string> roles)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured.");
        var issuer = config["Jwt:Issuer"] ?? "clevora";
        var audience = config["Jwt:Audience"] ?? "clevora-clients";
        var expiryMinutes = int.TryParse(config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        var secKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName ?? user.UserName ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
