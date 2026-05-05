using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymSaaS.Domain.Enums;
using GymSaaS.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymSaaS.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _opt = options.Value;

    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        string email,
        Guid tenantId,
        string tenantSlug,
        TenantRole role)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_opt.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("tenant_id", tenantId.ToString()),
            new("tenant_slug", tenantSlug),
            new("tenant_role", role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }
}
