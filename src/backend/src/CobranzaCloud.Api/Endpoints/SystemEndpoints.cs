namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// System endpoints for health checks and API info
/// </summary>
public static class SystemEndpoints
{
    public static void MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/system")
            .WithTags("System");

        group.MapGet("/info", GetSystemInfo)
            .WithName("GetSystemInfo")
            .WithDescription("Returns API version and system information")
            .Produces<SystemInfoResponse>(200);

        group.MapGet("/health/detailed", GetDetailedHealth)
            .WithName("GetDetailedHealth")
            .WithDescription("Returns detailed health status of all services")
            .Produces<DetailedHealthResponse>(200);
    }

    private static IResult GetSystemInfo()
    {
        return Results.Ok(new SystemInfoResponse
        {
            Name = "CobranzaCloud API",
            Version = "0.1.0",
            DotNetVersion = Environment.Version.ToString(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow
        });
    }

    private static IResult GetDetailedHealth()
    {
        return Results.Ok(new DetailedHealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Services = new Dictionary<string, ServiceHealth>
            {
                ["api"] = new() { Status = "healthy", ResponseTime = 0 },
                ["database"] = new() { Status = "pending", ResponseTime = null },
                ["cache"] = new() { Status = "pending", ResponseTime = null }
            }
        });
    }
}

public record SystemInfoResponse
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string DotNetVersion { get; init; }
    public required string Environment { get; init; }
    public required DateTime Timestamp { get; init; }
}

public record DetailedHealthResponse
{
    public required string Status { get; init; }
    public required DateTime Timestamp { get; init; }
    public required Dictionary<string, ServiceHealth> Services { get; init; }
}

public record ServiceHealth
{
    public required string Status { get; init; }
    public int? ResponseTime { get; init; }
}
