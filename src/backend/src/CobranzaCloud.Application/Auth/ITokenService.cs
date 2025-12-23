using CobranzaCloud.Core.Entities;

namespace CobranzaCloud.Application.Auth;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    RefreshToken GenerateRefreshToken(string? ipAddress = null);

    /// <summary>
    /// Validates a refresh token
    /// </summary>
    bool ValidateRefreshToken(string token);
}
