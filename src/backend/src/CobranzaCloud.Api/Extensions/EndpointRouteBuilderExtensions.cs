using CobranzaCloud.Api.Endpoints;

namespace CobranzaCloud.Api.Extensions;

/// <summary>
/// Extension methods for IEndpointRouteBuilder to map all API endpoints
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        // System endpoints (health, info)
        app.MapSystemEndpoints();

        // Future endpoints will be added here:
        // app.MapAuthEndpoints();
        // app.MapCarteraEndpoints();
        // app.MapClientesEndpoints();
        // app.MapCobranzaEndpoints();
        // app.MapConnectorsEndpoints();
        // app.MapSyncEndpoints();

        return app;
    }
}
