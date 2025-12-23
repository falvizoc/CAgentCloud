namespace CobranzaCloud.Application.Auth;

// ============================================================
// REQUEST DTOs
// ============================================================

public record LoginRequest(
    string Email,
    string Password
);

public record RegisterRequest(
    string Email,
    string Password,
    string Nombre,
    OrganizationRequest Organizacion
);

public record OrganizationRequest(
    string Nombre,
    string? Rfc = null
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record OAuthCallbackRequest(
    string Code,
    string RedirectUri
);

// ============================================================
// RESPONSE DTOs
// ============================================================

public record AuthResponse(
    UserResponse User,
    OrganizationSummaryResponse Organization,
    TokenPairResponse Tokens
);

public record UserResponse(
    Guid Id,
    string Email,
    string Nombre,
    string Role,
    Guid OrganizationId,
    List<string>? Permissions = null,
    DateTime? CreatedAt = null,
    DateTime? LastLogin = null
);

public record OrganizationSummaryResponse(
    Guid Id,
    string Nombre,
    string? Plan = null
);

public record TokenPairResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);
