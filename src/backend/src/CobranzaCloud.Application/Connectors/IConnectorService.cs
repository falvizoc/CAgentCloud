using CobranzaCloud.Core.Entities;

namespace CobranzaCloud.Application.Connectors;

/// <summary>
/// Service for managing connector registration and lifecycle
/// </summary>
public interface IConnectorService
{
    /// <summary>
    /// Generates a 6-character link code for connector registration
    /// </summary>
    Task<LinkCodeResponse> GenerateLinkCodeAsync(
        Guid organizationId,
        Guid userId,
        GenerateLinkCodeRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Registers a new connector using a valid link code
    /// </summary>
    Task<RegisterConnectorResponse?> RegisterConnectorAsync(
        RegisterConnectorRequest request,
        string? ipAddress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Records a heartbeat from a connector
    /// </summary>
    Task<HeartbeatResponse> RecordHeartbeatAsync(
        Guid connectorId,
        HeartbeatRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a JWT access token for a connector
    /// </summary>
    string GenerateConnectorAccessToken(Connector connector);

    /// <summary>
    /// Generates a refresh token for a connector
    /// </summary>
    RefreshToken GenerateConnectorRefreshToken(Guid connectorId, string? ipAddress = null);

    /// <summary>
    /// Refreshes a connector's access token
    /// </summary>
    Task<RegisterConnectorResponse?> RefreshConnectorTokenAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken ct = default);
}
