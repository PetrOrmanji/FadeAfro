using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FadeAfro.Domain.Entities;
using FadeAfro.Domain.Services;
using FadeAfro.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FadeAfro.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private const string TelegramIdClaimType = "telegramId";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(TelegramIdClaimType, user.TelegramId.ToString()),
            new(ClaimTypes.GivenName, user.FirstName),
        };

        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
