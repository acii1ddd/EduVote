using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EduVote.DAL.Postgresql.Models.Roles;
using Microsoft.IdentityModel.Tokens;

namespace EduVote.API.Services.Auth;

public class JwtAccessTokenGenerator(IConfiguration config) 
    : ITokenGenerator
{
    public string GenerateAccessToken(Guid userId, Role role)
    {
        var issuer = config["AuthSettings:Issuer"];
        var audience = config["AuthSettings:Audience"];
        var lifetime = int.Parse(config["AuthSettings:Lifetime"] ?? "60");
        var secret = config["AuthSettings:Secret"];
        
        // дата окончания срока жизни токена
        var expires = DateTime.UtcNow.AddMinutes(lifetime);

        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role?.Name ?? string.Empty)
        };
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var securityToken = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(securityToken);
    }
    
    // public string GenerateSecureToken()
    // {
    //     var bytes = new byte[32];
    //
    //     using var generator = RandomNumberGenerator.Create();
    //     
    //     generator.GetBytes(bytes);
    //     
    //     return Convert.ToBase64String(bytes);
    // }
}