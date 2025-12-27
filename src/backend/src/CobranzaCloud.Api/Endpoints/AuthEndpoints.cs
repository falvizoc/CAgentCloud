using System.Security.Claims;
using CobranzaCloud.Application.Auth;
using CobranzaCloud.Core;
using CobranzaCloud.Core.Entities;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Authentication endpoints
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithDescription("Register a new user and organization")
            .Produces<AuthResponse>(201)
            .Produces<ProblemDetails>(400);

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithDescription("Authenticate with email and password")
            .Produces<AuthResponse>(200)
            .Produces<ProblemDetails>(401);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithDescription("Refresh access token using refresh token")
            .Produces<RefreshTokenResponse>(200)
            .Produces<ProblemDetails>(401);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithDescription("Revoke refresh token")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(401);

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithDescription("Get current authenticated user")
            .RequireAuthorization()
            .Produces<UserResponse>(200)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        AppDbContext db,
        ITokenService tokenService,
        HttpContext httpContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        // Check if email already exists
        var existingUser = await db.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), ct);

        if (existingUser != null)
        {
            return Results.Problem(
                title: "Email already registered",
                detail: "An account with this email already exists",
                statusCode: 400
            );
        }

        // Create organization
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Nombre = request.Organizacion.Nombre,
            Rfc = request.Organizacion.Rfc,
            Plan = OrganizationPlan.Free,
            CreatedAt = DateTime.UtcNow
        };

        // Create user (owner of the organization)
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            Nombre = request.Nombre,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Owner,
            AuthProvider = AuthProvider.Email,
            EmailConfirmed = true, // FRICTIONLESS: Skip email verification for MVP
            IsActive = true,
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken(httpContext.Connection.RemoteIpAddress?.ToString());
        refreshToken.UserId = user.Id;

        user.RefreshTokens.Add(refreshToken);
        user.LastLoginAt = DateTime.UtcNow;

        db.Set<Organization>().Add(organization);
        db.Set<User>().Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("New user registered: {Email} for organization {OrgName}", user.Email, organization.Nombre);

        var response = new AuthResponse(
            User: new UserResponse(
                Id: user.Id,
                Email: user.Email,
                Nombre: user.Nombre,
                Role: user.Role.ToString().ToLower(),
                OrganizationId: user.OrganizationId,
                CreatedAt: user.CreatedAt
            ),
            Organization: new OrganizationSummaryResponse(
                Id: organization.Id,
                Nombre: organization.Nombre,
                Plan: organization.Plan.ToString().ToLower()
            ),
            Tokens: new TokenPairResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                ExpiresIn: 900 // 15 minutes
            )
        );

        return Results.Created($"/api/auth/me", response);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        AppDbContext db,
        ITokenService tokenService,
        HttpContext httpContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var user = await db.Set<User>()
            .Include(u => u.Organization)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), ct);

        if (user == null)
        {
            return Results.Problem(
                title: "Invalid credentials",
                detail: "Email or password is incorrect",
                statusCode: 401
            );
        }

        // Check lockout
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            return Results.Problem(
                title: "Account locked",
                detail: $"Account is locked. Try again after {user.LockoutEnd:HH:mm}",
                statusCode: 401
            );
        }

        // Verify password
        if (user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            // Lock after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                logger.LogWarning("User {Email} locked out after {Attempts} failed attempts", user.Email, user.FailedLoginAttempts);
            }

            await db.SaveChangesAsync(ct);

            return Results.Problem(
                title: "Invalid credentials",
                detail: "Email or password is incorrect",
                statusCode: 401
            );
        }

        if (!user.IsActive)
        {
            return Results.Problem(
                title: "Account disabled",
                detail: "Your account has been disabled. Contact support.",
                statusCode: 401
            );
        }

        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken(httpContext.Connection.RemoteIpAddress?.ToString());
        refreshToken.UserId = user.Id;

        // Revoke old refresh tokens (keep only last 5)
        var oldTokens = user.RefreshTokens
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(5)
            .ToList();

        foreach (var token in oldTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Replaced by new login";
        }

        // Add to DbSet directly to avoid tracking issues
        db.Set<RefreshToken>().Add(refreshToken);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User logged in: {Email}", user.Email);

        var response = new AuthResponse(
            User: new UserResponse(
                Id: user.Id,
                Email: user.Email,
                Nombre: user.Nombre,
                Role: user.Role.ToString().ToLower(),
                OrganizationId: user.OrganizationId,
                LastLogin: user.LastLoginAt
            ),
            Organization: new OrganizationSummaryResponse(
                Id: user.Organization.Id,
                Nombre: user.Organization.Nombre,
                Plan: user.Organization.Plan.ToString().ToLower()
            ),
            Tokens: new TokenPairResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                ExpiresIn: 900
            )
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        AppDbContext db,
        ITokenService tokenService,
        HttpContext httpContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var refreshToken = await db.Set<RefreshToken>()
            .Include(t => t.User)
            .ThenInclude(u => u.Organization)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);

        if (refreshToken == null)
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Refresh token is invalid",
                statusCode: 401
            );
        }

        if (!refreshToken.IsActive)
        {
            // Possible token reuse attack - revoke all tokens
            if (refreshToken.IsRevoked)
            {
                var user = refreshToken.User;
                var allTokens = await db.Set<RefreshToken>()
                    .Where(t => t.UserId == user.Id && t.IsActive)
                    .ToListAsync(ct);

                foreach (var token in allTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.ReasonRevoked = "Attempted reuse of revoked ancestor token";
                }

                await db.SaveChangesAsync(ct);
                logger.LogWarning("Token reuse detected for user {Email}. All tokens revoked.", user.Email);
            }

            return Results.Problem(
                title: "Invalid token",
                detail: "Refresh token has expired or been revoked",
                statusCode: 401
            );
        }

        var currentUser = refreshToken.User;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        // Revoke old token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = "Replaced by new token";

        // Generate new tokens
        var newAccessToken = tokenService.GenerateAccessToken(currentUser);
        var newRefreshToken = tokenService.GenerateRefreshToken(ipAddress);
        newRefreshToken.UserId = currentUser.Id;

        refreshToken.ReplacedByToken = newRefreshToken.Token;
        db.Set<RefreshToken>().Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new RefreshTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken.Token,
            ExpiresIn: 900
        ));
    }

    private static async Task<IResult> Logout(
        [FromBody] RefreshTokenRequest request,
        AppDbContext db,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var refreshToken = await db.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = httpContext.Connection.RemoteIpAddress?.ToString();
            refreshToken.ReasonRevoked = "Logged out";
            await db.SaveChangesAsync(ct);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal principal,
        AppDbContext db,
        CancellationToken ct)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "User ID not found in token",
                statusCode: 401
            );
        }

        var user = await db.Set<User>()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            return Results.Problem(
                title: "User not found",
                detail: "User no longer exists",
                statusCode: 401
            );
        }

        return Results.Ok(new UserResponse(
            Id: user.Id,
            Email: user.Email,
            Nombre: user.Nombre,
            Role: user.Role.ToString().ToLower(),
            OrganizationId: user.OrganizationId,
            LastLogin: user.LastLoginAt
        ));
    }
}
