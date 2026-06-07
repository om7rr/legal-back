using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LegalPlatform.Modules.Identity.Domain;
using Microsoft.IdentityModel.Tokens;

namespace LegalPlatform.Modules.Identity.Auth;

public sealed record AccessToken(string Token, int ExpiresInSeconds);

public interface IJwtTokenIssuer
{
    AccessToken Create(AppUser user);
}

/// <summary>Issues HMAC-signed JWT access tokens carrying the verified identity claims (ADR-0006).</summary>
public sealed class JwtTokenIssuer : IJwtTokenIssuer
{
    private readonly JwtOptions _options;

    public JwtTokenIssuer(JwtOptions options) => _options = options;

    public AccessToken Create(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("name", user.FullName),
            new Claim("national_id", user.NationalId),
        };

        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(jwt, _options.AccessTokenMinutes * 60);
    }
}
