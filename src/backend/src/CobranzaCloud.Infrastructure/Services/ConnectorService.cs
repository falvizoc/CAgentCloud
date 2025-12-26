using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CobranzaCloud.Application.Auth;
using CobranzaCloud.Application.Connectors;
using CobranzaCloud.Core.Entities;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CobranzaCloud.Infrastructure.Services;

/// <summary>
/// Service for managing connector registration and lifecycle
/// </summary>
public class ConnectorService : IConnectorService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _settings;
    private const int LinkCodeLength = 6;
    private const int LinkCodeExpirationMinutes = 15;
    private const int ConnectorTokenExpirationHours = 24;
    private const int ConnectorRefreshTokenExpirationDays = 30;

    public ConnectorService(AppDbContext db, IOptions<JwtSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<LinkCodeResponse> GenerateLinkCodeAsync(
        Guid organizationId,
        Guid userId,
        GenerateLinkCodeRequest request,
        CancellationToken ct = default)
    {
        var code = GenerateAlphanumericCode(LinkCodeLength);
        var expiresAt = DateTime.UtcNow.AddMinutes(LinkCodeExpirationMinutes);

        var linkCode = new LinkCode
        {
            Id = Guid.NewGuid(),
            Code = code,
            ExpiresAt = expiresAt,
            Used = false,
            MachineFingerprint = request.MachineFingerprint,
            ConnectorName = request.ConnectorName,
            ConnectorVersion = request.ConnectorVersion,
            OrganizationId = organizationId,
            CreatedByUserId = userId
        };

        _db.LinkCodes.Add(linkCode);
        await _db.SaveChangesAsync(ct);

        return new LinkCodeResponse(code, expiresAt);
    }

    public async Task<RegisterConnectorResponse?> RegisterConnectorAsync(
        RegisterConnectorRequest request,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        // Find valid link code
        var linkCode = await _db.LinkCodes
            .Where(l => l.Code == request.LinkCode
                && !l.Used
                && l.ExpiresAt > DateTime.UtcNow
                && l.MachineFingerprint == request.MachineFingerprint)
            .FirstOrDefaultAsync(ct);

        if (linkCode == null)
        {
            return null;
        }

        // Create connector
        var connector = new Connector
        {
            Id = Guid.NewGuid(),
            Nombre = request.ConnectorName,
            Version = request.ConnectorVersion,
            Tipo = request.Tipo,
            MachineFingerprint = request.MachineFingerprint,
            Status = ConnectorStatus.Online,
            OrganizationId = linkCode.OrganizationId,
            LastHeartbeat = DateTime.UtcNow
        };

        _db.Connectors.Add(connector);

        // Mark link code as used
        linkCode.Used = true;

        // Generate tokens
        var accessToken = GenerateConnectorAccessToken(connector);
        var refreshToken = GenerateConnectorRefreshToken(connector.Id, ipAddress);

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return new RegisterConnectorResponse(
            connector.Id,
            accessToken,
            refreshToken.Token,
            ConnectorTokenExpirationHours * 3600,
            new SyncConfig(15, 5)
        );
    }

    public async Task<HeartbeatResponse> RecordHeartbeatAsync(
        Guid connectorId,
        HeartbeatRequest request,
        CancellationToken ct = default)
    {
        var connector = await _db.Connectors.FindAsync([connectorId], ct);

        if (connector != null)
        {
            connector.LastHeartbeat = DateTime.UtcNow;
            connector.Status = request.Status switch
            {
                "healthy" => ConnectorStatus.Online,
                "degraded" => ConnectorStatus.Online,
                "error" => ConnectorStatus.Error,
                _ => ConnectorStatus.Online
            };

            await _db.SaveChangesAsync(ct);
        }

        return new HeartbeatResponse(
            Ack: true,
            ServerTime: DateTime.UtcNow,
            Commands: null
        );
    }

    public string GenerateConnectorAccessToken(Connector connector)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, connector.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", "connector"),
            new("org_id", connector.OrganizationId.ToString()),
            new("machine_id", connector.MachineFingerprint),
            new("version", connector.Version),
            new("permissions", "sync:write"),
            new("permissions", "heartbeat:write"),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(ConnectorTokenExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateConnectorRefreshToken(Guid connectorId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(ConnectorRefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ConnectorId = connectorId
        };
    }

    public async Task<RegisterConnectorResponse?> RefreshConnectorTokenAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var token = await _db.RefreshTokens
            .Include(r => r.Connector)
            .Where(r => r.Token == refreshToken
                && r.ConnectorId != null
                && r.RevokedAt == null
                && r.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(ct);

        if (token?.Connector == null)
        {
            return null;
        }

        // Revoke old token
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;

        // Generate new tokens
        var newAccessToken = GenerateConnectorAccessToken(token.Connector);
        var newRefreshToken = GenerateConnectorRefreshToken(token.Connector.Id, ipAddress);

        token.ReplacedByToken = newRefreshToken.Token;
        _db.RefreshTokens.Add(newRefreshToken);

        await _db.SaveChangesAsync(ct);

        return new RegisterConnectorResponse(
            token.Connector.Id,
            newAccessToken,
            newRefreshToken.Token,
            ConnectorTokenExpirationHours * 3600,
            new SyncConfig(15, 5)
        );
    }

    private static string GenerateAlphanumericCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(result);
    }
}
