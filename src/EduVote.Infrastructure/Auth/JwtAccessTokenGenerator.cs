using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduVote.Application.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EduVote.Infrastructure.Auth;

public sealed class JwtAccessTokenGenerator(IConfiguration config) : IAccessTokenGenerator
{
    public string GenerateAccessToken(Guid userId, string roleName)
    {
        var issuer = config["AuthSettings:Issuer"];
        var audience = config["AuthSettings:Audience"];
        var lifetime = int.Parse(config["AuthSettings:Lifetime"] ?? "60");
        var secret = config["AuthSettings:Secret"];

        var expires = DateTime.UtcNow.AddMinutes(lifetime);
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, roleName)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}
