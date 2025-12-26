using System.Security.Claims;
using CobranzaCloud.Application.Connectors;
using Microsoft.AspNetCore.Mvc;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Connector registration and management endpoints
/// </summary>
public static class ConnectorsEndpoints
{
    public static void MapConnectorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/connectors")
            .WithTags("Connectors")
            .WithOpenApi();

        // Requires authenticated user (generates code from web UI)
        group.MapPost("/link-code", GenerateLinkCode)
            .WithName("GenerateLinkCode")
            .WithDescription("Generate a 6-digit link code for connector registration")
            .RequireAuthorization()
            .Produces<LinkCodeResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        // Public endpoint (uses the link code)
        group.MapPost("/register", RegisterConnector)
            .WithName("RegisterConnector")
            .WithDescription("Register a new connector using a link code")
            .Produces<RegisterConnectorResponse>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(404);

        // Requires connector token
        group.MapPost("/heartbeat", Heartbeat)
            .WithName("ConnectorHeartbeat")
            .WithDescription("Record connector heartbeat")
            .RequireAuthorization()
            .Produces<HeartbeatResponse>(200)
            .Produces<ProblemDetails>(401);

        // Refresh connector token
        group.MapPost("/refresh", RefreshConnectorToken)
            .WithName("RefreshConnectorToken")
            .WithDescription("Refresh connector access token")
            .Produces<RegisterConnectorResponse>(200)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> GenerateLinkCode(
        [FromBody] GenerateLinkCodeRequest request,
        IConnectorService connectorService,
        ClaimsPrincipal principal,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var orgIdClaim = principal.FindFirst("org_id")?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "User ID not found in token",
                statusCode: 401
            );
        }

        if (orgIdClaim == null || !Guid.TryParse(orgIdClaim, out var orgId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Organization ID not found in token",
                statusCode: 401
            );
        }

        var response = await connectorService.GenerateLinkCodeAsync(orgId, userId, request, ct);

        logger.LogInformation(
            "Link code generated for org {OrgId} by user {UserId}: {Code}",
            orgId, userId, response.Code);

        return Results.Ok(response);
    }

    private static async Task<IResult> RegisterConnector(
        [FromBody] RegisterConnectorRequest request,
        IConnectorService connectorService,
        HttpContext httpContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var response = await connectorService.RegisterConnectorAsync(request, ipAddress, ct);

        if (response == null)
        {
            return Results.Problem(
                title: "Invalid link code",
                detail: "The link code is invalid, expired, or machine fingerprint doesn't match",
                statusCode: 404
            );
        }

        logger.LogInformation(
            "Connector registered: {ConnectorId} ({ConnectorName})",
            response.ConnectorId, request.ConnectorName);

        return Results.Created($"/api/connectors/{response.ConnectorId}", response);
    }

    private static async Task<IResult> Heartbeat(
        [FromBody] HeartbeatRequest request,
        IConnectorService connectorService,
        ClaimsPrincipal principal,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        // Verify this is a connector token
        var tokenType = principal.FindFirst("type")?.Value;
        if (tokenType != "connector")
        {
            return Results.Problem(
                title: "Invalid token type",
                detail: "This endpoint requires a connector token",
                statusCode: 401
            );
        }

        var connectorIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (connectorIdClaim == null || !Guid.TryParse(connectorIdClaim, out var connectorId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Connector ID not found in token",
                statusCode: 401
            );
        }

        var response = await connectorService.RecordHeartbeatAsync(connectorId, request, ct);

        logger.LogDebug("Heartbeat received from connector {ConnectorId}: {Status}", connectorId, request.Status);

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshConnectorToken(
        [FromBody] Application.Auth.RefreshTokenRequest request,
        IConnectorService connectorService,
        HttpContext httpContext,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var response = await connectorService.RefreshConnectorTokenAsync(request.RefreshToken, ipAddress, ct);

        if (response == null)
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Refresh token is invalid or expired",
                statusCode: 401
            );
        }

        logger.LogInformation("Connector token refreshed: {ConnectorId}", response.ConnectorId);

        return Results.Ok(response);
    }
}
