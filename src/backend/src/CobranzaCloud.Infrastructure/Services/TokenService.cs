using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CobranzaCloud.Application.Auth;
using CobranzaCloud.Core.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CobranzaCloud.Infrastructure.Services;

/// <summary>
/// JWT token generation service
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", user.Nombre),
            new("org_id", user.OrganizationId.ToString()),
            new("role", user.Role.ToString().ToLower()),
        };

        // Add permissions based on role
        var permissions = GetPermissionsForRole(user.Role);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public bool ValidateRefreshToken(string token)
    {
        // Basic validation - actual token lookup happens in the handler
        return !string.IsNullOrWhiteSpace(token) && token.Length >= 32;
    }

    private static List<string> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Owner => new List<string>
            {
                "users:*", "cartera:*", "clientes:*", "connectors:*", "settings:*", "billing:*"
            },
            UserRole.Admin => new List<string>
            {
                "users:create", "users:read", "users:update", "users:delete",
                "cartera:*", "clientes:*", "connectors:*", "settings:*"
            },
            UserRole.Manager => new List<string>
            {
                "users:read", "cartera:*", "clientes:*", "connectors:read"
            },
            UserRole.Collector => new List<string>
            {
                "cartera:read", "cartera:write", "clientes:read", "clientes:contact"
            },
            UserRole.Viewer => new List<string>
            {
                "cartera:read", "clientes:read"
            },
            _ => new List<string>()
        };
    }
}
